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
using BattlegroundsApp.Dialogs.NewCampaign;
using BattlegroundsApp.LocalData;
using BattlegroundsApp.Models.Campaigns;

namespace BattlegroundsApp.Views {
    
    /// <summary>
    /// Interaction logic for CampaignView.xaml
    /// </summary>
    public partial class CampaignBrowserView : ViewState {
        
        public MainWindow MainWindow { get; set; }

        public CampaignBrowserView() {
            InitializeComponent();
        }

        public override void StateOnFocus() { 
        
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

                if (state == NewCampaignDialogResult.HostCampaign) {



                } else if (state == NewCampaignDialogResult.NewSingleplayer) {



                }

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
