using System;
using System.Collections.Generic;
using System.ComponentModel;
using static TimeMachineServer.Constants;

namespace TimeMachineServer
{
    public class VolatilityBreakout : StrategyBase
    {
        private YearOfWeek _yearOfWeek; // 매수한 week
        private double _high;
        private double _low;

        public VolatilityBreakout()
            : base(StrategyType.VolatilityBreakout)
        {
        }

        [Category("Parameter")]
        [Description("The average value of the noise ratio over the last 20 days")]
        public double K { get; set; } = 0.5;

        [Category("기간설정")]
        [DisplayName("거래간격")]
        public TradeTerm TradeTerm { get; set; } = TradeTerm.Daily;

        private void _Initialize()
        {
            _yearOfWeek = new YearOfWeek();
            _high = 0;
            _low = double.MaxValue;
        }

        public override Report Run(Dictionary<string, Dictionary<DateTime, ITradingData>> portfolioDataset,
          List<DateTime> tradingCalendar,
          BacktestingProperty property,
          bool isBenchmark = false)
        {
            _Initialize();
            _simulator = new Simulator();
            return _simulator.Run(this, portfolioDataset, tradingCalendar, property, isBenchmark);
        }

        public override void OnAfterOpen(string assetCode)
        {
            double prevHigh = 0;
            double prevLow = 0;

            if (TradeTerm == TradeTerm.Daily)
            {
                if (_simulator.IsFirstDate(assetCode))
                {
                    return;
                }

                prevHigh = _simulator.GetPrice(assetCode, PriceType.High, -1);
                prevLow = _simulator.GetPrice(assetCode, PriceType.Low, -1);
            }
            else if (TradeTerm == TradeTerm.Weekly)
            {
                _high = Math.Max(_high, _simulator.GetPrice(assetCode, PriceType.High, 0));
                _low = Math.Min(_low, _simulator.GetPrice(assetCode, PriceType.Low, 0));

                if (_simulator.IsFirstWeek(assetCode))
                {
                    return;
                }

                var currentWeek = _simulator.GetYearOfWeek();
                if (currentWeek <= _yearOfWeek)
                {
                    return; // 한 주가 안지남
                }

                prevHigh = _high;
                prevLow = _low;

                _high = 0;
                _low = double.MaxValue;
            }

            var curHigh = _simulator.GetPrice(assetCode, PriceType.High, 0);
            var curLow = _simulator.GetPrice(assetCode, PriceType.Low, 0);
            var openPrice = _simulator.GetPrice(assetCode, PriceType.Open, 0);
            var closePrice = _simulator.GetPrice(assetCode, PriceType.Close, 0);

            var volPrice = openPrice + (prevHigh - prevLow) * K;
            var volume = _simulator.GetVolume(assetCode);

            if (0 < volume)
            {
                _simulator.LimitOrder(assetCode, OrderType.Sell, openPrice, volume); // 보유물량 전량매도
            }

            // 마지막날은 매수하지 않음
            if (!_simulator.IsLastDate(assetCode))
            {
                if (volPrice > curLow && volPrice <= curHigh)
                {
                    // 돌파가격에 매수
                    switch (_simulator.Property.TradeType)
                    {
                        case TradeType.Fixed:
                            _simulator.LimitOrder(assetCode, OrderType.Buy, volPrice, PortfolioManager.Instance.GetSubject(assetCode).Volume);
                            break;
                        case TradeType.Ratio:
                            _simulator.LimitOrderPercent(assetCode, OrderType.Buy, volPrice, PortfolioManager.Instance.GetSubject(assetCode).Ratio);
                            break;
                    }

                    // 매수한 week update
                    _yearOfWeek = _simulator.GetYearOfWeek();
                }
            }
        }
    }
}
