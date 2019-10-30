using System;
using System.Collections.Generic;
using System.ComponentModel;
using static TimemachineServer.ReqAnalyzePortfolio;
using static TimeMachineServer.Constants;

namespace TimeMachineServer
{
    public abstract class StrategyBase
    {
        public StrategyBase(StrategyType strategyType)
        {
            StrategyType = strategyType;
        }

        protected Simulator _simulator;

        [Browsable(false)]
        public StrategyType StrategyType { get; set; }

        public abstract Report Run(Dictionary<string, Dictionary<DateTime, ITradingData>> portfolioDataset,
           List<DateTime> tradingCalendar,
           BacktestingProperty property,
           Dictionary<string, PortfolioSubject> portfolio,
           Period period,
           bool isBenchmark = false);

        public abstract void OnAfterOpen(string assetCode);
    }
}
