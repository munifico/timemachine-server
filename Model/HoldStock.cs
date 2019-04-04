using static TimeMachineServer.Constants;

namespace TimeMachineServer
{
    public class HoldStock
    {
        private double _value = 0.0;

        public double InitialBalance { get; set; }
        public double Volume { get; set; }
        public double HighestRatingBalance { get; set; }
        public double Mdd { get; set; }

        public void Update(OrderType orderType, double price, double volume)
        {
            switch (orderType)
            {
                case OrderType.Buy:
                    _value += (price * volume);
                    Volume += volume;
                    break;
                case OrderType.Sell:
                    _value -= (price * volume);
                    Volume -= volume;
                    break;
            }
        }
    }
}
