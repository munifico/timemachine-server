using System;
using System.Collections.Generic;
using TimeMachineServer.Helper;
using static TimeMachineServer.Constants;

namespace TimeMachineServer
{
    public class Report
    {
        public Report(StrategyType strategyType)
        {
            StrategyType = EnumHelper<StrategyType>.GetDisplayValue(strategyType);
        }

        public string StrategyType { get; set; }

        public Summary Summary { get; set; }
        public List<Record> Records { get; set; } = new List<Record>();
        public Dictionary<string, Dictionary<DateTime, List<Transaction>>> Transactions { get; set; }
            = new Dictionary<string, Dictionary<DateTime, List<Transaction>>>();
    }
}
