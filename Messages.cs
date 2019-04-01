using System.Collections.Generic;

namespace TimemachineServer
{
    public class ReqAddPortfolio
    {
        public string StartDate { get; set; }
        public List<string> AssetCodes { get; set; }
    }
}
