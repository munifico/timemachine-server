using System;
using static TimeMachineServer.Constants;

namespace TimeMachineServer
{
    public class Transaction
    {
        public Transaction(DateTime date, string assetCode, OrderType orderType, double price, double volume, double commission, double creditBalance)
        {
            Date = date;
            AssetCode = assetCode;
            AssetName = AssetManager.Instance.GetAssetName(assetCode);
            Side = orderType;
            Price = price;
            Volume = volume;
            Commission = commission;
            CreditBalance = creditBalance;
        }

        public DateTime Date { get; }
        public string AssetName { get; }
        public string AssetCode { get; }
        public OrderType Side { get; }
        public double Price { get; }
        public double Volume { get; }
        public double Commission { get; }
        public double CreditBalance { get; }
    }
}
