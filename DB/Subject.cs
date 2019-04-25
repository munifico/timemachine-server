using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TimeMachineServer.DB
{
    [Table("japan_universe")]
    public class Subject : IEquatable<Subject>
    {
        #region Table Mapping
        [Column("asset_code")]
        public string AssetCode { get; set; }

        [Column("asset_name")]
        public string AssetName { get; set; }

        [Column("exchange")]
        public string Exchange { get; set; }

        [Column("gics_sector")]
        public string Sector { get; set; }

        [Column("gics_industry")]
        public string Industry { get; set; }

        [Column("market_cap")]
        public double MarketCap { get; set; }

        [Column("per")]
        public double? Per { get; set; }

        [Column("pbr")]
        public double? Pbr { get; set; }

        [Column("ev_evitda")]
        public double? EvEvitda { get; set; }

        [Column("div_yield")]
        public double? DivYield { get; set; }

        [Column("recent_volatility_1_year")]
        public double? RecentVolatility1Year { get; set; }

        [Column("recent_volatility_3_year")]
        public double? RecentVolatility3Year { get; set; }

        [Column("recent_volatility_5_year")]
        public double? RecentVolatility5Year { get; set; }

        [Column("recent_volatility_7_year")]
        public double? RecentVolatility7Year { get; set; }

        [Column("recent_volatility_10_year")]
        public double? RecentVolatility10Year { get; set; }

        #endregion

        #region Data Grid
        [NotMapped]
        public bool IsBenchmark { get; set; } = false;

        [NotMapped]
        public double Volume { get; set; }

        [NotMapped]
        public double Ratio { get; set; }

        [NotMapped]
        [Display(Description = "open price of trading first day")]
        public double Price { get; set; }
        #endregion

        #region Equal
        public override bool Equals(object obj)
        {
            return Equals(obj as Subject);
        }

        public bool Equals(Subject other)
        {
            return (AssetCode == other.AssetCode);
        }

        public override int GetHashCode()
        {
            return AssetName.GetHashCode();
        }
        #endregion
    }
}
