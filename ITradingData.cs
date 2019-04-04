using System;

namespace TimeMachineServer
{
    public interface ITradingData
    {
        DateTime CreatedAt { get; set; }
        string AssetCode { get; set; }
        double Close { get; set; }
        double Open { get; set; }
        double High { get; set; }
        double Low { get; set; }
        double Volume { get; set; }
    }
}
