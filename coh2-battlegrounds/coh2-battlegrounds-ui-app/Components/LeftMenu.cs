using System;
using System.Collections.Generic;
using System.Windows.Input;

using Battlegrounds.Locale;

namespace Battlegrounds.UI.Application.Components;

public sealed record LeftMenuButton(ICommand? Click, LocaleKey? Text, bool Enabled, LocaleKey? Tooltip = null);

public sealed class LeftMenu : ViewModelBase, IMenuController {

    private readonly AppViewManager m_viewManager;
    private readonly Dictionary<MenuButton, MenuButtonCallback> m_buttonCallbacks;

    public LeftMenuButton Dashboard { get; }
    public LeftMenuButton News { get; }
    public LeftMenuButton CompanyBuilder { get; }
    public LeftMenuButton Campaign { get; }
    public LeftMenuButton MatchFinder { get; }
    public LeftMenuButton Settings { get; }
    public LeftMenuButton Exit { get; }

    public override bool KeepAlive => true;

    public override bool SingleInstanceOnly => true;

    public LeftMenu(AppViewManager viewManager) {

        // Create dashboard
        this.Dashboard = new(new RelayCommand(this.DashboardButton), new("MainWindow_Dashboard"), true);

        // Create news
        this.News = new(new RelayCommand(this.NewsButton), new("MainWindow_News"), false);

        // Create builder
        this.CompanyBuilder = new(new RelayCommand(this.BuilderButton), new("MainWindow_Company_Builder"), true);

        // Create campaign
        this.Campaign = new(new RelayCommand(this.CampaignButton), new("MainWindow_Campaign"), false);

        // Create match finder
        this.MatchFinder = new(new RelayCommand(this.MatchFinderButton), new("MainWindow_Game_Browser"), true);

        // Create settings
        this.Settings = new(new RelayCommand(this.SettingsButton), new("MainWindow_Settings"), true);

        // Create exit
        this.Exit = new(new RelayCommand(this.ExitButton), new("MainWindow_Exit"), true);

        // Set view manager
        this.m_viewManager = viewManager;

        // Set callback dictionary
        this.m_buttonCallbacks = new();

    }

    private void InvokeCallback(MenuButton button) {
        if (this.m_buttonCallbacks.TryGetValue(button, out var callback)) {
            callback(this.m_viewManager);
        } else {
            //throw new NotImplementedException();
        }
    }

    private void DashboardButton() {
        InvokeCallback(MenuButton.Dashboard);
    }

    private void NewsButton() {
        InvokeCallback(MenuButton.News);
    }

    private void BuilderButton() {
        InvokeCallback(MenuButton.Companies);
    }

    private void CampaignButton() {
        InvokeCallback(MenuButton.Campaigns);
    }

    private void MatchFinderButton() {
        InvokeCallback(MenuButton.Browser);
    }

    private void SettingsButton() {
        InvokeCallback(MenuButton.Settings);
    }

    private void ExitButton() {

        // Ask main window to close -> this will shut down the application.
        System.Windows.Application.Current.MainWindow.Close();

    }

    public void SetMenuCallback(MenuButton button, MenuButtonCallback callback)
        => this.m_buttonCallbacks[button] = callback;

}
