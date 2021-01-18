using Battlegrounds;
using Battlegrounds.Online.Services;
using Battlegrounds.Steam;
using Battlegrounds.Game.Database;
using System;
using System.Collections.Generic;
using System.IO;
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
using System.Diagnostics;

using BattlegroundsApp.Views;
using BattlegroundsApp.Dialogs.YesNo;

namespace BattlegroundsApp {

    public delegate void OnWindowReady(MainWindow window);

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : CoreAppWindow {

        public const string GAMEBROWSERSTATE = "GAMEBROWSERVIEW";
        public const string CAMPAIGNSTATE = "CAMPAIGNVIEW";
        public const string COMPANYSTATE = "COMPANYBUILDERVIEW";
        public const string DASHBOARDSTATE = "DASHBOARDVIEW";
        public const string NEWSSTATE = "NEWSVIEW";

        private bool m_isReady;

        public string DashboardButtonContent { get; }
        public string NewsButtonContent { get; }
        public string CompanyBuilderButtonContent { get; }
        public string CampaignButtonContent { get; }
        public string GameBrowserButtonContent { get; }
        public string SettingsButtonContent { get; }
        public string ExitButtonContent { get; }

        private Dictionary<string, ViewState> m_constStates; // lookup table for all states that don't need initialization.

        public event OnWindowReady Ready;

        public MainWindow() {

            // Clear is ready
            this.m_isReady = false;

            // Initialize components etc...
            InitializeComponent();

            DashboardButtonContent = "Dashboard";
            NewsButtonContent = "News";
            CompanyBuilderButtonContent = "Company Builder";
            CampaignButtonContent = "Campaign";
            GameBrowserButtonContent = "Game Browser";
            SettingsButtonContent = "Settings";
            ExitButtonContent = "Exit";

            // Create all the views that can be created at startup
            this.m_constStates = new Dictionary<string, ViewState> {
                [GAMEBROWSERSTATE] = new GameBrowserView(),
                [CAMPAIGNSTATE] = new CampaignView(),
                [COMPANYSTATE] = new CompanyView(),
                [DASHBOARDSTATE] = new DashboardView(),
                [NEWSSTATE] = new NewsView(),
            };

            // Starts with Dashboard page opened
            this.DataContext = this.m_constStates[DASHBOARDSTATE];

        }

        // Open Dashboard page
        private void Dashboard_Click(object sender, RoutedEventArgs e) => this.State = this.m_constStates[DASHBOARDSTATE];

        // Open News page
        private void News_Click(object sender, RoutedEventArgs e) => this.State = this.m_constStates[NEWSSTATE];

        // Open Division Builder page
        private void CompanyBuilder_Click(object sender, RoutedEventArgs e) => this.State = this.m_constStates[COMPANYSTATE];

        // Open Campaign page
        private void Campaign_Click(object sender, RoutedEventArgs e) => this.State = this.m_constStates[CAMPAIGNSTATE];

        // Open Game Browser page
        private void GameBrowser_Click(object sender, RoutedEventArgs e) => this.State = this.m_constStates[GAMEBROWSERSTATE];

        // Exit application
        private void Exit_Click(object sender, RoutedEventArgs e) {

            var result = YesNoDialogViewModel.ShowYesNoDialog("Exit", "Are you sure?");

            if (result == YesNoDialogResult.Confirm) {

                this.Close();
            }

        }

        // Hanlde view state change requests
        public override bool StateChangeRequest(object request) {
            if (request is ViewState state) {
                this.State = state;
            } else if (request is string view && this.m_constStates.ContainsKey(view)) {
                this.State = this.m_constStates[view];
            } else {
                Trace.WriteLine($"Failed to change state to {request}", "MainWindow");
                return false;
            }
            return true;
        }

        // Get the current state from identifier
        public ViewState GetState(string stateIdentifier) {
            if (this.m_constStates.TryGetValue(stateIdentifier, out ViewState state)) {
                return state;
            } else {
                return null;
            }
        }

        // Get the request handler
        public override StateChangeRequestHandler GetRequestHandler() => this.StateChangeRequest;

        public bool AllowGetSteamUser()
            => YesNoDialogViewModel.ShowYesNoDialog("No Steam User Found", "No Steam user was found on startup. Would you like to have the application find the local Steam user?") == YesNoDialogResult.Confirm;

        protected override void OnContentRendered(EventArgs e) {
            base.OnContentRendered(e);
            if (!this.m_isReady) {
                this.Ready?.Invoke(this);
                this.m_isReady = true;
            }
        }

    }   

}
