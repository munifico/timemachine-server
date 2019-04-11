using System;
using System.Collections.Generic;
using static TimemachineServer.ReqAnalyzePortfolio;
using static TimeMachineServer.Constants;

namespace TimeMachineServer
{
    public class StrategyManager
    {
        #region Lazy Singleton
        private static readonly Lazy<StrategyManager> lazy =
            new Lazy<StrategyManager>(() => new StrategyManager());

        public static StrategyManager Instance => lazy.Value;
        #endregion

        private readonly Dictionary<StrategyType, StrategyBase> _strategies = new Dictionary<StrategyType, StrategyBase>();

        public void AddStrategy(StrategyType strategyType, StrategyBase strategy)
        {
            if (!_strategies.ContainsKey(strategyType))
            {
                _strategies.Add(strategyType, strategy);
            }
        }

        public void RemoveStrategy(StrategyType strategyType)
        {
            _strategies.Remove(strategyType);
        }

        public StrategyBase GetStrategy(StrategyType strategyType)
        {
            if (_strategies.ContainsKey(strategyType))
            {
                return _strategies[strategyType];
            }
            else
            {
                return null;
            }
        }

        public void Clear()
        {
            _strategies.Clear();
        }

        public void Run(Dictionary<KeyValuePair<bool, StrategyType>, Report> reports,
            Dictionary<string, Dictionary<DateTime, ITradingData>> portfolioDataset,
            List<DateTime> tradingCalendar,
            BacktestingProperty property,
            Dictionary<string, PortfolioSubject> portfolio)
        {
            foreach (var strategy in _strategies.Values)
            {
                var report = strategy.Run(portfolioDataset, tradingCalendar, property, portfolio);
                reports.Add(new KeyValuePair<bool, StrategyType>(false, strategy.StrategyType), report);
            }
        }
    }
}
