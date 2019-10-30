using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace TimeMachineServer.DB
{
    public class FX
    {
        public DateTime CreatedAt { get; set; }
        public string AssetCode { get; set; }
        public double Close { get; set; }
        public double Open { get; set; }
        public double High { get; set; }
        public double Low { get; set; }
        public double Volume { get; set; }
    }

    [Table("fx_1d")]
    public class FX1D : ITradingData
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
        public double Volume { get; set; }
    }

    [Table("fx_1w")]
    public class FX1W : ITradingData
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
        public double Volume { get; set; }
    }

    [Table("fx_60m")]
    public class FX60M : ITradingData
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
        public double Volume { get; set; }
    }
}
