using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace TimeMachineServer.DB
{
    [Table("fx_1d")]
    public partial class FX : ITradingData
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
