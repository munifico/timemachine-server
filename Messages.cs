using System.Collections.Generic;

namespace TimemachineServer
{
    public class ReqOpenPrice
    {
        public class AssetInfo
        {
            public string Exchange { get; set; }
            public string AssetCode { get; set; }
        }

        public List<AssetInfo> Assets { get; set; }
        public string StartDate { get; set; }
    }

    public class ResOpenPrice
    {
        public List<Context> Data { get; set; } = new List<Context>();

        public class Context
        {
            public string AssetCode { get; set; }
            public string AssetName { get; set; }
            public double OpenPrice { get; set; }
        }
    }

    public class ReqAnalyzePortfolio
    {
        public class Subject
        {
            public string AssetCode { get; set; }
            public double Ratio { get; set; }
        }

        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string Capital { get; set; }
        public string CommissionType { get; set; }
        public double Commission { get; set; }
        public string SlippageType { get; set; }
        public double Slippage { get; set; }
        public string OrderVolumeType { get; set; }
        public bool AllowDecimalPoint { get; set; }
        public bool AllowLeverage { get; set; }
        public List<Subject> Portfolio { get; set; }
    }
}
