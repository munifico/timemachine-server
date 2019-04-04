using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using static TimeMachineServer.Constants;

namespace TimeMachineServer
{
    public class BacktestingProperty
    {
        [Category("기간설정")]
        [DisplayName("시작날짜")]
        [Description("Start date of backtesting.")]
        public DateTime Start { get; set; } = new DateTime(2015, 1, 1);

        [Category("기간설정")]
        [DisplayName("종료날짜")]
        [Description("End date of backtesting.")]
        public DateTime End { get; set; } = new DateTime(2015, 1, 1).AddYears(4);

        [Category("거래설정")]
        [DisplayName("자산")]
        [Description("The capital of trading.")]
        [DisplayFormat(DataFormatString = "#,#0")]
        public double Capital { get; set; } = 100000;

        [Category("벤치마크")]
        [DisplayName("인덱스")]
        public BenchmarkType BenchmarkType { get; set; }

        [Category("거래설정")]
        [DisplayName("수수료 타입")]
        public CommissionType CommissionType { get; set; }

        [Category("거래설정")]
        [DisplayName("수수료")]
        public double Commission { get; set; }

        [Category("거래설정")]
        [DisplayName("슬리피지 타입")]
        public SlippageType SlippageType { get; set; } = 0.0;

        [Category("거래설정")]
        [DisplayName("슬리피지")]
        public double Slippage { get; set; } = 0.0;

        [Category("전략설정")]
        [DisplayName("바이앤홀드")]
        [Strategy]
        public bool BuyAndHold { get; set; } = true;

        [Category("전략설정")]
        [DisplayName("변동성돌파")]
        [Strategy]
        public bool VolatilityBreakout { get; set; } = true;

        [Category("전략설정")]
        [DisplayName("이평선돌파")]
        [Strategy]
        public bool MovingAverage { get; set; } = true;

        [Category("거래옵션")]
        [DisplayName("미수금 사용")]
        public bool UseOutstandingBalance { get; set; } = true;

        [Category("거래옵션")]
        [DisplayName("1주미만 거래 허용")]
        [Description("1주미만의 거래를 사용 할 수 있도록 허용 합니다.")]
        public bool UsePointVolume { get; set; } = false;

        [Category("거래옵션")]
        [DisplayName("주문수량")]
        public TradeType TradeType { get; set; }
    }
}
