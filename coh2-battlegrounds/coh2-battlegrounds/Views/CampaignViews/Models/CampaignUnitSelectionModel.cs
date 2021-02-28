using System;
using System.Collections.Generic;
using System.Linq;
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

        public void InvokeEach(Action<CampaignUnitFormationModel> action) => this.m_selection.ForEach(action);

        public bool All(Predicate<CampaignUnitFormationModel> predicate) => this.m_selection.All(x => predicate(x));

        public bool Shares<T>(Func<CampaignUnitFormationModel, T> selector) {
            if (this.Size > 0) {
                var def = selector(this.m_selection.FirstOrDefault());
                return this.All(x => def.Equals(selector(x)));
            } else {
                return false;
            }
        }

    }

}
