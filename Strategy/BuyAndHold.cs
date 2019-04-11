using System;
using System.Collections.Generic;
using static TimemachineServer.ReqAnalyzePortfolio;
using static TimeMachineServer.Constants;

namespace TimeMachineServer
{
    public class BuyAndHold : StrategyBase
    {
        public BuyAndHold()
            : base(StrategyType.BuyAndHold)
        {
        }

        public override Report Run(Dictionary<string, Dictionary<DateTime, ITradingData>> portfolioDataset,
            List<DateTime> tradingCalendar,
            BacktestingProperty property,
            Dictionary<string, PortfolioSubject> portfolio,
            bool isBenchmark = false)
        {
            _simulator = new Simulator();
            return _simulator.Run(this, portfolioDataset, tradingCalendar, property, portfolio, isBenchmark);
        }

        public override void OnAfterOpen(string assetCode)
        {
            var volume = _simulator.GetVolume(assetCode);

            if (0 >= volume)
            {
                var price = _simulator.GetPrice(assetCode, PriceType.Open, 0);

                switch (_simulator.Property.TradeType)
                {
                    case TradeType.Fixed:
                        _simulator.LimitOrder(assetCode, OrderType.Buy, price, _simulator.GetSubject(assetCode).Volume);
                        break;
                    case TradeType.Ratio:
                        _simulator.LimitOrderPercent(assetCode, OrderType.Buy, price, _simulator.GetSubject(assetCode).Ratio);
                        break;
                }
            }

            //// 마지막날 종가 매도
            //if (_simulator.IsLastDate(assetCode))
            //{
            //    var price = _simulator.GetPrice(assetCode, PriceType.Close, 0);
            //    _simulator.LimitOrder(assetCode, OrderType.Sell, price, volume);
            //}
        }
    }
}
