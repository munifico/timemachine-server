using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace TimeMachineServer.DB
{
    /// <summary>
    /// 지수
    /// </summary>
    [Table("indices")]
    public class Index : ITradingData
    {
        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Column("asset_code")]
        public string AssetCode { get; set; }

        [Column("asset_name")]
        public string AssetName { get; set; }

        [Column("close")]
        public double Close { get; set; }

        [Column("open")]
        public double Open { get; set; }

        [Column("high")]
        public double High { get; set; }

        [Column("low")]
        public double Low { get; set; }

        [NotMapped]
        [Column("volume")]
        public double Volume { get; set; }
    }

    [Table("kr_indices")]
    public class KoreaIndex : ITradingData
    {
        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Column("asset_code")]
        public string AssetCode { get; set; }

        [Column("close")]
        public double Close { get; set; }

        [Column("open")]
        public double Open { get; set; }

        [Column("high")]
        public double High { get; set; }

        [Column("low")]
        public double Low { get; set; }

        [NotMapped]
        [Column("volume")]
        public double Volume { get; set; }
    }
}
