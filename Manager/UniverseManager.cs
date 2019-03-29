using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TimeMachine.Server.DB;

namespace TimeMachine.Core
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
                universe = context.Universe.ToList();
            }
        }

        public List<Subject> GetUniverse(string exchange)
        {
            if (exchange == null)
            {
                return universe.ToList();
            }
            else
            {
                return universe.Where(x => x.Exchange == exchange).ToList();
            }
        }
    }
}