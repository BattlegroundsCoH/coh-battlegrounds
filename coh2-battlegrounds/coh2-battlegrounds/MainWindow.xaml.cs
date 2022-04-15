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
using Battlegrounds.Locale;
using BattlegroundsApp.MVVM;
using BattlegroundsApp.Modals;
using BattlegroundsApp.Modals.Dialogs.MVVM.Models;

namespace BattlegroundsApp {

    public delegate void OnWindowReady(MainWindow window);

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : CoreAppWindow {

        private AppDisplayState m_displayState;

        public AppDisplayState DisplayState => this.m_displayState;

        public const string GAMEBROWSERSTATE = "GAMEBROWSERVIEW";
        public const string CAMPAIGNBORWSESTATE = "CAMPAIGNVIEW";
        public const string COMPANYSTATE = "COMPANYBUILDERVIEW";
        public const string DASHBOARDSTATE = "DASHBOARDVIEW";
        public const string NEWSSTATE = "NEWSVIEW";

        private bool m_isReady;
        private Button[] m_leftPanelButtons;

        public LocaleKey DashboardButtonContent { get; }
        public LocaleKey NewsButtonContent { get; }
        public LocaleKey CompanyBuilderButtonContent { get; }
        public LocaleKey CampaignButtonContent { get; }
        public LocaleKey GameBrowserButtonContent { get; }
        public LocaleKey SettingsButtonContent { get; }
        public LocaleKey ExitButtonContent { get; }

        private Dictionary<string, ViewState> m_constStates; // lookup table for all states that don't need initialization.

        public event OnWindowReady Ready;

        public MainWindow() {

            // Clear is ready
            this.m_isReady = false;

            // Initialize components etc...
            InitializeComponent();

            // Set self display state
            this.m_displayState = AppDisplayState.LeftRight;

            // Define left panels buttons
            this.m_leftPanelButtons = new Button[] {
                /*DashboardButton,
                NewsButton,
                CompanyBuilderButton,
                CampaignButton,
                GameBrowserButton,
                SettingsButton,
                ExitButton*/
            };

            // Define locales
            DashboardButtonContent = new LocaleKey("MainWindow_Dashboard");
            NewsButtonContent = new LocaleKey("MainWindow_News");
            CompanyBuilderButtonContent = new LocaleKey("MainWindow_Company_Builder");
            CampaignButtonContent = new LocaleKey("MainWindow_Campaign");
            GameBrowserButtonContent = new LocaleKey("MainWindow_Game_Browser");
            SettingsButtonContent = new LocaleKey("MainWindow_Settings");
            ExitButtonContent = new LocaleKey("MainWindow_Exit");

            // Create all the views that can be created at startup
            this.m_constStates = new Dictionary<string, ViewState> {
                [GAMEBROWSERSTATE] = new GameBrowserView(),
                [CAMPAIGNBORWSESTATE] = new CampaignBrowserView() { MainWindow = this },
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
        private void Campaign_Click(object sender, RoutedEventArgs e) => this.State = this.m_constStates[CAMPAIGNBORWSESTATE];

        // Open Game Browser page
        private void GameBrowser_Click(object sender, RoutedEventArgs e) => this.State = this.m_constStates[GAMEBROWSERSTATE];

        // Exit application
        private void Exit_Click(object sender, RoutedEventArgs e) {

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
        public ViewState? GetState(string stateIdentifier) {
            if (this.m_constStates.TryGetValue(stateIdentifier, out ViewState? state)) {
                return state;
            } else {
                return null;
            }
        }

        // Get the request handler
        public override StateChangeRequestHandler GetRequestHandler() => this.StateChangeRequest;

        public void AllowGetSteamUser(Action<bool> callback) {

            // Null check
            if (App.ViewManager.GetModalControl() is not ModalControl mControl) {
                callback.Invoke(false);
                return;
            }

            // Lookup strings
            string title = BattlegroundsInstance.Localize.GetString("MainWindow_YesNoDialog_No_Steam_User_Title");
            string desc = BattlegroundsInstance.Localize.GetString("MainWindow_YesNoDialog_No_Steam_User_Message");

            // Do modal
            YesNoDialogViewModel.ShowModal(mControl, (vm, resault) => {

                callback.Invoke(resault is ModalDialogResult.Confirm);

            }, title, desc);

        }
        
        protected override void OnContentRendered(EventArgs e) {
            base.OnContentRendered(e);
            if (!this.m_isReady) {
                this.Ready?.Invoke(this);
                this.m_isReady = true;
            }
        }

        public void SetLeftPanel(object lhs) {
            if (this.m_displayState != AppDisplayState.LeftRight) {
                this.SetDisplayState(AppDisplayState.LeftRight);
            }
            this.Dispatcher.Invoke(() => {
                this.LeftContent.Content = lhs;
            });
        }

        public void SetRightPanel(object rhs) {
            if (this.m_displayState != AppDisplayState.LeftRight) {
                this.SetDisplayState(AppDisplayState.LeftRight);
            }
            this.Dispatcher.Invoke(() => {
                this.RightContent.Content = rhs;
            });
        }

        public void SetFull(object full) {
            if (this.m_displayState != AppDisplayState.Full) {
                this.SetDisplayState(AppDisplayState.Full);
            }
            this.Dispatcher.Invoke(() => {
                this.LeftContent.Content = full;
            });
        }

        private void SetDisplayState(AppDisplayState state) {
            switch (state) {
                case AppDisplayState.LeftRight:
                    break;
                case AppDisplayState.Full:
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Show or hide left-side panel. Use when additional space is required and it does not make sense to expose other data.
        /// </summary>
        /// <param name="show">Show the left-side panel</param>
        public void ShowLeftPanel(bool show) {
            var vs = show ? Visibility.Visible : Visibility.Collapsed;
            foreach (var bttn in this.m_leftPanelButtons) {
                bttn.Visibility = vs;
            }
            if (show) {
                //this.AppContent.SetValue(Grid.ColumnProperty, 2);
                //this.AppContent.SetValue(Grid.ColumnSpanProperty, 7);
            } else {
                //this.AppContent.SetValue(Grid.ColumnProperty, 1);
                //this.AppContent.SetValue(Grid.ColumnSpanProperty, 8);
            }
        }

    }

}
