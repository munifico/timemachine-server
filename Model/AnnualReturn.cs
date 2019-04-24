using System;
using static TimeMachineServer.Constants;

namespace TimeMachineServer
{
    public class AnnualReturn
    {
        public int Year { get; set; }
        public double ReturnRatio { get; set; }
    }

    public class MonthlyReturn
    {
        public string Date { get; set; }
        public double ReturnRatio { get; set; }
    }
}