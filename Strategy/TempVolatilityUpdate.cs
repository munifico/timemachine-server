using TimeMachineServer.DB;
using System.Linq;
using System.Collections.Generic;
using System;
using TimeMachineServer;
using Microsoft.EntityFrameworkCore;

namespace TimemachineServer
{
    class TempVolatilityUpdate
    {
        public void Run()
        {
            using (var context = new QTContext())
            {
                var universe = context.Universe.Where(x => int.Parse(x.AssetCode) < 2201).ToList();
                var sql = "";

                foreach (var subject in universe)
                {
                    var tradingDataset = new List<ITradingData>();

                    // 분할정보
                    var splits = context.Splits.Where(x => x.AssetCode == subject.AssetCode).ToList();

                    if (0 < splits.Count)
                    {
                        // 원본
                        var stock = context.Stocks.Where(x => x.AssetCode == subject.AssetCode).ToList();
                        var stockCopy = stock.Select(x => new Stock
                        {
                            CreatedAt = x.CreatedAt,
                            AssetCode = x.AssetCode,
                            Close = x.Close,
                            Open = x.Open,
                            High = x.High,
                            Low = x.Low,
                            Volume = x.Volume
                        }).ToList();

                        // 분할적용
                        splits.ForEach(split =>
                                    {
                                        foreach (var s in stockCopy.Where(x => x.CreatedAt < split.SplitDate))
                                        {
                                            s.Open = s.Open / split.SplitRatio;
                                            s.High = s.High / split.SplitRatio;
                                            s.Low = s.Low / split.SplitRatio;
                                            s.Close = s.Close / split.SplitRatio;
                                        }
                                    });

                        stockCopy.ForEach(x => tradingDataset.Add(x));

                        var year1 = tradingDataset.Where(x => x.CreatedAt >= DateTime.Now.AddYears(-1)).ToList();
                        var year3 = tradingDataset.Where(x => x.CreatedAt >= DateTime.Now.AddYears(-3)).ToList();
                        var year5 = tradingDataset.Where(x => x.CreatedAt >= DateTime.Now.AddYears(-5)).ToList();
                        var year7 = tradingDataset.Where(x => x.CreatedAt >= DateTime.Now.AddYears(-7)).ToList();
                        var year10 = tradingDataset.Where(x => x.CreatedAt >= DateTime.Now.AddYears(-10)).ToList();

                        var rate1 = (year1.Max(x => x.High) - year1.Min(x => x.Low)) / year1.Min(x => x.Low);
                        var rate3 = (year3.Max(x => x.High) - year3.Min(x => x.Low)) / year3.Min(x => x.Low);
                        var rate5 = (year5.Max(x => x.High) - year5.Min(x => x.Low)) / year5.Min(x => x.Low);
                        var rate7 = (year7.Max(x => x.High) - year7.Min(x => x.Low)) / year7.Min(x => x.Low);
                        var rate10 = (year10.Max(x => x.High) - year10.Min(x => x.Low)) / year10.Min(x => x.Low);

                        subject.RecentVolatility1Year = rate1;
                        subject.RecentVolatility3Year = rate3;
                        subject.RecentVolatility5Year = rate5;
                        subject.RecentVolatility7Year = rate7;
                        subject.RecentVolatility10Year = rate10;

                        sql += $@"UPDATE qt.japan_universe SET recent_volatility_1_year = {rate1},
                            recent_volatility_3_year = {rate3}, recent_volatility_5_year = {rate5}, recent_volatility_7_year = {rate7}, recent_volatility_10_year = {rate10} WHERE asset_code = {subject.AssetCode};";

                        Console.WriteLine($"{subject.AssetCode}");
                    }


                }
                context.Database.ExecuteSqlCommand(sql);
                // context.SaveChanges();
            }
        }
    }
}