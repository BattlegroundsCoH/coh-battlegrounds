using System;
using System.Collections;
using System.Linq;
using System.Windows;
using Battlegrounds.Campaigns.API;
using Battlegrounds.Functional;
using Battlegrounds.Util.Coroutines;
using BattlegroundsApp.Utilities;

namespace BattlegroundsApp.Views.CampaignViews.Models {
    
    public class CampaignUnitFormationModel : ICampaignMapVisual {
    
        public ICampaignFormation Formation { get; }

        public UIElement VisualElement { get; }

        public Func<ICampaignMapNode, ICampaignPointsNode> NodeModelFetcher { get; init; }

        public Func<ICampaignFormation, ICampaignMapVisual> UnitModelFetcher { get; init; }

        public Action<CampaignUnitFormationModel> RemoveHandle { get; init; }

        public GUIThreadDispatcher ThreadDispatcher { get; init; }

        public CampaignUnitFormationModel(UIElement element, ICampaignFormation formation) {
            this.VisualElement = element;
            this.Formation = formation;
            this.Formation.FormationMoved += this.FormationMoved;
            this.Formation.FormationDisbanded += this.FormationDisbanded;
        }

        private void FormationDisbanded(ICampaignFormation formation, bool killed) {

            if (killed) {
                IEnumerator KillFormation() {
                    // TODO: Show visually
                    yield return 0;
                    this.RemoveHandle?.Invoke(this);
                }
                Coroutine.StartCoroutine(KillFormation(), this.ThreadDispatcher);
            } else {
                this.ThreadDispatcher.Invoke(() => {
                    this.RemoveHandle?.Invoke(this);
                });
            }
        }

        private void FormationMoved(ICampaignFormation formation, ICampaignMapNode origin, ICampaignMapNode destination) {
            
            // Update formation, visually
            this.ThreadDispatcher.Invoke(() => {

                // Get target
                var targetModel = this.NodeModelFetcher(destination);

                // Get next offset
                (double x, double y) = targetModel.GetNextOffset(true);
                (x, y) = targetModel.GetRelative(x, y);

                // Show visually
                (this as ICampaignMapVisual).GotoPosition(x, y);

            });

            // Delayed update of origin
            IEnumerator UpdateOldNode() {
                yield return new WaitTimespan(TimeSpan.FromSeconds(0.5));
                if (origin.Occupants.Count > 0) {
                    var ogNode = this.NodeModelFetcher(origin);
                    ogNode.ResetOffset();
                    var remainingOccupants = origin.Occupants.Select(a => this.UnitModelFetcher(a)).ForEach(b => {
                        (double x, double y) = ogNode.GetNextOffset(true);
                        (x, y) = ogNode.GetRelative(x, y);
                        b.GotoPosition(x, y, TimeSpan.FromMilliseconds(50));
                    });
                }
            }

            // Tell coroutine to activate
            Coroutine.StartCoroutine(UpdateOldNode(), this.ThreadDispatcher);

        }

    }

}
