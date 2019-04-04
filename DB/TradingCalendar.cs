using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace TimeMachineServer.DB
{
    [Table("trading_calendar")]
    public class TradingCalendar
    {
        [Column("trading_date")]
        public DateTime TradingDate { get; set; }

        [Column("iso_code")]
        public string IsoCode { get; set; }

        [Column("country")]
        public string Country { get; set; }
    }
}