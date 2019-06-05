using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TimeMachineServer;
using TimeMachineServer.DB;
using static TimemachineServer.ReqAnalyzePortfolio;

namespace TimemachineServer.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        private readonly bool _analyzing = false;
        private readonly object _sync = new object();

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

        [HttpGet("{country}/{exchange?}")]
        public ActionResult<List<Subject>> Universe(string country, string exchange)
        {
            return UniverseManager.Instance.GetUniverse(country, exchange);
        }

        [HttpPost]
        public ActionResult<ResOpenPrice> OpenPrice([FromBody] ReqOpenPrice request)
        {
            if (!Validate(request.StartDate))
            {
                return null;
            }

            var date = DateTime.ParseExact(request.StartDate, "yyyy-MM-dd", CultureInfo.InvariantCulture);

            using (var context = new QTContext())
            {
                var response = new ResOpenPrice();

                foreach (var asset in request.Assets)
                {
                    if (asset.AssetCode == "JP225")
                    {
                        var index = context.Indices
                                            .Where(x => x.CreatedAt >= date && x.AssetCode == asset.AssetCode)
                                            .OrderBy(x => x.CreatedAt)
                                            .Take(10) // date가 실제 트레이딩 날짜가 아닐 수 있기 때문에 최대 10일 뒤의 데이터를 가져온다.(10일간 거래를 안할 수 없다는 가정)
                                            .FirstOrDefault();
                        var indexCopy = new Index()
                        {
                            CreatedAt = index.CreatedAt,
                            AssetCode = index.AssetCode,
                            AssetName = index.AssetName,
                            Close = index.Close,
                            Open = index.Open,
                            High = index.High,
                            Low = index.Low,
                            Volume = index.Volume
                        };

                        // 분할정보
                        var splits = context.Splits.Where(x => x.AssetCode == asset.AssetCode).ToList();

                        // 분할적용
                        splits.ForEach(split =>
                        {
                            if (indexCopy.CreatedAt < split.SplitDate)
                            {
                                indexCopy.Open = indexCopy.Open / split.SplitRatio;
                                indexCopy.High = indexCopy.High / split.SplitRatio;
                                indexCopy.Low = indexCopy.Low / split.SplitRatio;
                                indexCopy.Close = indexCopy.Close / split.SplitRatio;
                            }
                        });

                        response.Data.Add(new ResOpenPrice.Context
                        {
                            AssetCode = asset.AssetCode,
                            AssetName = AssetManager.Instance.GetAssetName(asset.AssetCode),
                            Exchange = asset.Exchange,
                            Date = indexCopy.CreatedAt,
                            OpenPrice = indexCopy.Open,
                        });
                    }
                    else if (asset.AssetCode == "KOSPI")
                    {
                        var index = context.KoreaIndices
                            .Where(x => x.CreatedAt >= date && x.AssetCode == asset.AssetCode)
                            .OrderBy(x => x.CreatedAt)
                            .Take(10) // date가 실제 트레이딩 날짜가 아닐 수 있기 때문에 최대 10일 뒤의 데이터를 가져온다.(10일간 거래를 안할 수 없다는 가정)
                            .FirstOrDefault();

                        var indexCopy = new KoreaIndex()
                        {
                            CreatedAt = index.CreatedAt,
                            AssetCode = index.AssetCode,
                            Close = index.Close,
                            Open = index.Open,
                            High = index.High,
                            Low = index.Low,
                            Volume = index.Volume
                        };

                        response.Data.Add(new ResOpenPrice.Context
                        {
                            AssetCode = asset.AssetCode,
                            AssetName = AssetManager.Instance.GetAssetName(asset.AssetCode),
                            Exchange = asset.Exchange,
                            Date = indexCopy.CreatedAt,
                            OpenPrice = indexCopy.Open,
                        });
                    }
                    else if (asset.Exchange == "TSE" || asset.Exchange == "ETF" || asset.Exchange == "NIKKEI")
                    {
                        var stock = context.Stocks
                                            .Where(x => x.CreatedAt >= date && x.AssetCode == asset.AssetCode)
                                            .OrderBy(x => x.CreatedAt)
                                            .Take(10) // date가 실제 트레이딩 날짜가 아닐 수 있기 때문에 최대 10일 뒤의 데이터를 가져온다.(10일간 거래를 안할 수 없다는 가정)
                                            .FirstOrDefault();
                        var stockCopy = new Stock
                        {
                            CreatedAt = stock.CreatedAt,
                            AssetCode = stock.AssetCode,
                            Close = stock.Close,
                            Open = stock.Open,
                            High = stock.High,
                            Low = stock.Low,
                            Volume = stock.Volume
                        };

                        // 분할정보
                        var splits = context.Splits.Where(x => x.AssetCode == asset.AssetCode).ToList();

                        // 분할적용
                        splits.ForEach(split =>
                        {
                            if (stockCopy.CreatedAt < split.SplitDate)
                            {
                                stockCopy.Open = stockCopy.Open / split.SplitRatio;
                                stockCopy.High = stockCopy.High / split.SplitRatio;
                                stockCopy.Low = stockCopy.Low / split.SplitRatio;
                                stockCopy.Close = stockCopy.Close / split.SplitRatio;
                            }
                        });

                        response.Data.Add(new ResOpenPrice.Context
                        {
                            AssetCode = asset.AssetCode,
                            AssetName = AssetManager.Instance.GetAssetName(asset.AssetCode),
                            Exchange = asset.Exchange,
                            Date = stockCopy.CreatedAt,
                            OpenPrice = stockCopy.Open,
                        });
                    }
                    else if (asset.Exchange == "KOSPI" || asset.Exchange == "KOSDAQ")
                    {
                        var stock = context.KoreaStocks
                                            .Where(x => x.CreatedAt >= date && x.AssetCode == asset.AssetCode)
                                            .OrderBy(x => x.CreatedAt)
                                            .Take(10) // date가 실제 트레이딩 날짜가 아닐 수 있기 때문에 최대 10일 뒤의 데이터를 가져온다.(10일간 거래를 안할 수 없다는 가정)
                                            .FirstOrDefault();
                        var stockCopy = new KoreaStock
                        {
                            CreatedAt = stock.CreatedAt,
                            AssetCode = stock.AssetCode,
                            Close = stock.Close,
                            Open = stock.Open,
                            High = stock.High,
                            Low = stock.Low,
                            Volume = stock.Volume
                        };

                        response.Data.Add(new ResOpenPrice.Context
                        {
                            AssetCode = asset.AssetCode,
                            AssetName = AssetManager.Instance.GetAssetName(asset.AssetCode),
                            Exchange = asset.Exchange,
                            Date = stockCopy.CreatedAt,
                            OpenPrice = stockCopy.Open,
                        });
                    }
                    else if (asset.Exchange == "FX")
                    {
                        var fx = context.FX
                                            .Where(x => x.CreatedAt >= date && x.AssetCode == asset.AssetCode)
                                            .OrderBy(x => x.CreatedAt)
                                            .Take(10) // date가 실제 트레이딩 날짜가 아닐 수 있기 때문에 최대 10일 뒤의 데이터를 가져온다.(10일간 거래를 안할 수 없다는 가정)
                                            .FirstOrDefault();
                        var fxCopy = new FX
                        {
                            CreatedAt = fx.CreatedAt,
                            AssetCode = fx.AssetCode,
                            Close = fx.Close,
                            Open = fx.Open,
                            High = fx.High,
                            Low = fx.Low,
                            Volume = 0
                        };

                        response.Data.Add(new ResOpenPrice.Context
                        {
                            AssetCode = asset.AssetCode,
                            AssetName = AssetManager.Instance.GetAssetName(asset.AssetCode),
                            Exchange = asset.Exchange,
                            Date = fxCopy.CreatedAt,
                            OpenPrice = fxCopy.Open,
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
            return await Task.Run(async () =>
            {
                var requestTime = DateTime.Now;

                var analyzer = new Analyzer();
                var reports = new Dictionary<string, List<Trend>>(); // key: StrategyType
                var completed = new List<string>();

                var universe = UniverseManager.Instance.GetUniverse("JP", null);

                var tasks = new List<Task>();

                foreach (var subject in universe)
                {
                    var task = Task.Run(() =>
                    {
                        var requestCopy = request.Clone();
                        requestCopy.Portfolio = new List<PortfolioSubject>
                        {
                            new PortfolioSubject()
                            {
                                AssetCode = subject.AssetCode,
                                Volume = 1,
                                Ratio = 1,
                            }
                        };

                        foreach (var report in analyzer.AnalyzePortfolio(requestCopy, analyzeBenchmark: false))
                        {
                            var strategyType = report.StrategyType;

                            var trend = new Trend()
                            {
                                AssetCode = subject.AssetCode,
                                AssetName = subject.AssetName,
                                Exchange = subject.Exchange,
                                Sector = subject.Sector,
                                Industry = subject.Industry,
                                MarketCap = subject.MarketCap,
                                FirstDate = subject.FirstDate,
                                InitialBalance = report.Summary.InitialBalance,
                                EndBalance = report.Summary.EndBalance,
                                Commission = report.Summary.Commission,
                                Transactions = report.Transactions[subject.AssetCode].Count,
                                PeriodReturnRatio = report.Summary.PeriodReturnRatio,
                                AnnualizedReturnRatio = report.Summary.AnnualizedReturnRatio,
                                VolatilityRatio = report.Summary.VolatilityRatio,
                                PriceVolatilityRatio = report.Summary.PriceVolatilityRatio,
                                MddRatio = report.Summary.MddRatio,
                                SharpeRatio = report.Summary.SharpeRatio,
                                Per = UniverseManager.Instance.FindUniverse(subject.AssetCode).Per,
                                Pbr = UniverseManager.Instance.FindUniverse(subject.AssetCode).Pbr,
                                EvEvitda = UniverseManager.Instance.FindUniverse(subject.AssetCode).EvEvitda,
                                DivYield = UniverseManager.Instance.FindUniverse(subject.AssetCode).DivYield
                            };

                            lock (_sync)
                            {
                                if (reports.ContainsKey(strategyType))
                                {
                                    reports[strategyType].Add(trend);
                                }
                                else
                                {
                                    reports.Add(strategyType, new List<Trend> { trend });
                                }
                            }
                        }

                        Console.WriteLine($"[{requestTime.ToShortTimeString()}] {subject.AssetCode}");
                    });

                    tasks.Add(task);
                }

                Console.WriteLine("Wait tasks...");
                await Task.WhenAll(tasks);
                Console.WriteLine($"Start - {requestTime.ToShortTimeString()} End - {DateTime.Now.ToShortTimeString()}");

                return reports;
            });
        }

        private bool Validate(string date)
        {
            return DateTime.TryParseExact(date, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime temp);
        }
    }
}
