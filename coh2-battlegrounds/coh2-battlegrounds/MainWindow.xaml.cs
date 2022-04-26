using System;

using Battlegrounds;

using BattlegroundsApp.MVVM;
using BattlegroundsApp.Modals;
using BattlegroundsApp.Modals.Dialogs.MVVM.Models;
using System.ComponentModel;
using BattlegroundsApp.Lobby.MVVM.Models;
using BattlegroundsApp.CompanyEditor.MVVM.Models;
using System.Windows;
using System.Windows.Controls;

namespace BattlegroundsApp;

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

    public void SetLeftPanel(IViewModel? lhs) {
        this.Dispatcher.Invoke(() => {
            this.LeftContent.Content = null; // Trigger a clear before we set to lhs
            this.LeftContent.Content = lhs;
        });
    }

    public void SetRightPanel(IViewModel? rhs) {
        this.Dispatcher.Invoke(() => {

            // Clear
            this.RightContent.Content = null;

            // Bail if done
            if (rhs is null) {
                return;
            }

            // Grab view
            if (App.TryFindDataTemplate(rhs.GetType()) is DataTemplate viewTemplate) {

                // Load view and set datacontext
                var view = viewTemplate.LoadContent();
                view.SetValue(DataContextProperty, rhs);

                // Set content
                this.RightContent.Content = view;

            }

        });
    }

    public void SetFull(IViewModel? full) {
        this.Dispatcher.Invoke(() => {
            this.LeftContent.Content = null; // Trigger a clear before we set to lhs
            this.LeftContent.Content = full;
        });
    }

    private void OnAppClosing(object sender, CancelEventArgs e) {

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
