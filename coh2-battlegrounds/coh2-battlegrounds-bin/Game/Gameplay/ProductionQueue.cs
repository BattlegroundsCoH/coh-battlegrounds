using System;
using System.Collections.Generic;
using System.Text;
using coh2_battlegrounds_bin.Game.Database;

namespace coh2_battlegrounds_bin.Game.Gameplay {
    
    /// <summary>
    /// 
    /// </summary>
    public class ProductionQueue {



        private struct ProductionItem {
            public Blueprint build;
            public BlueprintType type;
            public TimeSpan remaining;
        }

        private List<ProductionItem> m_productionQueue;

        public ProductionQueue() {

        }

        public void AddProduction(Blueprint bp) {

        }

        public void CancelProduction(int index) => this.m_productionQueue.RemoveAt(index);

        public Squad UpdateProduction(TimeSpan diff) {

            return null;
        }

    }

}
