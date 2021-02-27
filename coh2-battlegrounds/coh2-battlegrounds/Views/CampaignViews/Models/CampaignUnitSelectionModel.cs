using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Battlegrounds.Functional;

namespace BattlegroundsApp.Views.CampaignViews.Models {
    
    public class CampaignUnitSelectionModel {

        private HashSet<CampaignUnitFormationModel> m_selection;

        public int Size => this.m_selection.Count;

        public CampaignUnitSelectionModel() {
            this.m_selection = new HashSet<CampaignUnitFormationModel>();
        }

        public void Select(CampaignUnitFormationModel model) {
            this.Clear();
            this.m_selection.Add(model);
        }

        public void AddToSelection(CampaignUnitFormationModel model) => this.m_selection.Add(model);

        public void Select(IEnumerable<CampaignUnitFormationModel> model) {
            this.Clear();
            model.ForEach(x => this.m_selection.Add(x));
        }

        public void DeSelect(CampaignUnitFormationModel model) => this.m_selection.Remove(model);

        public void Clear() => this.m_selection.Clear();

    }

}
