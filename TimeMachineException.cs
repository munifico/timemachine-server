using System;

namespace TimeMachine.Server
{
    public class NotTradingDateException : Exception
    {
        public NotTradingDateException()
        {
        }

        public NotTradingDateException(string assetCode, DateTime datetime)
            : base($"not trading date: {assetCode} {datetime.ToShortDateString()}")
        {
        }
    }

    public class NoTradingDataExistsException : Exception
    {
        public NoTradingDataExistsException()
        {
        }

        public NoTradingDataExistsException(string assetCode, DateTime datetime)
            : base($"No trading data exists. asset code: {assetCode}, {datetime.ToShortDateString()}")
        {
        }
    }

    public class NotEnoughBalanceException : Exception
    {
        public NotEnoughBalanceException()
        {
        }

        public NotEnoughBalanceException(string assetCode, double balance, double buyBalance, DateTime datetime)
            : base($"Not enough balance. asset code: {assetCode}, date: {datetime.ToShortDateString()}, {balance} < {buyBalance}")
        {
        }
    }

    public class NotEnoughVolumeException : Exception
    {
        public NotEnoughVolumeException()
        {
        }

        public NotEnoughVolumeException(string assetCode, double volume, double sellVolume, DateTime datetime)
            : base($"Not enough volume. asset code: {assetCode}, date: {datetime.ToShortDateString()}, {volume} < {sellVolume}")
        {
        }
    }

    public class InvalidAssetCodeException : Exception
    {
        public InvalidAssetCodeException()
        {
        }

        public InvalidAssetCodeException(string assetCode)
            : base($"Invalid asset code: {assetCode}")
        {
        }
    }

    public class InvalidPriceException : Exception
    {
        public InvalidPriceException()
        {
        }

        public InvalidPriceException(string assetCode, double orderPrice, double lowPrice, double highPrice, DateTime datetime)
            : base($"Invalid price. asset code: {assetCode}, date: {datetime.ToShortDateString()}, {orderPrice} is out of {lowPrice}~{highPrice}")
        {
        }
    }
}
