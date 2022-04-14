using System.Windows;
using System.Windows.Input;

using Battlegrounds.Locale;
using BattlegroundsApp.CompanyEditor.MVVM.Models;
using BattlegroundsApp.Dashboard.MVVM.Models;
using BattlegroundsApp.Modals;
using BattlegroundsApp.Modals.Dialogs.MVVM.Models;
using BattlegroundsApp.Utilities;

namespace BattlegroundsApp.MVVM.Models {
    
    public class LeftMenuButton {
        public ICommand Click { get; init; }
        public LocaleKey Text { get; init; }
        public LocaleKey Tooltip { get; init; }
        public bool Enabled { get; init; }
    }

    public class LeftMenu : IViewModel {

        public LeftMenuButton Dashboard { get; }
        public LeftMenuButton News { get; }
        public LeftMenuButton CompanyBuilder { get; }
        public LeftMenuButton Campaign { get; }
        public LeftMenuButton MatchFinder { get; }
        public LeftMenuButton Settings { get; }
        public LeftMenuButton Exit { get; }

        public bool SingleInstanceOnly => true;

        public LeftMenu() {

            // Create dashboard
            this.Dashboard = new() {
                Click = new RelayCommand(this.DashboardButton),
                Text = new("MainWindow_Dashboard"),
                Enabled = false
            };

            // Create news
            this.News = new() {
                Click = new RelayCommand(this.NewsButton),
                Text = new("MainWindow_News"),
                Enabled = false
            };
            
            // Create builder
            this.CompanyBuilder = new() {
                Click = new RelayCommand(this.BuilderButton),
                Text = new("MainWindow_Company_Builder"),
                Enabled = true
            };
            
            // Create campaign
            this.Campaign = new() {
                Click = new RelayCommand(this.CampaignButton),
                Text = new("MainWindow_Campaign"),
                Enabled = false
            };
            
            // Create match finder
            this.MatchFinder = new() {
                Click = new RelayCommand(this.MatchFinderButton),
                Text = new("MainWindow_Game_Browser"),
                Enabled = true
            };
            
            // Create settings
            this.Settings = new() {
                Click = new RelayCommand(this.SettingsButton),
                Text = new("MainWindow_Settings"),
                Enabled = false
            };
            
            // Create exit
            this.Exit = new() {
                Click = new RelayCommand(this.ExitButton),
                Text = new("MainWindow_Exit"),
                Enabled = true
            };

        }

        private void DashboardButton() {

            App.ViewManager.UpdateDisplay<DashboardViewModel>(AppDisplayTarget.Right);

        }

        private void NewsButton() {

        }
        
        private void BuilderButton() {

            // Get browser
            var browser = App.ViewManager.UpdateDisplay<CompanyBrowserViewModel>(AppDisplayTarget.Right);

            // Trigger list refresh
            browser.UpdateCompanyList();

        }

        private void CampaignButton() {

        }
        
        private void MatchFinderButton() {

            // Set RHS to lobby browser
            var browser = App.ViewManager.UpdateDisplay<LobbyBrowserViewModel>(AppDisplayTarget.Right);

            // Triger lobby refresh
            browser.RefreshLobbies();

        }

        private void SettingsButton() {

        }

        private void ExitButton() {

            Application.Current.Shutdown();

        }

        public bool UnloadViewModel() => true;

    }

}
