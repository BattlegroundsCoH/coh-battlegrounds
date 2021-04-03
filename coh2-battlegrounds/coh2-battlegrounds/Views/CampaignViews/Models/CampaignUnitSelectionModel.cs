using System;
using System.Collections.Generic;
using System.Linq;
using Battlegrounds.Campaigns.API;
using Battlegrounds.Functional;

namespace BattlegroundsApp.Views.CampaignViews.Models {
    
    public class CampaignUnitSelectionModel {

        private HashSet<CampaignUnitFormationModel> m_selection;
        private bool m_isLocked;

        public CampaignUnitFormationModel First => this.m_selection.FirstOrDefault();

        public int Size => this.m_selection.Count;

        public CampaignUnitSelectionModel() {
            this.m_selection = new HashSet<CampaignUnitFormationModel>();
        }

        public void Select(CampaignUnitFormationModel model) {
            this.Clear();
            this.AddToSelection(model);
        }

        public void AddToSelection(CampaignUnitFormationModel model) =>  (!this.m_isLocked).Then(() => this.m_selection.Add(model));

        public void Select(IEnumerable<CampaignUnitFormationModel> model) {
            if (!this.m_isLocked) {
                this.Clear();
                model.ForEach(x => this.m_selection.Add(x));
            }
        }

        public void DeSelect(CampaignUnitFormationModel model) => (!this.m_isLocked).Then(() => this.m_selection.Remove(model));

        public void Clear() => (!this.m_isLocked).Then(() => this.m_selection.Clear());

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

        public void Lock() => this.m_isLocked = true;

        public void Unlock() => this.m_isLocked = false;

        public List<ICampaignFormation> Get() => this.m_selection.Select(x => x.Formation).ToList();

        public int Filter(Predicate<CampaignUnitFormationModel> p) {
            int count = 0;
            var itt = this.m_selection.GetEnumerator();
            while (itt.MoveNext()) {
                if (!p(itt.Current)) {
                    this.m_selection.Remove(itt.Current);
                } else {
                    count++;
                }
            }
            return count;
        }

    }

}
