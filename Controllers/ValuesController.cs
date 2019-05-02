﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TimeMachineServer;
using TimeMachineServer.DB;
using static TimemachineServer.ReqAnalyzePortfolio;
using static TimeMachineServer.Constants;

namespace TimemachineServer.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        private bool _analyzing = false;

        // GET api/values
        [HttpGet]
        public ActionResult<IEnumerable<string>> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public ActionResult<string> Get(int id)
        {
            return "value";
        }

        // POST api/values
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }

        [HttpGet]
        public ActionResult<List<Subject>> Universe(string exchange)
        {
            return UniverseManager.Instance.GetUniverse(exchange);
        }

        [HttpPost]
        public ActionResult<ResOpenPrice> OpenPrice([FromBody] ReqOpenPrice request)
        {
            var date = DateTime.ParseExact(request.StartDate, "yyyy-MM-dd", CultureInfo.InvariantCulture);

            using (var context = new QTContext())
            {
                var response = new ResOpenPrice();

                foreach (var asset in request.Assets)
                {
                    if (asset.Exchange == "INDEX")
                    {
                        var index = context.Indices
                                            .Where(x => x.CreatedAt >= date && x.AssetCode == asset.AssetCode)
                                            .OrderBy(x => x.CreatedAt)
                                            .Take(10) // date가 실제 트레이딩 날짜가 아닐 수 있기 때문에 최대 10일 뒤의 데이터를 가져온다.(10일간 거래를 안할 수 없다는 가정)
                                            .FirstOrDefault();

                        response.Data.Add(new ResOpenPrice.Context
                        {
                            AssetCode = asset.AssetCode,
                            AssetName = AssetManager.Instance.GetAssetName(asset.AssetCode),
                            Exchange = asset.Exchange,
                            OpenPrice = index.Open,
                        });
                    }
                    // else if (asset.Exchange != "ETF")
                    else
                    {
                        var stock = context.Stocks
                                            .Where(x => x.CreatedAt >= date && x.AssetCode == asset.AssetCode)
                                            .OrderBy(x => x.CreatedAt)
                                            .Take(10) // date가 실제 트레이딩 날짜가 아닐 수 있기 때문에 최대 10일 뒤의 데이터를 가져온다.(10일간 거래를 안할 수 없다는 가정)
                                            .FirstOrDefault();

                        response.Data.Add(new ResOpenPrice.Context
                        {
                            AssetCode = asset.AssetCode,
                            AssetName = AssetManager.Instance.GetAssetName(asset.AssetCode),
                            Exchange = asset.Exchange,
                            OpenPrice = stock.Open,
                        });
                    }
                }

                return response;
            }
        }

        [HttpPost]
        public ActionResult<List<Report>> AnalyzePortfolio([FromBody] ReqAnalyzePortfolio request)
        {
            var analyzer = new Analyzer();
            return analyzer.AnalyzePortfolio(request);
        }

        [HttpPost]
        public async Task<ActionResult<Dictionary<string, List<Trend>>>> AnalyzeAllPortfolio([FromBody] ReqAnalyzePortfolio request)
        {
            return await Task.Run(() =>
            {
                var analyzer = new Analyzer();
                var reports = new Dictionary<string, List<Trend>>(); // key: StrategyType

                foreach (var subject in UniverseManager.Instance.GetUniverse(null))
                {
                    request.Portfolio = new List<PortfolioSubject>();
                    request.Portfolio.Add(new PortfolioSubject()
                    {
                        AssetCode = subject.AssetCode,
                        Volume = 1,
                        Ratio = 1,
                    });

                    foreach (var report in analyzer.AnalyzePortfolio(request, analyzeBenchmark: false))
                    {
                        var strategyType = report.StrategyType;

                        var trend = new Trend()
                        {
                            AssetCode = subject.AssetCode,
                            AssetName = subject.AssetName,
                            Exchange = subject.Exchange,
                            InitialBalance = report.Summary.InitialBalance,
                            EndBalance = report.Summary.EndBalance,
                            Commission = report.Summary.Commission,
                            PeriodReturnRatio = report.Summary.PeriodReturnRatio,
                            AnnualizedReturnRatio = report.Summary.AnnualizedReturnRatio,
                            VolatilityRatio = report.Summary.VolatilityRatio,
                            MddRatio = report.Summary.MddRatio,
                            SharpeRatio = report.Summary.SharpeRatio,
                            Per = UniverseManager.Instance.FindUniverse(subject.AssetCode).Per,
                            Pbr = UniverseManager.Instance.FindUniverse(subject.AssetCode).Pbr,
                            EvEvitda = UniverseManager.Instance.FindUniverse(subject.AssetCode).EvEvitda,
                            DivYield = UniverseManager.Instance.FindUniverse(subject.AssetCode).DivYield
                        };

                        if (reports.ContainsKey(strategyType))
                        {
                            reports[strategyType].Add(trend);
                        }
                        else
                        {
                            reports.Add(strategyType, new List<Trend> { trend });
                        }
                    }

                    Console.WriteLine(subject.AssetCode);
                }

                return reports;
            });
        }
    }
}
