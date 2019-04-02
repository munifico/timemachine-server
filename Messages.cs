using System.Collections.Generic;

namespace TimemachineServer
{
    public class ReqOpenPrice
    {
        public class AssetInfo
        {
            public string Exchange { get; set; }
            public string AssetCode { get; set; }
        }

        public List<AssetInfo> Assets { get; set; }
        public string StartDate { get; set; }
    }

    public class ResOpenPrice
    {
        public List<Context> Data { get; set; } = new List<Context>();

        public class Context
        {
            public string AssetCode { get; set; }
            public string AssetName { get; set; }
            public double OpenPrice { get; set; }
        }
    }
}
