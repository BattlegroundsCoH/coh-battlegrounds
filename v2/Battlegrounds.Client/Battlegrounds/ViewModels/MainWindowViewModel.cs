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

    public MultiplayerView MultiplayerView => _serviceProvider.GetRequiredService<MultiplayerView>();

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

    public bool HasMainContent => _currentContent != null;

    public IAsyncRelayCommand SingleplayerCommand { get; }

    public MainWindowViewModel(IServiceProvider serviceProvider, LoginViewModel loginViewModel) {
        _serviceProvider = serviceProvider;
        _serviceProvider.GetRequiredService<IDialogService>().RegisterHost(this);
        SingleplayerCommand = new AsyncRelayCommand(StartSingleplayerLobby);
        LoginViewModel = loginViewModel ?? throw new ArgumentNullException(nameof(loginViewModel));
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

    private async Task StartSingleplayerLobby() {

        var lobby = await _serviceProvider.GetRequiredService<ILobbyService>().CreateLobbyAsync("Private Skirmish", null, false, _serviceProvider.GetRequiredService<IGameService>().GetGame<CoH3>());
        if (lobby is null) {
            return;
        }

        // Set lobby view as content
        SetContent(LobbyViewFactory.CreateLobbyViewForLobby(_serviceProvider, lobby));
    
    }

}
