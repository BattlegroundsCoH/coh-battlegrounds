using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;

using Battlegrounds.Factories;
using Battlegrounds.Helpers;
using Battlegrounds.Models;
using Battlegrounds.Services;
using Battlegrounds.ViewModels.Modals;
using Battlegrounds.Views.Modals;

using CommunityToolkit.Mvvm.Input;

using Microsoft.Extensions.DependencyInjection;

namespace Battlegrounds.ViewModels;

public sealed class MultiplayerViewModel : INotifyPropertyChanged {

    public event PropertyChangedEventHandler? PropertyChanged;

    private readonly IServiceProvider _serviceProvider;

    private readonly ObservableCollection<BrowserLobby> _lobbies = [];
    private readonly ICollectionView _lobbyView;

    private string _statusMessage = "Connecting to the Battlegrounds server...";
    private bool _isConnected;
    private bool _isLoading;
    private int _playersOnline;

    public bool IsConnected {
        get => _isConnected;
        private set {
            if (_isConnected == value) {
                return;
            }
            _isConnected = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsConnected)));
        }
    }

    public bool IsLoading {
        get => _isLoading;
        private set {
            if (_isLoading == value) {
                return;
            }
            _isLoading = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsLoading)));
        }
    }

    public string StatusMessage {
        get => _statusMessage;
        private set {
            if (_statusMessage == value) {
                return;
            }
            _statusMessage = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(StatusMessage)));
        }
    }

    public int PlayersOnline {
        get => _playersOnline;
        private set {
            if (_playersOnline == value) {
                return;
            }
            _playersOnline = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PlayersOnline)));
        }
    }

    public IAsyncRelayCommand CreateLobby => new AsyncRelayCommand(CreateLobbyAsync);

    public IAsyncRelayCommand RefreshLobbies => new AsyncRelayCommand(RefreshLobbiesAsync);

    public ICollectionView Lobbies => _lobbyView;

    public MultiplayerViewModel(IServiceProvider serviceProvider) {
        _serviceProvider = serviceProvider;
        _lobbyView = CollectionViewSource.GetDefaultView(_lobbies);
        _lobbyView.Filter = item => true;
        InitialiseAsync();
    }

    private async void InitialiseAsync() {
        await RefreshLobbiesAsync();
    }

    private async Task RefreshLobbiesAsync() {
        if (!_isConnected) {
            await CheckConnectionStatusAsync();
            if (!_isConnected) {
                return;
            }
        }
        try {
            IsLoading = true;
            StatusMessage = "Refreshing lobby list ...";

            var lobbies = await _serviceProvider.GetRequiredService<ILobbyBrowserService>().GetLobbiesAsync();
            _lobbies.Clear();
            foreach (var lobby in lobbies) {
                _lobbies.Add(lobby);
            }

            PlayersOnline = _lobbies.Sum(lobby => lobby.CurrentPlayers);
            StatusMessage = $"Found {_lobbies.Count} lobbies with {PlayersOnline} players online.";

        } finally {
            IsLoading = false;
        }
    }

    private async Task CheckConnectionStatusAsync() {
        try {
            IsLoading = true;
            StatusMessage = "Connecting to the Battlegrounds server...";

            IsConnected = await _serviceProvider.GetRequiredService<ILobbyBrowserService>().IsServerAvailableAsync();
            if (IsConnected) {
                StatusMessage = "Connected to the Battlegrounds server.";
            } else {
                StatusMessage = "Unable to connect to the Battlegrounds server.";
            }

        } catch {
            IsConnected = false;
        } finally {
            IsLoading = false;
        }
    }

    private async Task CreateLobbyAsync() {

        var result = await Modal.ShowModalAsync<CreateLobbyModalView, CreateLobbyParameters>(_serviceProvider);
        if (!result.Create) {
            return;
        }

        var lobby = await _serviceProvider.GetRequiredService<ILobbyService>().CreateLobbyAsync(result.Name, result.Password, true);
        if (lobby is null) {
            StatusMessage = "Failed to create lobby.";
            return;
        }

        // Set lobby view as content
        var mainWindow = _serviceProvider.GetRequiredService<MainWindowViewModel>();
        mainWindow.SetContent(LobbyViewFactory.CreateLobbyViewForLobby(_serviceProvider, lobby));

    }

}
