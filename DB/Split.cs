using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace TimeMachineServer.DB
{
    [Table("split_info")]
    public partial class Split
    {
        [Column("asset_code")]
        public string AssetCode { get; set; }

        [Column("split_date")]
        public DateTime SplitDate { get; set; }

        [Column("split_ratio")]
        public float SplitRatio { get; set; }
    }
}
