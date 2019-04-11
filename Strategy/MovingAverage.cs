using System;
using System.Collections.Generic;
using static TimemachineServer.ReqAnalyzePortfolio;
using static TimeMachineServer.Constants;

namespace TimeMachineServer
{
    public class MovingAverage : StrategyBase
    {
        public MovingAverage()
            : base(StrategyType.MovingAverage)
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
            if (_simulator.GetTradingIndex(assetCode) < 60)
            {
                return;
            }

            var shortMovingAverage = _simulator.GetMovingAverage(assetCode, 20, PriceType.Close);
            var longMovingAverage = _simulator.GetMovingAverage(assetCode, 60, PriceType.Close);

            var price = _simulator.GetPrice(assetCode, PriceType.Close, 0);

            if (shortMovingAverage > longMovingAverage)
            {
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
            else if (shortMovingAverage < longMovingAverage)
            {
                var volume = _simulator.GetVolume(assetCode);

                if (0 < volume)
                {
                    _simulator.LimitOrder(assetCode, OrderType.Sell, price, volume);
                }
            }
        }
    }
}
