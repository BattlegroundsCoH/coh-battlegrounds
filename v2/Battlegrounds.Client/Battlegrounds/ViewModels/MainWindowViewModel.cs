using System.ComponentModel;
using System.Windows.Controls;

using Battlegrounds.Factories;
using Battlegrounds.Helpers;
using Battlegrounds.Models.Playing;
using Battlegrounds.Services;
using Battlegrounds.Views;

using CommunityToolkit.Mvvm.Input;

using Microsoft.Extensions.DependencyInjection;

namespace Battlegrounds.ViewModels;

public sealed class MainWindowViewModel : IDialogHost, INotifyPropertyChanged {

    private readonly IServiceProvider _serviceProvider;

    private object? _dialogContent = null;
    private UserControl? _currentContent = null;

    public event PropertyChangedEventHandler? PropertyChanged;

    public HomeView HomeView => _serviceProvider.GetRequiredService<HomeView>();

    public MultiplayerView MultiplayerView => _serviceProvider.GetRequiredService<MultiplayerView>();

    public CompanyBrowserView CompanyBrowserView => _serviceProvider.GetRequiredService<CompanyBrowserView>();

    public NewsView NewsView => _serviceProvider.GetRequiredService<NewsView>();

    /// <summary>
    /// The settings page. Cached, unlike its siblings above: both <see cref="SettingsView"/> and its
    /// view-model are registered transient and the view-model rebuilds every section from
    /// <c>Configuration</c> in its constructor, so resolving a fresh instance each time the binding
    /// re-evaluates would silently throw away the user's unsaved edits.
    /// </summary>
    public SettingsView SettingsView => field ??= _serviceProvider.GetRequiredService<SettingsView>();

    public UserViewModel UserViewModel => _serviceProvider.GetRequiredService<UserViewModel>();

    public LoginViewModel LoginViewModel { get; }

    public object? DialogContent {
        get => _dialogContent;
        private set {
            if (_dialogContent == value)
                return;
            _dialogContent = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DialogContent)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(HasDialog)));
        }
    }

    public bool HasDialog => _dialogContent != null;

    public UserControl? CurrentContent {
        get => _currentContent;
        set {
            if (_currentContent == value)
                return;
            _currentContent = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentContent)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(HasMainContent)));
        }
    }

    public bool HasMainContent => _currentContent is not null;

    public bool IsHomeButtonActive {
        get;
        set {
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsHomeButtonActive)));
            if (value && HomeView is { DataContext: HomeViewModel homeViewModel }) {
                homeViewModel.Refresh();
            }
        }
    } = true;

    public bool IsMultiplayerButtonActive {
        get;
        set {
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsMultiplayerButtonActive)));
        }
    } = false;

    public bool IsNewsButtonActive {
        get;
        set {
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsNewsButtonActive)));
        }
    } = false;

    public IAsyncRelayCommand SingleplayerCommand { get; }

    public IAsyncRelayCommand LogoutCommand { get; }

    public IAsyncRelayCommand ShowNewsCommand { get; }

    public MainWindowViewModel(IServiceProvider serviceProvider, LoginViewModel loginViewModel) {
        _serviceProvider = serviceProvider;
        _serviceProvider.GetRequiredService<IDialogService>().RegisterHost(this);
        SingleplayerCommand = new AsyncRelayCommand(StartSingleplayerLobby);
        LogoutCommand = new AsyncRelayCommand(LogoutAsync);
        ShowNewsCommand = new AsyncRelayCommand(ShowNews);
        LoginViewModel = loginViewModel ?? throw new ArgumentNullException(nameof(loginViewModel));
    }

    private Task ShowNews() {
        IsNewsButtonActive = true;
        return Task.CompletedTask;
    }

    public void PresentDialog(object dialog) {
        DialogContent = dialog;
    }

    public void CloseDialog() {
        DialogContent = null;
    }

    public void SetContent(UserControl? view) {
        CurrentContent = view;
    }

    public void GoBack() {
        // This method is intended to navigate back to the previous content
        // In this case, it will set the content to the CompanyBrowserView
        SetContent(null); // Currently, we don't have a back stack, so we just clear the content
    }

    private Task LogoutAsync() {
        // Every state change here must happen before the first await, so the window renders the login view once
        Task logout = UserViewModel.LogoutAsync();
        SetContent(null);
        IsHomeButtonActive = true; // Also unchecks SETTINGS, so the next login lands on Home
        return logout;
    }

    private async Task StartSingleplayerLobby() {

        var lobby = await _serviceProvider.GetRequiredService<ILobbyService>().CreateLobbyAsync("Private Skirmish", null, false, _serviceProvider.GetRequiredService<IGameService>().GetGame<CoH3>());
        if (lobby is null) {
            return;
        }

        // Set lobby view as content
        SetContent(LobbyViewFactory.CreateLobbyViewForLobby(_serviceProvider, lobby));

    }

}
