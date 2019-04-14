using System;
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
        private BacktestingProperty _property;

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
                    if (asset.Exchange != "ETF")
                    {
                        var stock = context.Stocks
                                            .Where(x => x.CreatedAt >= date && x.AssetCode == asset.AssetCode)
                                            .OrderBy(x => x.CreatedAt)
                                            .Take(10) // date가 실제 트레이딩 날짜가 아닐 수 있기 때문에 최대 10일 뒤의 데이터를 가져온다.(10일간 거래를 안할 수 없다는 가정)
                                            .FirstOrDefault();

                        // var subject = new Subject()
                        // {
                        //     AssetCode = asset.AssetCode
                        // };
                        // PortfolioManager.Instance.AddToPortfolio(subject, date);

                        response.Data.Add(new ResOpenPrice.Context
                        {
                            AssetCode = asset.AssetCode,
                            AssetName = AssetManager.Instance.GetAssetName(asset.AssetCode),
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
            var startDate = DateTime.ParseExact(request.StartDate, "yyyy-MM-dd", CultureInfo.InvariantCulture);
            var endDate = DateTime.ParseExact(request.EndDate, "yyyy-MM-dd", CultureInfo.InvariantCulture);

            _property = new BacktestingProperty()
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

            if (request.UseBuyAndHold)
            {
                StrategyManager.Instance.AddStrategy(StrategyType.BuyAndHold, new BuyAndHold());
            }
            if (request.UseVolatilityBreakout)
            {
                StrategyManager.Instance.AddStrategy(StrategyType.VolatilityBreakout, new VolatilityBreakout());
            }
            if (request.UseMovingAverage)
            {
                StrategyManager.Instance.AddStrategy(StrategyType.MovingAverage, new MovingAverage());
            }

            // 각 종목별 OHLC 데이터
            var portfolioDataset = new Dictionary<string, Dictionary<DateTime, ITradingData>>();

            foreach (var subject in request.Portfolio)
            {
                var tradingDataset = new Dictionary<DateTime, ITradingData>();
                using (var context = new QTContext())
                {
                    var stock = context.Stocks.Where(x => x.AssetCode == subject.AssetCode &&
                        x.CreatedAt >= startDate && x.CreatedAt <= endDate).ToList();

                    stock.ForEach(x => tradingDataset.Add(x.CreatedAt, x));
                }

                portfolioDataset.Add(subject.AssetCode, tradingDataset);
            }

            var tradingCalendar = CreateCalendar(startDate, endDate);
            var reports = new List<Report>();

            if (request.Benchmark.AssetCode == "JP225")
            {
                // PortfolioManager.Instance.AddToBenchmark(_property.Start); // TODO:  Portfoliomanager 사용하면 안되고, request마다 새로 생성해야 한다.

                var simulator = new Simulator();
                using (var context = new QTContext())
                {
                    var assetCode = "JP225";
                    var tradingDataset = new Dictionary<DateTime, ITradingData>();
                    var benchmarkDataset = new Dictionary<string, Dictionary<DateTime, ITradingData>>();

                    var index = context.Indices.Where(x => x.AssetCode == assetCode &&
                        x.CreatedAt >= startDate && x.CreatedAt <= endDate).ToList();

                    index.ForEach(x => tradingDataset.Add(x.CreatedAt, x));
                    benchmarkDataset.Add(assetCode, tradingDataset);

                    var strategy = StrategyManager.Instance.GetStrategy(StrategyType.BuyAndHold);
                    if (strategy == null)
                    {
                        strategy = new BuyAndHold();
                    }

                    var benchmark = new Dictionary<string, PortfolioSubject>();
                    benchmark.Add("JP225", request.Benchmark);

                    var report = strategy.Run(benchmarkDataset, tradingCalendar, _property, benchmark, isBenchmark: true);
                    reports.Add(report);
                }
            }

            var portfolio = request.Portfolio.ToDictionary(x => x.AssetCode, x => x);

            StrategyManager.Instance.Run(reports, portfolioDataset, tradingCalendar, _property, portfolio);

            return reports;
        }

        private List<DateTime> CreateCalendar(DateTime start, DateTime end)
        {
            var tradingCalendar = new List<DateTime>();
            using (var context = new QTContext())
            {
                tradingCalendar = context.TradingCalendars.Where(x => x.TradingDate >= start && x.TradingDate <= end && x.IsoCode == "XTKS").Select(x => x.TradingDate).ToList();
            }

            return tradingCalendar;
        }
    }
}
