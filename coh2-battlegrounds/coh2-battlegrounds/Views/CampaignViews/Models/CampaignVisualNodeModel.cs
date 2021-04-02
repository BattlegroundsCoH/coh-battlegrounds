using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

using Battlegrounds.Campaigns;
using Battlegrounds.Campaigns.API;
using Battlegrounds.Campaigns.Organisations;
using Battlegrounds.Functional;

namespace BattlegroundsApp.Views.CampaignViews.Models {

    public class CampaignVisualNodeModel : IVisualCampaignNode, ICampaignMapNode {
        
        public UIElement VisualElement { get; }

        public event Action<CampaignMapNode, bool> NodeClicked;

        public CampaignMapNode Node { get; }

        public CampaignResourceContext ResourceContext { get; }

        public Func<Formation, CampaignUnitFormationModel> ModelFromFormation { get; init; }

        private double m_formationXOffset;
        private double m_formationYOffset;

        public CampaignVisualNodeModel(CampaignMapNode node, CampaignResourceContext resourceContext) {
            
            // Set fields and properties
            this.Node = node;
            this.ResourceContext = resourceContext;
            this.m_formationXOffset = 0.0;
            this.m_formationYOffset = 0.0;

            // Init UI
            var visual = this.ResourceContext.GetResource($"{(node.Owner == CampaignArmyTeam.TEAM_ALLIES ? "allies" : "axis")}_node");
            this.VisualElement = visual is null ? new Ellipse {
                Width = 32,
                Height = 32,
            } : new Image() { 
                Source = visual,
                Width = 32,
                Height = 32
            };

            // Subscribe to events
            this.OwnershipChanged(node.Owner);
            this.VisualElement.MouseLeftButtonDown += (a, b) => this.LeftClick();
            this.VisualElement.MouseRightButtonDown += (a, b) => this.RightClick();

            // Set position of node
            (this as ICampaignMapNode).SetPosition(node.U * resourceContext.MapWidth - (32.0 / 2.0), node.V * resourceContext.MapHeight - (32.0 / 2.0));

        }

        public void OwnershipChanged(CampaignArmyTeam armyTeam) { 
            if (this.VisualElement is Ellipse ellipse) {
                ellipse.Fill = armyTeam == CampaignArmyTeam.TEAM_ALLIES ? Brushes.Red : (armyTeam == CampaignArmyTeam.TEAM_AXIS ? Brushes.Green : Brushes.Gray);
            } else if (this.VisualElement is Image img) {
                img.Source = this.ResourceContext.GetResource($"{(this.Node.Owner == CampaignArmyTeam.TEAM_ALLIES ? "allies" : "axis")}_node");
            }
            this.ResourceContext.Controller.FireEvent(ActiveCampaignEventManager.ET_Ownership, this.Node);
        }

        public void VictoryValueChanged(double newValue) {

        }

        public void AttritionValueChanged(double newValue) {

        }

        public void OccupantAdded(Formation formation) {
        }

        public void OccupantRemoved(Formation formation) {
            if (this.Node.Occupants.Count == 0) {
                this.ResetOffset();
            }
        }

        public void LeftClick() => this.NodeClicked?.Invoke(this.Node, false);

        public void RightClick() => this.NodeClicked?.Invoke(this.Node, true);

        public void ResetOffset() {
            this.m_formationXOffset = 0.0;
            this.m_formationYOffset = 0.0;
        }

        public void RefreshFormations(IEnumerable<CampaignUnitFormationModel> models) {
            models.ForEach(q => {
                if (q.Formation.Node == this.Node) {
                    (double x, double y) = this.GetNextOffset(true);
                    (x, y) = this.GetRelative(x, y);
                    (q as ICampaignMapVisual).SetPosition(x, y);
                }
            });
        }

        public (double x, double y) GetNextOffset(bool increment) {
            (double, double) tuple = (this.m_formationXOffset, this.m_formationYOffset);
            if (increment) {
                this.m_formationXOffset += 26.0;
                if (this.m_formationXOffset > 26.0 * 3) {
                    this.m_formationXOffset = 0.0;
                    this.m_formationYOffset += 34.0;
                }
            }
            return tuple;
        }

        public (double x, double y) GetRelative(double x, double y) {
            (double x, double y) tuple = (x, y);
            double nx = this.Node.U * this.ResourceContext.MapWidth;
            double ny = this.Node.V * this.ResourceContext.MapHeight;
            if (tuple.x + nx + 24.0 >= this.ResourceContext.MapWidth) {
                tuple.x = nx - (tuple.x + 24.0);
            } else {
                tuple.x += nx;
            }
            if (tuple.y + ny + 34.0 >= this.ResourceContext.MapHeight) {
                tuple.y = ny - (tuple.y + 34.0);
            } else {
                tuple.y += ny;
            }
            return tuple;
        }

    }

}
