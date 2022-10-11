using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

using Battlegrounds.UI;
using Battlegrounds.UI.Modals;

using Battlegrounds.UI.Modals.Prompts;
using Battlegrounds.Editor.Pages;
using Battlegrounds.Lobby.Pages;

namespace BattlegroundsApp;

public delegate void OnWindowReady(MainWindow window);

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window, IMainWindow {

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

            // Grab
            if (Application.Current is not IResourceResolver resourceResolver) {
                return;
            }

            // Grab view
            if (resourceResolver.TryFindDataTemplate(rhs.GetType()) is DataTemplate viewTemplate) {

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
            this.RightContent.Content = null;
            this.m_displayState = AppDisplayState.Full;
        });
    }

    private void OnAppClosing(object sender, CancelEventArgs e) {

        // Grab obj
        var target = this.RightContent.Content is FrameworkElement fe ? fe.DataContext : this.RightContent.Content;

        // Get current rhs
        if (target is BaseLobby lobby && !lobby.IsLocal) {
            e.Cancel = true;
            YesNoPrompt.Show(this.ModalView, (_, res) => {
                if (res is ModalDialogResult.Confirm) {
                    this.RightContent.Content = null;
                    Environment.Exit(0);
                }
            }, "Leave Lobby?", "Are you sure you want to leave the lobby?");
            return;
        } else if (target is CompanyEditor cb) {
            if (cb.HasChanges) {
                e.Cancel = true;
                YesNoPrompt.Show(this.ModalView, (_, res) => {
                    if (res is ModalDialogResult.Confirm) {
                        this.RightContent.Content = null;
                        Environment.Exit(0);
                    }
                }, "Unsaved Changes", "You have unsaved changes that will be lost if you exit now.");
            }
        }

    }

    public ContentControl GetLeft() => this.LeftContent;

    public ContentControl GetRight() => this.RightContent;

    public ModalControl? GetModalControl() => this.ModalView;

    public ModalControl? GetRightsideModalControl() => this.RightModalView;

}
