using System;

namespace TimeMachineServer
{
    public class RecordDetail
    {
        public string AssetCode { get; set; }
        public double RatingBalance { get; set; }
        public double Return { get; set; }
        public double CumulativeReturn { set; get; }
    }

    public class Record
    {
        public DateTime Date { get; set; }
        public double TotalBalance { get; set; }
        public double Balance { get; set; }
        public double RatingBalance { get; set; }
        public double Return { get; set; }
        public double CumulativeReturn { set; get; }
        public double ReturnRatio { get; set; }
        public double CumulativeReturnRatio { set; get; }
        public double VolatilityRatio { get; set; }
        public double Mdd { get; set; }
        public double Max { get; set; }
    }
}
