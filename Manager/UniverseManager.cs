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

        private List<Subject> universe = new List<Subject>();

        public void Initialize()
        {
            using (var context = new QTContext())
            {
                universe = context.Universe.Where(x => x.FirstDate != null).ToList();

                universe.ForEach(x =>
                {
                    AssetManager.Instance.AddAsset(x.AssetCode, x.AssetName);
                });
            }
        }

        public List<Subject> GetUniverse(string country, string exchange)
        {
            if (exchange == null)
            {
                return universe.Where(x => x.Country == country).ToList();
            }
            else
            {
                return universe.Where(x => x.Country == country && x.Exchange == exchange).ToList();
            }
        }

        public Subject FindUniverse(string assetCode)
        {
            return universe.Where(x => x.AssetCode == assetCode).FirstOrDefault();
        }
    }
}