using System;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using TimeMachineServer.Helper;

namespace TimeMachineServer
{
    public class Trend
    {
        public string AssetCode { get; set; }
        public string AssetName { get; set; }
        public string Exchange { get; set; }
        public string Sector { get; set; }
        public string Industry { get; set; }
        public double MarketCap { get; set; }

        [DataType(DataType.Date)]
        [JsonConverter(typeof(JsonDateConverter))]
        public DateTime FirstDate { get; set; }

        public double InitialBalance { get; set; }
        public double EndBalance { get; set; }
        public double Commission { get; set; }
        public double PeriodReturnRatio { get; set; }
        public double AnnualizedReturnRatio { get; set; }
        public double VolatilityRatio { get; set; }
        public double MddRatio { get; set; }
        public double SharpeRatio { get; set; }
        public double? Per { get; set; }
        public double? Pbr { get; set; }
        public double? EvEvitda { get; set; }
        public double? DivYield { get; set; }
    }

    public class Summary
    {
        public ObservableCollection<SummaryDetail> SummaryDetails { get; set; } = new ObservableCollection<SummaryDetail>();

        public string RelationalKey { get; set; }
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
        public string RelationalKey { get; set; }
        public string AssetName { get; set; }
        public string AssetCode { get; set; }
        public double InitialBalance { get; set; }
        public double EndBalance { get; set; }
        public double Commission { get; set; }
        public double PeriodReturnRatio { get; set; }
        public double AnnualizedReturnRatio { get; set; }
        public double VolatilityRatio { get; set; }
        public double MddRatio { get; set; }
        public double SharpeRatio { get; set; }
    }
}
