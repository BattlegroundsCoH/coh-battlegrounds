using System;

using Battlegrounds;

using BattlegroundsApp.MVVM;
using BattlegroundsApp.Modals;
using BattlegroundsApp.Modals.Dialogs.MVVM.Models;
using System.ComponentModel;
using BattlegroundsApp.Lobby.MVVM.Models;
using BattlegroundsApp.CompanyEditor.MVVM.Models;
using System.Windows;

namespace BattlegroundsApp {

    public delegate void OnWindowReady(MainWindow window);

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {

        private AppDisplayState m_displayState;
        private bool m_isReady;

        public AppDisplayState DisplayState => this.m_displayState;

        public event OnWindowReady? Ready;

        public MainWindow() {

            // Clear is ready
            this.m_isReady = false;

            // Initialize components etc...
            this.InitializeComponent();

            // Set self display state
            this.m_displayState = AppDisplayState.LeftRight;

            this.DataContext = new MainWindowViewModel(this);

        }

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
        public void ShowLeftPanel(bool show) {}

        private void CoreAppWindow_Closing(object sender, CancelEventArgs e) {

            // Get current rhs
            if (this.RightContent.Content is LobbyModel lobby) {
                e.Cancel = true;
                YesNoDialogViewModel.ShowModal(this.ModalView, (_, res) => {
                    if (res is ModalDialogResult.Confirm) {
                        this.RightContent.Content = null;
                        Environment.Exit(0);
                    }
                }, "Leave Lobby?", "Are you sure you want to leave the lobby?");
                return;
            } else if (this.RightContent.Content is CompanyBuilderViewModel cb) {
                if (cb.HasChanges) {
                    e.Cancel = true;
                    YesNoDialogViewModel.ShowModal(this.ModalView, (_, res) => {
                        if (res is ModalDialogResult.Confirm) {
                            this.RightContent.Content = null;
                            Environment.Exit(0);
                        }
                    }, "Unsaved Changes", "You have unsaved changes that will be lost if you exit now.");
                }
            }

        }

    }

}
