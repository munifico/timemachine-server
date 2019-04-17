using System;
using System.Collections.Generic;
using System.Linq;
using TimeMachineServer.Helper;
using static TimemachineServer.ReqAnalyzePortfolio;
using static TimeMachineServer.Constants;

namespace TimeMachineServer
{
    public class Simulator
    {
        public BacktestingProperty Property { get; set; }

        private Dictionary<string, List<DateTime>> _tradingDates = new Dictionary<string, List<DateTime>>();
        private readonly Dictionary<string, int> _tradingIndex = new Dictionary<string, int>();
        private DateTime _currentDate;

        private StrategyBase _strategy;

        private List<DateTime> _tradingCalendar;
        private Dictionary<string, Dictionary<DateTime, ITradingData>> _portfolioDataset;
        private Report _report;
        private bool _isBenchmark;

        private Dictionary<string, HoldStock> _holdStocks = new Dictionary<string, HoldStock>();
        private double _highestTotalBalance = 0;
        private double _balance = 0;
        private double _outstandingBalance = 0;

        private Dictionary<string, List<RecordDetail>> _recordDetails = new Dictionary<string, List<RecordDetail>>();
        private Dictionary<string, PortfolioSubject> _portfolio;

        public Report Run(StrategyBase strategy,
            Dictionary<string, Dictionary<DateTime, ITradingData>> portfolioDataset,
            List<DateTime> tradingCalendar,
            BacktestingProperty property,
            Dictionary<string, PortfolioSubject> portfolio,
            bool isBenchmark = false)
        {
            _strategy = strategy;
            _isBenchmark = isBenchmark;
            _report = new Report(_strategy.StrategyType);
            _portfolioDataset = portfolioDataset;
            _tradingCalendar = tradingCalendar;
            _portfolio = portfolio;
            Property = property;

            _balance = property.Capital;
            _highestTotalBalance = property.Capital;

            foreach (var assetCode in _portfolioDataset.Keys)
            {
                var initialBalance = Property.Capital * _portfolio[assetCode].Ratio;

                _holdStocks.Add(assetCode, new HoldStock
                {
                    InitialBalance = initialBalance
                });

                _report.Transactions.Add(assetCode, new Dictionary<DateTime, List<Transaction>>());

                // 에셋별로 실제거래한 날짜데이터를 갖는다(트레이딩 달력이랑 다름)
                var tradingDate = portfolioDataset[assetCode].Keys.ToList();
                _tradingDates.Add(assetCode, tradingDate);
                _tradingIndex.Add(assetCode, 0);
                _recordDetails.Add(assetCode, new List<RecordDetail>());
            }

            foreach (var date in tradingCalendar)
            {
                _currentDate = date; // 트레이딩 달력기준 날짜

                var recordDetails = new List<RecordDetail>();
                foreach (var assetCode in _portfolioDataset.Keys)
                {
                    var subjectDataset = _portfolioDataset[assetCode];
                    if (subjectDataset.ContainsKey(date))
                    {
                        _strategy.OnAfterOpen(assetCode);

                        var recordDetail = CreateRecordDetail(assetCode);
                        _recordDetails[assetCode].Add(recordDetail);

                        recordDetails.Add(recordDetail);
                        _tradingIndex[assetCode]++;
                    }
                }

                var record = CreateRecord(date, recordDetails);
                _report.Records.Add(record);
            }

            // 통계생성
            string relationalKey = Guid.NewGuid().ToString();
            var summaryDetails = CreateSummaryDetails(relationalKey);
            _report.Summary = CreateSummary(summaryDetails, relationalKey);

            return _report;
        }

        public double GetMovingAverage(string assetCode, int days, PriceType priceType)
        {
            days *= -1;
            var prices = new List<double>();

            for (var i = -1; i >= days; --i)
            {
                prices.Add(GetPrice(assetCode, priceType, i));
            }

            return prices.Average();
        }

        public double GetVolume(string assetCode)
        {
            return _holdStocks[assetCode].Volume;
        }

        public int GetTradingIndex(string assetCode)
        {
            return _tradingIndex[assetCode];
        }

        public bool IsFirstDate(string assetCode)
        {
            return _currentDate == _portfolioDataset[assetCode].FirstOrDefault().Key;
        }

        public bool IsLastDate(string assetCode)
        {
            return _currentDate == _portfolioDataset[assetCode].LastOrDefault().Key;
        }

        public bool IsFirstWeek(string assetCode)
        {
            var firstweek = new YearOfWeek(_portfolioDataset[assetCode].FirstOrDefault().Key);
            var currentweek = new YearOfWeek(_currentDate);

            return firstweek == currentweek;
        }

        public YearOfWeek GetYearOfWeek()
        {
            return new YearOfWeek(_currentDate);
        }

        public PortfolioSubject GetSubject(string assetCode)
        {
            return _portfolio[assetCode];
        }

        public double GetPrice(string assetCode, PriceType priceType, int period)
        {
            if (!_portfolioDataset.ContainsKey(assetCode))
            {
                throw new InvalidAssetCodeException(assetCode);
            }

            var tradingDates = _tradingDates[assetCode];
            var tradingIndex = _tradingIndex[assetCode];
            var date = tradingDates[tradingIndex + period];
            var stock = _portfolioDataset[assetCode][date];

            switch (priceType)
            {
                case PriceType.Open:
                    return stock.Open;
                case PriceType.High:
                    return stock.High;
                case PriceType.Low:
                    return stock.Low;
                case PriceType.Close:
                    return stock.Close;
                default:
                    throw new Exception("Unknown price type");
            }
        }

        private List<SummaryDetail> CreateSummaryDetails(string relationalKey)
        {
            var summaryDetails = new List<SummaryDetail>();

            foreach (var assetCode in _recordDetails.Keys)
            {
                var initialBalance = Property.Capital * _portfolio[assetCode].Ratio;
                var endBalance = initialBalance + _recordDetails[assetCode].Last().CumulativeReturn;

                var summaryDetail = new SummaryDetail
                {
                    RelationalKey = relationalKey,
                    AssetName = AssetManager.Instance.GetAssetName(assetCode),
                    AssetCode = assetCode,
                    InitialBalance = initialBalance,
                    EndBalance = endBalance,
                    Commission = _report.Transactions[assetCode].Values.Sum(x => x.Sum(y => y.Commission)),
                    PeriodReturnRatio = (endBalance - initialBalance) / initialBalance,
                    MddRatio = _holdStocks[assetCode].Mdd,
                };

                summaryDetails.Add(summaryDetail);
            }

            return summaryDetails;
        }

        private Summary CreateSummary(List<SummaryDetail> summaryDetails, string relationalKey)
        {
            var periodReturnRatio = (_report.Records.Last().TotalBalance - Property.Capital) / Property.Capital;
            var annualizedReturnRatio = Math.Pow(Math.Pow(_report.Records.Last().TotalBalance / _report.Records.First().TotalBalance, (1.0 / _tradingCalendar.Count)), 250.0) - 1.0;
            var volatilityRatio = GetStandardDeviation(_report.Records.Select(x => Convert.ToDouble(x.ReturnRatio)).ToList()) * Math.Sqrt(250);
            var mddRatio = _report.Records.Min(x => x.Mdd);
            var sharpeRatio = annualizedReturnRatio / volatilityRatio;

            var initBalance = Property.Capital;
            var endBalance = _report.Records.Last().TotalBalance;

            var summary = new Summary
            {
                RelationalKey = relationalKey,
                SubjectType = (_isBenchmark == true) ? "벤치마크" : "포트폴리오",
                StrategyType = EnumHelper<StrategyType>.GetDisplayValue(_strategy.StrategyType),
                InitialBalance = initBalance,
                EndBalance = endBalance,
                Commission = summaryDetails.Sum(x => x.Commission),
                PeriodReturnRatio = periodReturnRatio,
                AnnualizedReturnRatio = annualizedReturnRatio,
                VolatilityRatio = volatilityRatio,
                MddRatio = mddRatio,
                SharpeRatio = sharpeRatio,
            };

            summaryDetails.ForEach(x => { summary.SummaryDetails.Add(x); });

            return summary;
        }

        // 개별 종목 계산
        private RecordDetail CreateRecordDetail(string assetCode)
        {
            var index = _tradingIndex[assetCode];

            // 전일 누적수익
            var prevCumulativeReturn = IsFirstDate(assetCode) ?
                0 : _recordDetails[assetCode][index - 1].CumulativeReturn;

            // 전일 평가금액
            var prevRatingBalane = IsFirstDate(assetCode) ?
                 0 : _recordDetails[assetCode][index - 1].RatingBalance;

            // 평가금액
            var ratingBalance = _portfolioDataset[assetCode][_currentDate].Close * _holdStocks[assetCode].Volume;

            // 최고 평가금액 업데이트
            _holdStocks[assetCode].HighestRatingBalance = Math.Max(_holdStocks[assetCode].HighestRatingBalance, ratingBalance);

            // 최저 MDD 업데이트
            if (0 < ratingBalance)
            {
                _holdStocks[assetCode].Mdd = Math.Min(_holdStocks[assetCode].Mdd,
                (_holdStocks[assetCode].HighestRatingBalance - ratingBalance) / _holdStocks[assetCode].HighestRatingBalance * -1);
            }

            // 오늘수익
            var dailyReturn = 0.0;
            switch (_strategy.StrategyType)
            {
                case StrategyType.BuyAndHold:
                    {
                        if (IsFirstDate(assetCode))
                        {
                            ratingBalance = _portfolioDataset[assetCode][_currentDate].Open * _holdStocks[assetCode].Volume;
                        }
                        else
                        {
                            // 평가금액 - 전일평가금액
                            dailyReturn = ratingBalance - prevRatingBalane;
                        }
                    }
                    break;
                default:
                    {
                        if (_report.Transactions[assetCode].ContainsKey(_currentDate))
                        {
                            var transaction = _report.Transactions[assetCode][_currentDate];
                            var sellValue = transaction.Where(x => x.Side == OrderType.Sell).Sum(x => x.Price * x.Volume); // 당일매도금액
                            var buyValue = transaction.Where(x => x.Side == OrderType.Buy).Sum(x => x.Price * x.Volume); // 당일매수금액
                            var dailyReturnRatio = (ratingBalance + sellValue) / (prevRatingBalane + buyValue) - 1;

                            dailyReturn = (ratingBalance + sellValue) - (prevRatingBalane + buyValue);
                        }
                    }
                    break;
            }

            // 누적수익
            var cumulativeReturn = prevCumulativeReturn + dailyReturn;

            var recordDetail = new RecordDetail
            {
                AssetCode = assetCode,
                RatingBalance = ratingBalance,
                Return = dailyReturn,
                CumulativeReturn = cumulativeReturn,
            };

            return recordDetail;
        }

        private Record CreateRecord(DateTime date, List<RecordDetail> recordDetails)
        {
            // 오늘총평가 = 잔고 + 각주식들의 평가금액(sum) 잔고는 이미 미수금 반영되어 마이너스로 되어있음!!!
            var totalBalance = _balance + recordDetails.Sum(x => x.RatingBalance);

            // 어제 누적수익
            var prevCumulativeReturn = 0 < _report.Records.Count ?
                _report.Records.Last().CumulativeReturn : 0;

            // 어제총평가
            var prevTotalBalance = 0 < _report.Records.Count ?
                _report.Records.Last().TotalBalance : 0;

            // 오늘수익
            // TODO: 이렇게 계산하면 안될 듯
            var dailyReturn = 0 < _report.Records.Count ?
                totalBalance - prevTotalBalance : 0;

            // 오늘 수익률
            var dailyReturnRatio = 0.0;
            switch (_strategy.StrategyType)
            {
                case StrategyType.BuyAndHold:
                    {
                        double openSum = 0.0;
                        foreach (var detail in recordDetails)
                        {
                            openSum += _portfolioDataset[detail.AssetCode][_currentDate].Open;
                        }
                        dailyReturnRatio = dailyReturn / openSum;
                    }
                    break;
                default:
                    {
                        dailyReturnRatio = prevTotalBalance == 0 ?
                            0 : (totalBalance - prevTotalBalance) / prevTotalBalance;
                    }
                    break;
            }

            // 최고 자산 갱신
            _highestTotalBalance = Math.Max(_highestTotalBalance, totalBalance);

            var record = new Record
            {
                Date = date,
                TotalBalance = totalBalance,
                Balance = _balance,
                RatingBalance = recordDetails.Sum(x => x.RatingBalance),
                Return = dailyReturn,
                CumulativeReturn = prevCumulativeReturn + dailyReturn,
                ReturnRatio = dailyReturnRatio,
                CumulativeReturnRatio = (prevCumulativeReturn + dailyReturn) / Property.Capital,
                VolatilityRatio = dailyReturnRatio,
                Mdd = (_highestTotalBalance - totalBalance) / _highestTotalBalance * -1,
                Max = _highestTotalBalance
            };

            return record;
        }

        private double GetStandardDeviation(List<double> values)
        {
            double average = values.Average();
            double sumOfDerivation = 0;
            foreach (double value in values)
            {
                sumOfDerivation += (value) * (value);
            }
            double sumOfDerivationAverage = sumOfDerivation / (values.Count - 1);
            return Math.Sqrt(sumOfDerivationAverage - (average * average));
        }

        #region LimitOrder
        public bool LimitOrderPercent(string assetCode, OrderType orderType, double orderPrice, double rate)
        {
            double orderVolume = (_balance * rate) / orderPrice;

            return LimitOrder(assetCode, orderType, orderPrice, orderVolume);
        }

        public bool LimitOrder(string assetCode, OrderType orderType, double orderPrice, double orderVolume)
        {
            // 1주미만 거래허용하지 않으면 소수점 버림
            if (!Property.UsePointVolume)
            {
                orderVolume = Math.Truncate(orderVolume);
            }

            if (0 >= orderVolume)
            {
                return false;
            }

            if (!_portfolioDataset.ContainsKey(assetCode))
            {
                throw new InvalidAssetCodeException(assetCode);
            }

            if (!_holdStocks.ContainsKey(assetCode))
            {
                throw new InvalidAssetCodeException(assetCode);
            }

            var subjectDataset = _portfolioDataset[assetCode];
            if (!subjectDataset.ContainsKey(_currentDate))
            {
                throw new NoTradingDataExistsException(assetCode, _currentDate);
            }

            // 슬리피지 적용
            switch (Property.SlippageType)
            {
                case SlippageType.Fixed:
                    orderPrice =
                        orderType == OrderType.Buy ?
                        orderPrice + Property.Slippage : orderPrice - Property.Slippage;
                    break;
                case SlippageType.Ratio:
                    orderPrice =
                        orderType == OrderType.Buy ?
                        orderPrice * (1 + Property.Slippage / 100) : orderPrice * (1 - Property.Slippage / 100);
                    break;
            }

            orderPrice = Math.Truncate(orderPrice * 100) / 100;  // 소수점 2자리 이후 버림

            // 수수료
            double commission = 0.0;
            switch (Property.CommissionType)
            {
                case CommissionType.Fixed:
                    commission = Property.Commission;
                    break;
                case CommissionType.Ratio:
                    commission = orderPrice * orderVolume * (Property.Commission / 100);
                    break;
            }

            var orderValue = (orderPrice * orderVolume);

            if (OrderType.Buy == orderType)
            {
                if (_balance < orderValue + commission)
                {
                    // 미수금 사용하지 않는 모드에서는 거래 실패
                    if (!Property.UseOutstandingBalance)
                    {
                        return false;
                    }
                    else
                    {
                        _outstandingBalance += orderValue - _balance;
                    }
                }
            }
            else if (OrderType.Sell == orderType)
            {
                if (_holdStocks[assetCode].Volume < orderVolume)
                {
                    throw new NotEnoughVolumeException(assetCode, _holdStocks[assetCode].Volume, orderVolume, subjectDataset[_currentDate].CreatedAt);
                }
            }

            var low = subjectDataset[_currentDate].Low;
            var high = subjectDataset[_currentDate].High;

            orderPrice = Math.Max(low, orderPrice);
            orderPrice = Math.Min(high, orderPrice);

            if (orderPrice >= low && orderPrice <= high)
            {
                if (OrderType.Buy == orderType)
                {
                    // 잔고감소
                    _balance -= (orderValue + commission);
                }
                else if (OrderType.Sell == orderType)
                {
                    _balance += (orderValue - commission);

                    // 미수금 상환
                    if (0 < _outstandingBalance)
                    {
                        _outstandingBalance -= Math.Min(_outstandingBalance, _balance);
                    }
                }

                _holdStocks[assetCode].Update(orderType, orderPrice, orderVolume);
            }
            else
            {
                throw new InvalidPriceException(assetCode, orderPrice, low, high, subjectDataset[_currentDate].CreatedAt);
            }

            var transaction = new Transaction(subjectDataset[_currentDate].CreatedAt, assetCode, orderType, orderPrice, orderVolume, commission, _outstandingBalance);

            if (_report.Transactions[assetCode].ContainsKey(_currentDate))
            {
                _report.Transactions[assetCode][_currentDate].Add(transaction);
            }
            else
            {
                _report.Transactions[assetCode].Add(_currentDate, new List<Transaction> { transaction });
            }

            return true;
        }
        #endregion
    }
}
