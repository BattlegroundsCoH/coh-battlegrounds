using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Battlegrounds.Campaigns.API;
using Battlegrounds.Functional;

namespace BattlegroundsApp.Views.CampaignViews.Models {
    
    public class CampaignUnitSelectionModel {

        private ICampaignSelectable m_selectedNode;
        private HashSet<CampaignUnitFormationModel> m_selection;

        public ObservableCollection<CampaignModelSelectionViewModel> SelectionView { get; set; }

        public CampaignUnitFormationModel First => this.m_selection.FirstOrDefault();

        public int Size => this.m_selection.Count;

        public CampaignUnitSelectionModel() {
            this.SelectionView = new ObservableCollection<CampaignModelSelectionViewModel>();
            this.m_selection = new HashSet<CampaignUnitFormationModel>();
        }

        public void Select(ICampaignSelectable model, bool clearSelection) {
            if (clearSelection) {
                this.Clear();
            }
            if (this.m_selectedNode is not null) {
                this.SelectionView.Remove(this.SelectionView.FirstOrDefault(x => x.SelectedObject == this.m_selectedNode));
            }
            this.m_selectedNode = model;
            this.SelectionView.Insert(0, new CampaignModelSelectionViewModel(this.m_selectedNode));
        }

        public void Select(CampaignUnitFormationModel model) {
            this.Clear();
            this.AddToSelection(model);
        }

        public void AddToSelection(CampaignUnitFormationModel model) {
            this.m_selection.Add(model);
            this.SelectionView.Add(new CampaignModelSelectionViewModel(model));
        }

        public void DeSelect(CampaignUnitFormationModel model) {
            this.m_selection.Remove(model);
            this.SelectionView.Remove(this.SelectionView.FirstOrDefault(x => x.SelectedObject == model));
        }

        public void Clear() {
            this.m_selection.Clear();
            this.SelectionView.Clear();
        }

        public void InvokeEach(Action<CampaignUnitFormationModel> action) => this.m_selection.ForEach(action);

        public bool All(Predicate<CampaignUnitFormationModel> predicate) => this.m_selection.All(x => predicate(x));

        public bool Shares<T>(Func<CampaignUnitFormationModel, T> selector) {
            if (this.Size == 1) {
                return true;
            } else if (this.Size > 0) {
                if (selector(this.m_selection.FirstOrDefault()) is CampaignUnitFormationModel def) {
                    return this.m_selection.All(x => def.Equals(selector(x)));
                }
            }
            return false;
        }

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

        public void SetSelectionView(ICampaignSelectable selectable) {
            this.m_selection.Clear();
            this.SelectionView.Clear();
            this.SelectionView.Add(new CampaignModelSelectionViewModel(selectable));
        }

        public ICampaignFormation[] ToArray() => this.m_selection.Select(x => x.Formation).ToArray();

    }

}
