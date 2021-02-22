using System;
using System.Collections.Generic;
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
    public partial class CampaignMapView : ViewState {
        
        public ICampaignController Controller { get; }

        public ImageSource CampaignMapImage { get; }

        public double CampaignMapWidth => this.CampaignMapImage.Width;

        public double CampaignMapHeight => this.CampaignMapImage.Height;

        public CampaignMapView(ICampaignController controller) {

            // Assign controller
            this.Controller = controller;

            // Init components
            InitializeComponent();

            // Init data
            this.CampaignMapImage = PngImageSource.FromMemory(controller.Campaign.PlayMap.RawImageData);
            this.CreateNodeNetwork();

            if (controller.IsSingleplayer) {
                this.CampaignChat.Visibility = Visibility.Collapsed;
                // TODO: Expand selection view
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
                    Fill = Brushes.Orange,
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

        private void NodeClicked(object sender, MouseButtonEventArgs e) {
            if (sender is Ellipse ellipse && ellipse.Tag is CampaignMapNode node) {

            }
        }

        private static void SetPosition(UIElement element, double x, double y) {
            element.SetValue(Canvas.LeftProperty, x);
            element.SetValue(Canvas.TopProperty, y);
            element.SetValue(Panel.ZIndexProperty, 100);
        }

    }

}
