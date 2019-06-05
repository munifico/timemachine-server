using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using TimeMachineServer;
using TimeMachineServer.DB;
using static TimemachineServer.ReqAnalyzePortfolio;
using static TimeMachineServer.Constants;

namespace TimemachineServer
{
    public class Analyzer
    {
        public List<Report> AnalyzePortfolio(ReqAnalyzePortfolio request, bool analyzeBenchmark = true)
        {
            if (request.Country == null)
            {
                request.Country = "JP";
            }

            var startDate = DateTime.ParseExact(request.StartDate, "yyyy-MM-dd", CultureInfo.InvariantCulture);
            var endDate = DateTime.ParseExact(request.EndDate, "yyyy-MM-dd", CultureInfo.InvariantCulture);

            BacktestingProperty property = new BacktestingProperty()
            {
                Start = startDate,
                End = endDate,
                Capital = request.Capital,
                BenchmarkType = BenchmarkType.Nikkei225,
                CommissionType = request.CommissionType == "Ratio" ? CommissionType.Ratio : CommissionType.Fixed,
                Commission = request.Commission,
                SlippageType = request.SlippageType == "Ratio" ? SlippageType.Ratio : SlippageType.Fixed,
                Slippage = request.Slippage,
                BuyAndHold = true,
                VolatilityBreakout = true,
                MovingAverage = true,
                UseOutstandingBalance = request.AllowLeverage,
                UsePointVolume = request.AllowDecimalPoint,
                TradeType = request.OrderVolumeType == "Ratio" ? TradeType.Ratio : TradeType.Fixed
            };

            var strategies = new Dictionary<StrategyType, StrategyBase>();

            if (request.UseBuyAndHold)
            {
                strategies.Add(StrategyType.BuyAndHold, new BuyAndHold());
            }
            if (request.UseVolatilityBreakout)
            {
                strategies.Add(StrategyType.VolatilityBreakout, new VolatilityBreakout());
            }
            if (request.UseMovingAverage)
            {
                strategies.Add(StrategyType.MovingAverage, new MovingAverage());
            }

            // 각 종목별 OHLC 데이터
            var portfolioDataset = new Dictionary<string, Dictionary<DateTime, ITradingData>>();
            foreach (var subject in request.Portfolio)
            {
                var tradingDataset = new Dictionary<DateTime, ITradingData>();
                using (var context = new QTContext())
                {
                    if (request.Country == "FX")
                    {
                        var fx = context.FX.Where(x => x.AssetCode == subject.AssetCode &&
                            x.CreatedAt >= startDate && x.CreatedAt <= endDate).ToList();

                        var fxCopy = fx.Select(x => new Stock
                        {
                            CreatedAt = x.CreatedAt,
                            AssetCode = x.AssetCode,
                            Close = x.Close,
                            Open = x.Open,
                            High = x.High,
                            Low = x.Low,
                            Volume = 0
                        }).ToList();

                        fxCopy.ForEach(x => tradingDataset.Add(x.CreatedAt, x));
                    }
                    else if (request.Country == "JP")
                    {
                        // 원본
                        var stock = context.Stocks.Where(x => x.AssetCode == subject.AssetCode &&
                            x.CreatedAt >= startDate && x.CreatedAt <= endDate).ToList();

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

                        // 분할정보
                        var splits = context.Splits.Where(x => x.AssetCode == subject.AssetCode).ToList();

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

                        stockCopy.ForEach(x => tradingDataset.Add(x.CreatedAt, x));
                    }
                    else if (request.Country == "KR")
                    {
                        var stock = context.KoreaStocks.Where(x => x.AssetCode == subject.AssetCode &&
                            x.CreatedAt >= startDate && x.CreatedAt <= endDate).ToList();

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

                        stockCopy.ForEach(x => tradingDataset.Add(x.CreatedAt, x));
                    }
                }

                portfolioDataset.Add(subject.AssetCode, tradingDataset);
            }

            var tradingCalendar = CreateCalendar(startDate, endDate, request.Country);
            var reports = new List<Report>();

            // benchmark
            if (analyzeBenchmark)
            {
                RunBenchmark(request, property, startDate, endDate, tradingCalendar, reports);
            }

            // portfolio
            var portfolio = request.Portfolio.ToDictionary(x => x.AssetCode, x => x);
            foreach (var strategy in strategies.Values)
            {
                var report = strategy.Run(portfolioDataset, tradingCalendar, property, portfolio);
                reports.Add(report);
            }

            return reports;
        }

        private void RunBenchmark(ReqAnalyzePortfolio request, BacktestingProperty property, DateTime startDate, DateTime endDate, List<DateTime> tradingCalendar, List<Report> reports)
        {
            var simulator = new Simulator();
            using (var context = new QTContext())
            {
                var assetCode = request.Benchmark.AssetCode;
                var tradingDataset = new Dictionary<DateTime, ITradingData>();
                var benchmarkDataset = new Dictionary<string, Dictionary<DateTime, ITradingData>>();

                if (request.Country == "JP")
                {
                    var index = context.Indices.Where(x => x.AssetCode == assetCode &&
                                        x.CreatedAt >= startDate && x.CreatedAt <= endDate);
                    if (0 < index.Count())
                    {
                        index.ToList().ForEach(x => tradingDataset.Add(x.CreatedAt, x));
                    }
                }
                else if (request.Country == "KR")
                {
                    var index = context.KoreaIndices.Where(x => x.AssetCode == assetCode &&
                                                            x.CreatedAt >= startDate && x.CreatedAt <= endDate);

                    if (0 < index.Count())
                    {
                        index.ToList().ForEach(x => tradingDataset.Add(x.CreatedAt, x));
                    }
                }

                benchmarkDataset.Add(assetCode, tradingDataset);

                var strategy = new BuyAndHold();
                var benchmark = new Dictionary<string, PortfolioSubject>();
                benchmark.Add(assetCode, request.Benchmark);

                var report = strategy.Run(benchmarkDataset, tradingCalendar, property, benchmark, isBenchmark: true);
                reports.Add(report);
            }
        }

        private List<DateTime> CreateCalendar(DateTime start, DateTime end, string country)
        {
            var tradingCalendar = new List<DateTime>();
            using (var context = new QTContext())
            {
                if (country == "FX")
                {
                    tradingCalendar = context.TradingCalendars.Where(x => x.TradingDate >= start && x.TradingDate <= end && x.IsoCode == country).Select(x => x.TradingDate).ToList();
                }
                else
                {
                    tradingCalendar = context.TradingCalendars.Where(x => x.TradingDate >= start && x.TradingDate <= end && x.Country == country && x.IsoCode != "FX").Select(x => x.TradingDate).ToList();
                }
            }

            return tradingCalendar;
        }
    }
}