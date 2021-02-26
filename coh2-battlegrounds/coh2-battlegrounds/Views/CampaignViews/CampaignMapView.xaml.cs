using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using Battlegrounds.Campaigns;
using Battlegrounds.Campaigns.Controller;

using BattlegroundsApp.Resources;

namespace BattlegroundsApp.Views.CampaignViews {
    
    /// <summary>
    /// Interaction logic for CampaignMapView.xaml
    /// </summary>
    public partial class CampaignMapView : ViewState, INotifyPropertyChanged {

        private List<CampaignUnitFormationView> m_formationViews;

        public ICampaignController Controller { get; }

        public ImageSource CampaignMapImage { get; }

        public double CampaignMapWidth => this.CampaignMapImage.Width;

        public double CampaignMapHeight => this.CampaignMapImage.Height;

        public string CampaignDate => this.Controller.Campaign.Turn.Date;

        public event PropertyChangedEventHandler PropertyChanged;

        public CampaignMapView(ICampaignController controller) {

            // Assign controller
            this.Controller = controller;

            // Init list
            this.m_formationViews = new List<CampaignUnitFormationView>();

            // Init components
            InitializeComponent();

            // Init data
            this.CampaignMapImage = PngImageSource.FromMemory(controller.Campaign.PlayMap.RawImageData);
            this.CreateNodeNetwork();
            this.RefreshDisplayedFormations();

            // Hide chat control if singleplayer
            if (controller.IsSingleplayer) {
                this.CampaignChat.Visibility = Visibility.Collapsed;
                // TODO: Expand selection view
            } else {

            }

        }

        public override void StateOnFocus() {}

        public override void StateOnLostFocus() {}

        private void CreateNodeNetwork() {

            // Loop through all nodes
            this.Controller.Campaign.PlayMap.EachNode(n => {

                // Create figure and add to canvas
                Ellipse node = new Ellipse {
                    Width = 32,
                    Height = 32,
                    Fill = n.Owner == CampaignArmyTeam.TEAM_ALLIES ? Brushes.Red : (n.Owner == CampaignArmyTeam.TEAM_AXIS ? Brushes.Green : Brushes.Gray),
                    Tag = n,
                };

                // Add events
                node.MouseDown += this.NodeClicked;

                // Add node
                this.CampaignMapCanvas.Children.Add(node);

                // Set visual
                n.VisualNode = node;

                // Set position of node
                SetPosition(node, n.U * this.CampaignMapWidth - (32.0 / 2.0), n.V * this.CampaignMapHeight - (32.0 / 2.0));

            });

            // Loop through all transitions
            this.Controller.Campaign.PlayMap.EachTransition(t => {

                // Create line object
                Line line = new Line {
                    X1 = t.From.U * this.CampaignMapWidth,
                    Y1 = t.From.V * this.CampaignMapHeight,
                    X2 = t.To.U * this.CampaignMapWidth,
                    Y2 = t.To.V * this.CampaignMapHeight,
                    Stroke = Brushes.Black,
                    StrokeThickness = 1.75,
                    Tag = t
                };

                // Add line
                this.CampaignMapCanvas.Children.Add(line);

                // Set Z-index
                line.SetValue(Panel.ZIndexProperty, 99);

            });

        }

        private void RefreshDisplayedFormations() {

            // Clear all formations
            this.ClearFormations();

            // Loop through all formations and display them
            this.Controller.Campaign.PlayMap.EachFormation(f => { 
            

            
            });

        }

        private void ClearFormations() {
            foreach (var form in this.m_formationViews) {
                this.CampaignMapCanvas.Children.Remove(form.Element);
            }
            this.m_formationViews.Clear();
        }

        private void NodeClicked(object sender, MouseButtonEventArgs e) {
            if (sender is Ellipse ellipse && ellipse.Tag is CampaignMapNode node) {

            }
        }

        private static void SetPosition(UIElement element, double x, double y) {
            element.SetValue(Canvas.LeftProperty, x);
            element.SetValue(Canvas.TopProperty, y);
            element.SetValue(Panel.ZIndexProperty, 100);
        }

        private void EndTurnBttn_Click(object sender, RoutedEventArgs e) {
            if (!this.Controller.EndTurn()) {
                this.Controller.EndCampaign();
            }
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.CampaignDate)));
        }

        private void LeaveAndSaveButton_Click(object sender, RoutedEventArgs e) {

        }

    }

}
