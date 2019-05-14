using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace TimeMachineServer.DB
{
    [Table("stocks")]
    public partial class Stock : ITradingData
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

        [Column("volume")]
        public double Volume { get; set; }
    }

    [Table("kr_stocks")]
    public partial class KoreaStock : ITradingData
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

        [Column("volume")]
        public double Volume { get; set; }
    }
}
