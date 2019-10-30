using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TimeMachineServer.DB;

namespace TimeMachineServer
{
    public class UniverseManager
    {
        #region Lazy Singleton
        private static readonly Lazy<UniverseManager> lazy =
            new Lazy<UniverseManager>(() => new UniverseManager());

        public static UniverseManager Instance => lazy.Value;
        #endregion

        private List<Subject> _universe = new List<Subject>();

        public void Initialize()
        {
            using (var context = new QTContext())
            {
                _universe = context.Universe.Where(x => x.FirstDate != null).ToList();

                _universe.ForEach(x =>
                {
                    AssetManager.Instance.AddAsset(x.AssetCode, x.AssetName);
                });
            }
        }

        public List<Subject> GetUniverse(string country, string exchange)
        {
            if (exchange == null)
            {
                return _universe.Where(x => x.Country == country && x.MarketCap > 0).ToList();
            }
            else
            {
                if (exchange.Contains("FX"))
                {
                    var universe = _universe.Where(x => x.Country == country && x.Exchange == "FX").ToList();

                    using (var context = new QTContext())
                    {
                        if (exchange == "FX_1d")
                        {
                            universe.ForEach(subject =>
                            {
                                subject.FirstDate = context.FX1D.Where(x => x.AssetCode == subject.AssetCode).Min(x => x.CreatedAt);
                            });
                        }
                        else if (exchange == "FX_1w")
                        {
                            universe.ForEach(subject =>
                            {
                                subject.FirstDate = context.FX1W.Where(x => x.AssetCode == subject.AssetCode).Min(x => x.CreatedAt);
                            });
                        }
                        else if (exchange == "FX_60m")
                        {
                            universe.ForEach(subject =>
                            {
                                subject.FirstDate = context.FX60M.Where(x => x.AssetCode == subject.AssetCode).Min(x => x.CreatedAt);
                            });
                        }
                    }
                    return universe;
                }
                else
                {
                    return _universe.Where(x => x.Country == country && x.Exchange == exchange && x.MarketCap > 0).ToList();
                }
            }
        }

        public Subject FindUniverse(string assetCode)
        {
            return _universe.Where(x => x.AssetCode == assetCode).FirstOrDefault();
        }
    }
}