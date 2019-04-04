using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace TimeMachineServer
{
    public static class Constants
    {
        public enum StrategyType
        {
            [Display(Name = "바이앤홀드")]
            BuyAndHold,

            [Display(Name = "변동성돌파")]
            VolatilityBreakout,

            [Display(Name = "이평선돌파")]
            MovingAverage,
        }

        public enum IndicatorType
        {
            DailyTotalBalance,      // 총평가금액
            DailyBalance,           // 잔고
            DailyRatingBalance,     // 평가잔고
            Mdd,
            DailyReturnRatio,
            CumulativeReturnRatio,
            DailyReturn,
            CumulativeReturn
        }

        public enum PriceType
        {
            Open,
            High,
            Low,
            Close
        }

        public enum OrderType
        {
            Sell,
            Buy
        }

        public enum TradeType
        {
            [Description("자산 대비 비율")]
            Ratio,

            [Description("고정 수량")]
            Fixed,
        }

        public enum CommissionType
        {
            // 매도금액의 일정 %
            [Description("매도금액비율")]
            Ratio,

            // 고정금액 수수료
            [Description("고정금액")]
            Fixed,
        }

        public enum SlippageType
        {
            [Description("매매금액비율")]
            Ratio,

            [Description("고정금액")]
            Fixed,
        }

        public enum BenchmarkType
        {
            Nikkei225
        }

        public enum LogLevel
        {
            Debug,
            Info,
            Warn,
            Error,
        }

        public enum TradeTerm
        {
            Daily,
            Weekly,
            Monthly,
        }
    }
}
