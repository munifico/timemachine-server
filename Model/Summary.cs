using System.Collections.ObjectModel;

namespace TimeMachineServer
{
    public class Summary
    {
        public ObservableCollection<SummaryDetail> SummaryDetails { get; set; } = new ObservableCollection<SummaryDetail>();

        public string SubjectType { get; set; }
        public string StrategyType { get; set; }
        public double InitialBalance { get; set; }
        public double EndBalance { get; set; }
        public double Commission { get; set; }
        public double PeriodReturnRatio { get; set; }
        public double AnnualizedReturnRatio { get; set; }
        public double VolatilityRatio { get; set; }
        public double MddRatio { get; set; }
        public double SharpeRatio { get; set; }
    }

    public class SummaryDetail
    {
        public string AssetName { get; set; }
        public string AssetCode { get; set; }
        public double InitialBalance { get; set; }
        public double EndBalance { get; set; }
        public double Commission { get; set; }
        public double PeriodReturnRatio { get; set; }
        public double MddRatio { get; set; }
    }
}
