using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

using BattlegroundsApp.LocalData;
using BattlegroundsApp.Dialogs.NewCampaign;
using BattlegroundsApp.Models.Campaigns;
using BattlegroundsApp.Views.CampaignViews;

namespace BattlegroundsApp.Views {
    
    /// <summary>
    /// Interaction logic for CampaignView.xaml
    /// </summary>
    public partial class CampaignBrowserView : ViewState {

        private ViewState m_actualState;

        public bool CanContinueCampaign { get; set; }

        public MainWindow MainWindow { get; set; }

        public CampaignBrowserView() {
            
            InitializeComponent();
            
            this.CanContinueCampaign = false;
            
            this.m_actualState = null;

        }

        public override void StateOnFocus() {

            if (this.m_actualState is null) {

                // TODO: Read from BG instance if there's a "latest played" campaign.

            } else {

                this.StateChangeRequest?.Invoke(this.m_actualState);

            }

        }

        public override void StateOnLostFocus() { 
        
        }

        private void ContinueCampaignButton_Click(object sender, RoutedEventArgs e) {
            
            // Hide Left Panel
            MainWindow.ShowLeftPanel(false);

        }

        private void LoadCampaignButton_Click(object sender, RoutedEventArgs e) {

        }

        private void NewCampaignButton_Click(object sender, RoutedEventArgs e) {

            //Show dialog and retrieve data
            var state = NewCampaignDialogViewModel.ShowHostGameDialog("Begin New Campaign", out NewCampaignData campaignData, PlayerCampaigns.CampaignPackages.ToArray());
            if (state != NewCampaignDialogResult.Cancel && campaignData.CampaignToLoad is not null) {

                // Hide the left panel
                this.MainWindow.ShowLeftPanel(false);

                // View for displaying map data
                CampaignMapView mapView = null;

                if (state == NewCampaignDialogResult.HostCampaign) {



                } else if (state == NewCampaignDialogResult.NewSingleplayer) {

                    // Create campaign data and controller
                    var activeCampaign = ActiveCampaign.FromPackage(campaignData.CampaignToLoad, CampaignMode.Singleplayer, campaignData.CampaignDifficulty, campaignData.CampaignHostSide);
                    var singleController = new SingleplayerCampaign(activeCampaign);

                    // Create view with controller
                    mapView = new CampaignMapView(singleController);

                } else {
                    throw new NotImplementedException();
                }

                // Set actual state and switch to that
                this.m_actualState = mapView;
                this.StateChangeRequest?.Invoke(this.m_actualState);

            }

        }

        private void JoinCampaignButton_Click(object sender, RoutedEventArgs e) {

        }

        private void OnlineCampaignView_MouseDoubleClick(object sender, MouseButtonEventArgs e) {

        }

        private void SelfCampaignView_MouseDoubleClick(object sender, MouseButtonEventArgs e) {

        }

    }

}
