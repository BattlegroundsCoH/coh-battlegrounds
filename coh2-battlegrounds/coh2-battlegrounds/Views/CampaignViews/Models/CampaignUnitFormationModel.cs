using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Battlegrounds.Campaigns.API;
using Battlegrounds.Campaigns.Organisations;
using Battlegrounds.Functional;
using Battlegrounds.Util.Coroutines;
using BattlegroundsApp.Utilities;

namespace BattlegroundsApp.Views.CampaignViews.Models {
    
    public class CampaignUnitFormationModel : ICampaignMapVisual, ICampaignSelectable {
    
        public ICampaignFormation Formation { get; }

        public UIElement VisualElement { get; }

        public Func<ICampaignMapNode, ICampaignPointsNode> NodeModelFetcher { get; init; }

        public Func<ICampaignFormation, ICampaignMapVisual> UnitModelFetcher { get; init; }

        public Action<CampaignUnitFormationModel> RemoveHandle { get; init; }

        public GUIThreadDispatcher ThreadDispatcher { get; init; }

        public string Title => this.Formation.Army;

        public string Description => "No description available";

        public CampaignResourceContext ResourceContext { get; }

        public CampaignUnitFormationModel(UIElement element, ICampaignFormation formation, CampaignResourceContext resourceContext) {
            this.VisualElement = element;
            this.Formation = formation;
            this.Formation.FormationMoved += this.FormationMoved;
            this.Formation.FormationDisbanded += this.FormationDisbanded;
            this.ResourceContext = resourceContext;
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

        public CampaignSelectableInfoSection[] GetInfoSections() {
            List<CampaignSelectableInfoSection> infos = new List<CampaignSelectableInfoSection>() {
                new CampaignSelectableInfoSection(this.ResourceContext.GetResource("unit_infantry"), 24, 24, $"{(this.Formation.CalculateStrength()*100.0):0}% Combat Strength")
            };

            int infantry = this.Formation.Regiments.Sum(x => x.CountType(Regiment.UT_INFANTRY));
            int support = this.Formation.Regiments.Sum(x => x.CountType(Regiment.UT_SUPPORT));
            int vehicle = this.Formation.Regiments.Sum(x => x.CountType(Regiment.UT_VEHICLE));
            int tank = this.Formation.Regiments.Sum(x => x.CountType(Regiment.UT_TANK));
            int air = this.Formation.Regiments.Sum(x => x.CountType(Regiment.UT_AIR));

            if (infantry >= 1) {
                infos.Add(new CampaignSelectableInfoSection(this.ResourceContext.GetResource("unit_infantry"), 24, 24, $"{infantry} Infantry units available."));
            }

            if (support >= 1) {
                infos.Add(new CampaignSelectableInfoSection(this.ResourceContext.GetResource("unit_support"), 24, 24, $"{support} Support units available."));
            }
            
            if (vehicle >= 1) {
                infos.Add(new CampaignSelectableInfoSection(this.ResourceContext.GetResource("unit_vehicle"), 24, 24, $"{vehicle} Vehicle units available."));
            }
            
            if (tank >= 1) {
                infos.Add(new CampaignSelectableInfoSection(this.ResourceContext.GetResource("unit_tank"), 24, 24, $"{tank} Armour units available."));
            }
            
            if (air >= 1) {
                infos.Add(new CampaignSelectableInfoSection(this.ResourceContext.GetResource("unit_air"), 24, 24, $"{air} Air units available."));
            }

            return infos.ToArray();
        }

    }

}
