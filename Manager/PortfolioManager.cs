// using System;
// using System.Collections.Generic;
// using System.Collections.ObjectModel;
// using System.Linq;
// using TimeMachineServer.DB;
// using static TimeMachineServer.Constants;

// namespace TimeMachineServer
// {
//     public class PortfolioManager
//     {
//         #region Lazy Singleton
//         private static readonly Lazy<PortfolioManager> lazy =
//             new Lazy<PortfolioManager>(() => new PortfolioManager());

//         public static PortfolioManager Instance => lazy.Value;
//         #endregion

//         public ObservableCollection<Subject> Portfolio { get; private set; } = new ObservableCollection<Subject>();

//         public Subject GetSubject(string assetCode)
//         {
//             var subject = Portfolio.SingleOrDefault(x => x.AssetCode == assetCode);
//             if (null == subject)
//             {
//                 throw new InvalidAssetCodeException(assetCode);
//             }

//             return subject;
//         }

//         public List<Subject> GetPortfolio()
//         {
//             return Portfolio.ToList();
//         }

//         public void AddToBenchmark(DateTime date)
//         {
//             var assetCode = "JP225";

//             if (!Portfolio.Any(x => x.AssetCode == assetCode))
//             {
//                 var subject = new Subject
//                 {
//                     AssetCode = "JP225",
//                     AssetName = "NIKKEI225",
//                     IsBenchmark = true
//                 };

//                 using (var context = new QTContext())
//                 {
//                     var openPrice = context.Indices.Where(x => x.CreatedAt >= date && x.AssetCode == assetCode)
//                                     .OrderBy(x => x.CreatedAt).Take(10).FirstOrDefault().Open;

//                     subject.Price = openPrice;
//                 }

//                 Portfolio.Add(subject);
//                 AssetManager.Instance.AddAsset(subject.AssetCode, subject.AssetName);
//             }
//         }

//         public void AddToPortfolio(Subject subject, DateTime date)
//         {
//             if (!Portfolio.Any(x => x.AssetCode == subject.AssetCode))
//             {
//                 using (var context = new QTContext())
//                 {
//                     var openPrice = context.Stocks.Where(x => x.CreatedAt >= date && x.AssetCode == subject.AssetCode)
//                                     .OrderBy(x => x.CreatedAt).Take(10).FirstOrDefault().Open;

//                     subject.Price = openPrice;
//                 }

//                 Portfolio.Add(subject);
//             }
//         }

//         public void RemoveFromPortfolio(Subject subject)
//         {
//             Portfolio.Remove(subject);
//         }

//         public void UpdateOpenPrice(DateTime date)
//         {
//             foreach (var subject in Portfolio)
//             {
//                 using (var context = new QTContext())
//                 {
//                     if (subject.IsBenchmark)
//                     {
//                         var openPrice = context.Indices.Where(x => x.CreatedAt >= date && x.AssetCode == subject.AssetCode)
//                                     .OrderBy(x => x.CreatedAt).Take(10).FirstOrDefault().Open;

//                         subject.Price = openPrice;
//                     }
//                     else
//                     {
//                         var openPrice = context.Stocks.Where(x => x.CreatedAt >= date && x.AssetCode == subject.AssetCode)
//                                     .OrderBy(x => x.CreatedAt).Take(10).FirstOrDefault().Open;

//                         subject.Price = openPrice;
//                     }
//                 }
//             }
//         }
//     }
// }
