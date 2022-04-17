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
        private bool m_isReady;

        public AppDisplayState DisplayState => this.m_displayState;

        public event OnWindowReady? Ready;

        public MainWindow() {

            // Clear is ready
            this.m_isReady = false;

            // Initialize components etc...
            InitializeComponent();

            // Set self display state
            this.m_displayState = AppDisplayState.LeftRight;

        }

        // Hanlde view state change requests
        public override bool StateChangeRequest(object request) {
            return true;
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

        public void SetLeftPanel(object? lhs) {
            this.Dispatcher.Invoke(() => {
                this.LeftContent.Content = null; // Trigger a clear before we set to lhs
                this.LeftContent.Content = lhs;
            });
        }

        public void SetRightPanel(object? rhs) {
            this.Dispatcher.Invoke(() => {
                this.RightContent.Content = null; // Trigger a clear before we set to rhs
                this.RightContent.Content = rhs;
            });
        }

        public void SetFull(object? full) {
            this.Dispatcher.Invoke(() => {
                this.LeftContent.Content = null; // Trigger a clear before we set to lhs
                this.LeftContent.Content = full;
            });
        }

        /// <summary>
        /// Show or hide left-side panel. Use when additional space is required and it does not make sense to expose other data.
        /// </summary>
        /// <param name="show">Show the left-side panel</param>
        [Obsolete]
        public void ShowLeftPanel(bool show) {
        }

    }

}
