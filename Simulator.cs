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
        private double _totalBalanceSnapshot = 0;
        private double _outstandingBalance = 0;

        private Dictionary<string, List<RecordDetail>> _recordDetails = new Dictionary<string, List<RecordDetail>>();
        private Dictionary<string, PortfolioSubject> _portfolio;

        private Dictionary<int, BalanceOfPeriod> _balanceOfYears = new Dictionary<int, BalanceOfPeriod>();
        private Dictionary<string, BalanceOfPeriod> _balanceOfMonths = new Dictionary<string, BalanceOfPeriod>();

        public Report Run(StrategyBase strategy,
            Dictionary<string, Dictionary<DateTime, ITradingData>> portfolioDataset,
            List<DateTime> tradingCalendar,
            BacktestingProperty property,
            Dictionary<string, PortfolioSubject> portfolio,
            Period period,
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
                _totalBalanceSnapshot = _report.Records.Count > 0 ? _report.Records.Last().TotalBalance : _balance;

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
                    else
                    {
                        // var prevRecord = _report.Records.OrderByDescending(x => x.Date).Take(1).FirstOrDefault();
                        var prevRecordDetail = _recordDetails[assetCode].OrderByDescending(x => x.Date).Take(1).FirstOrDefault();
                        if (prevRecordDetail != null)
                        {
                            var recordDetail = new RecordDetail()
                            {
                                Date = _currentDate,
                                AssetCode = assetCode,
                                RatingBalance = prevRecordDetail.RatingBalance,
                                Return = 0,
                                ReturnRatio = 0,
                                CumulativeReturn = prevRecordDetail.CumulativeReturn
                            };
                            _recordDetails[assetCode].Add(recordDetail);
                            recordDetails.Add(recordDetail);
                        }
                    }
                }

                var record = CreateRecord(date, recordDetails);
                _report.Records.Add(record);

                if (IsFirstTradingDateOfYear(_currentDate.Year))
                {
                    _balanceOfYears.Add(_currentDate.Year, new BalanceOfPeriod { FirstDateBalance = _report.Records.Last().TotalBalance });
                }
                else if (IsLastTradingDateOfYear(_currentDate.Year))
                {
                    _balanceOfYears[_currentDate.Year].LastDateBalance = _report.Records.Last().TotalBalance;
                }

                if (IsFirstTradingDateOfMonth(_currentDate.Year, _currentDate.Month))
                {
                    _balanceOfMonths.Add(_currentDate.ToString("yyyy-MM"), new BalanceOfPeriod { FirstDateBalance = _report.Records.Last().TotalBalance });
                }
                else if (IsLastTradingDateOfMonth(_currentDate.Year, _currentDate.Month))
                {
                    _balanceOfMonths[_currentDate.ToString("yyyy-MM")].LastDateBalance = _report.Records.Last().TotalBalance;
                }
            }

            // 통계생성
            string relationalKey = Guid.NewGuid().ToString();
            var summaryDetails = CreateSummaryDetails(relationalKey);
            _report.Summary = CreateSummary(summaryDetails, relationalKey);
            _report.AnnualReturns = CreateAnnualReturns();
            _report.MonthlyReturns = CreateMonthlyReturns();

            return _report;
        }

        private List<AnnualReturn> CreateAnnualReturns()
        {
            var annualReturns = new List<AnnualReturn>();

            foreach (var balanceOfYear in _balanceOfYears)
            {
                var annualReturn = new AnnualReturn
                {
                    Year = balanceOfYear.Key,
                    ReturnRatio = (balanceOfYear.Value.LastDateBalance - balanceOfYear.Value.FirstDateBalance) / balanceOfYear.Value.FirstDateBalance
                };

                annualReturns.Add(annualReturn);
            }

            return annualReturns;
        }

        private List<MonthlyReturn> CreateMonthlyReturns()
        {
            var monthlyReturns = new List<MonthlyReturn>();

            foreach (var balanceOfMonth in _balanceOfMonths)
            {
                var monthlyReturn = new MonthlyReturn
                {
                    Date = balanceOfMonth.Key,
                    ReturnRatio = (balanceOfMonth.Value.LastDateBalance - balanceOfMonth.Value.FirstDateBalance) / balanceOfMonth.Value.FirstDateBalance
                };

                monthlyReturns.Add(monthlyReturn);
            }

            return monthlyReturns;
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

        public bool IsFirstTradingDateOfYear(int year)
        {
            return _tradingCalendar.Where(x => x.Year == year).Min() == _currentDate;
        }

        public bool IsLastTradingDateOfYear(int year)
        {
            return _tradingCalendar.Where(x => x.Year == year).Max() == _currentDate;
        }

        public bool IsFirstTradingDateOfMonth(int year, int month)
        {
            return _tradingCalendar.Where(x => x.Year == year && x.Month == month).Min() == _currentDate;
        }

        public bool IsLastTradingDateOfMonth(int year, int month)
        {
            return _tradingCalendar.Where(x => x.Year == year && x.Month == month).Max() == _currentDate;
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
                var endBalance = _recordDetails[assetCode].LastOrDefault() == null ?
                    initialBalance : initialBalance + _recordDetails[assetCode].LastOrDefault().CumulativeReturn;

                // var annualizedReturnRatio = Math.Pow(Math.Pow(_recordDetails[assetCode].Last().RatingBalance / _recordDetails[assetCode].First().RatingBalance, (1.0 / _tradingCalendar.Count)), 250.0) - 1.0;
                // var volatilityRatio = GetStandardDeviation(_recordDetails[assetCode].Select(x => Convert.ToDouble(x.ReturnRatio)).ToList()) * Math.Sqrt(250);

                var priceVolatilityRatio = 0 < _portfolioDataset[assetCode].Count ?
                    _portfolioDataset[assetCode].Average(x => (x.Value.High - x.Value.Low) / x.Value.Close) : 0;

                var summaryDetail = new SummaryDetail
                {
                    RelationalKey = relationalKey,
                    AssetName = AssetManager.Instance.GetAssetName(assetCode),
                    AssetCode = assetCode,
                    InitialBalance = initialBalance,
                    EndBalance = endBalance,
                    Commission = _report.Transactions[assetCode].Values.Sum(x => x.Sum(y => y.Commission)),
                    Transactions = _report.Transactions[assetCode].Count,
                    PeriodReturnRatio = (endBalance - initialBalance) / initialBalance,
                    // AnnualizedReturnRatio = annualizedReturnRatio,
                    // VolatilityRatio = volatilityRatio,
                    PriceVolatilityRatio = priceVolatilityRatio,
                    MddRatio = _holdStocks[assetCode].Mdd,
                    // SharpeRatio = annualizedReturnRatio / volatilityRatio
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

            var priceVolatilityRatio = 0 < summaryDetails.Count ?
                  summaryDetails.Average(x => x.PriceVolatilityRatio) : 0;

            var summary = new Summary
            {
                RelationalKey = relationalKey,
                SubjectType = (_isBenchmark == true) ? "벤치마크" : "포트폴리오",
                StrategyType = EnumHelper<StrategyType>.GetDisplayValue(_strategy.StrategyType),
                InitialBalance = initBalance,
                EndBalance = endBalance,
                Commission = summaryDetails.Sum(x => x.Commission),
                Transactions = summaryDetails.Sum(x => x.Transactions),
                PeriodReturnRatio = periodReturnRatio,
                AnnualizedReturnRatio = annualizedReturnRatio,
                VolatilityRatio = volatilityRatio,
                PriceVolatilityRatio = priceVolatilityRatio,
                MddRatio = mddRatio,
                SharpeRatio = sharpeRatio,
            };

            summaryDetails.ForEach(x => { summary.SummaryDetails.Add(x); });

            return summary;
        }

        // 개별 종목 계산
        private RecordDetail CreateRecordDetail(string assetCode)
        {
            // var index = _tradingIndex[assetCode];

            // 전일 누적수익
            var prevCumulativeReturn = IsFirstDate(assetCode) ?
            // 0 : _recordDetails[assetCode][index - 1].CumulativeReturn;
            0 : _recordDetails[assetCode][_recordDetails[assetCode].Count - 1].CumulativeReturn;

            // 전일 평가금액
            var prevRatingBalane = IsFirstDate(assetCode) ?
            // 0 : _recordDetails[assetCode][index - 1].RatingBalance;
            0 : _recordDetails[assetCode][_recordDetails[assetCode].Count - 1].RatingBalance;

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

                            // 당일매도금액
                            var sellValue = transaction.Where(x => x.Side == OrderType.Sell).Sum(x => x.Price * x.Volume) - transaction.Where(x => x.Side == OrderType.Sell).Sum(x => x.Commission);

                            // 당일매수금액
                            var buyValue = transaction.Where(x => x.Side == OrderType.Buy).Sum(x => x.Price * x.Volume) + transaction.Where(x => x.Side == OrderType.Buy).Sum(x => x.Commission);

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
                Date = _currentDate,
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
            var dailyReturn = recordDetails.Sum(x => x.Return);

            // 오늘 수익률
            var dailyReturnRatio = 0.0;
            switch (_strategy.StrategyType)
            {
                case StrategyType.BuyAndHold:
                    {
                        double closeSum = 0.0;
                        foreach (var detail in recordDetails)
                        {
                            // closeSum += _portfolioDataset[detail.AssetCode][_currentDate].Close * GetVolume(detail.AssetCode);

                            if (!IsFirstDate(detail.AssetCode))
                            {
                                closeSum += GetPrice(detail.AssetCode, PriceType.Close, -1) * GetVolume(detail.AssetCode); // 이미 _tradingIndex를 증가시켜서 -1이 오늘
                            }
                        }
                        dailyReturnRatio = (0 < dailyReturn && 0 < closeSum) ? dailyReturn / closeSum : 0;
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
            double average = values.Average(); // 수익률의평균

            double sumOfDerivation = 0;
            foreach (double value in values)
            {
                sumOfDerivation += Math.Pow(value - average, 2);
            }
            double sumOfDerivationAverage = sumOfDerivation / (values.Count - 1);
            return Math.Sqrt(sumOfDerivationAverage);
        }

        #region LimitOrder
        public bool LimitOrderPercent(string assetCode, OrderType orderType, double orderPrice, double rate)
        {
            orderPrice = ApplySlippage(orderPrice, orderType);

            // double orderVolume = (_balance * rate) / orderPrice;
            double orderVolume = (_totalBalanceSnapshot * rate) / orderPrice;

            return LimitOrder(assetCode, orderType, orderPrice, orderVolume, applySlippage: false);
        }

        public bool LimitOrder(string assetCode, OrderType orderType, double orderPrice, double orderVolume, bool applySlippage = true)
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

            if (applySlippage)
            {
                orderPrice = ApplySlippage(orderPrice, orderType);
            }

            orderPrice = Math.Truncate(orderPrice * 100) / 100;  // 소수점 2자리 이후 버림

            // 수수료
            double commission = GetCommission(orderPrice, orderVolume);

            var low = subjectDataset[_currentDate].Low;
            var high = subjectDataset[_currentDate].High;

            orderPrice = Math.Max(low, orderPrice);
            orderPrice = Math.Min(high, orderPrice);

            var orderValue = (orderPrice * orderVolume);

            if (OrderType.Buy == orderType)
            {
                if (_balance < orderValue + commission)
                {
                    // 수수료 때문에 밸런스를 초과하는 경우
                    double overbalance = (orderValue + commission) - _balance;
                    orderVolume -= ((overbalance / orderPrice) + 1);
                    // 1주미만 거래허용하지 않으면 소수점 버림
                    if (!Property.UsePointVolume)
                    {
                        orderVolume = Math.Truncate(orderVolume);
                    }
                    orderValue = (orderPrice * orderVolume);
                    commission = GetCommission(orderPrice, orderVolume); // 수수료 다시 계산

                    if (0 >= orderVolume)
                    {
                        return false;
                    }

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
            }
            else if (OrderType.Sell == orderType)
            {
                if (_holdStocks[assetCode].Volume < orderVolume)
                {
                    throw new NotEnoughVolumeException(assetCode, _holdStocks[assetCode].Volume, orderVolume, subjectDataset[_currentDate].CreatedAt);
                }
            }

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

        private double ApplySlippage(double orderPrice, OrderType orderType)
        {
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
                        orderPrice * (1 + (Property.Slippage / 2)) : orderPrice * (1 - (Property.Slippage / 2)); // 슬리피지 0.15입력하면 살 때, 팔 때 0.075씩 적용(1/2 적용)
                    break;
            }

            return orderPrice;
        }

        private double GetCommission(double orderPrice, double orderVolume)
        {
            // 수수료
            double commission = 0.0;
            switch (Property.CommissionType)
            {
                case CommissionType.Fixed:
                    commission = Property.Commission;
                    break;
                case CommissionType.Ratio:
                    commission = orderPrice * orderVolume * (Property.Commission);
                    break;
            }

            return commission;
        }

        #endregion
    }
}
