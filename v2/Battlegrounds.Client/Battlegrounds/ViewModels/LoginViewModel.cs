using System.ComponentModel;
using System.Windows.Controls;

using Battlegrounds.Models;
using Battlegrounds.Services;

using CommunityToolkit.Mvvm.Input;

using Microsoft.Extensions.Logging;

namespace Battlegrounds.ViewModels;

public sealed class LoginViewModel : INotifyPropertyChanged {

    private const string DiscordProviderName = "Discord";
    private const string SteamProviderName = "Steam";

    public event PropertyChangedEventHandler? PropertyChanged;

    private readonly ILogger<LoginViewModel> _logger;
    private readonly UserViewModel _userViewModel;
    private readonly HomeViewModel _homeViewModel;
    private readonly IUserService _userService;

    private string _username = string.Empty;
    private string _errorMessage = string.Empty;
    private string _statusMessage = string.Empty;
    private bool _isLoggingIn;
    private string? _pendingProvider;

    private CancellationTokenSource? _providerLoginCts;

    public string Username {
        get => _username;
        set {
            if (_username != value) {
                _username = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Username)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CanLogin)));
            }
        }
    }

    public string ErrorMessage {
        get => _errorMessage;
        set {
            if (_errorMessage != value) {
                _errorMessage = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ErrorMessage)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(HasError)));
            }
        }
    }

    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

    /// <summary>
    /// What the sign-in is currently waiting on. Empty when nothing is in flight.
    /// </summary>
    public string StatusMessage {
        get => _statusMessage;
        set {
            if (_statusMessage != value) {
                _statusMessage = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(StatusMessage)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(HasStatus)));
            }
        }
    }

    public bool HasStatus => !string.IsNullOrEmpty(StatusMessage);

    public bool IsLoggingIn {
        get => _isLoggingIn;
        set {
            if (_isLoggingIn != value) {
                _isLoggingIn = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsLoggingIn)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CanLogin)));
                ContinueWithDiscordCommand.NotifyCanExecuteChanged();
                ContinueWithSteamCommand.NotifyCanExecuteChanged();
                CancelProviderLoginCommand.NotifyCanExecuteChanged();
            }
        }
    }

    /// <summary>
    /// The provider whose browser sign-in is in flight, or <c>null</c> when none is.
    /// </summary>
    private string? PendingProvider {
        get => _pendingProvider;
        set {
            if (_pendingProvider != value) {
                _pendingProvider = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsDiscordPending)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsSteamPending)));
            }
        }
    }

    public bool IsDiscordPending => PendingProvider == DiscordProviderName;

    public bool IsSteamPending => PendingProvider == SteamProviderName;

    public bool CanLogin => !string.IsNullOrWhiteSpace(Username) && !IsLoggingIn;

    public IRelayCommand LoginCommand { get; }

    public IAsyncRelayCommand ContinueWithDiscordCommand { get; }

    public bool IsDiscordVisible => true; // This can be made dynamic based on configuration or platform

    public IAsyncRelayCommand ContinueWithSteamCommand { get; }

    public IRelayCommand CancelProviderLoginCommand { get; }

    public bool IsPasswordLoginVisible => AppEnvironment.IsDeveloperMode;

    public LoginViewModel(ILogger<LoginViewModel> logger, UserViewModel userViewModel, HomeViewModel homeViewModel, IUserService userService) {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _userViewModel = userViewModel ?? throw new ArgumentNullException(nameof(userViewModel));
        _homeViewModel = homeViewModel ?? throw new ArgumentNullException(nameof(homeViewModel));
        _userService = userService ?? throw new ArgumentNullException(nameof(userService));

        LoginCommand = new RelayCommand<PasswordBox>(ExecuteLogin);
        ContinueWithDiscordCommand = new AsyncRelayCommand(ContinueWithDiscordAsync, () => !IsLoggingIn);
        ContinueWithSteamCommand = new AsyncRelayCommand(ContinueWithSteamAsync, () => !IsLoggingIn);
        CancelProviderLoginCommand = new RelayCommand(CancelProviderLogin, () => IsLoggingIn);
    }

    private async void ExecuteLogin(PasswordBox? passwordBox) {
        if (passwordBox == null) return;

        var password = passwordBox.Password;

        if (string.IsNullOrWhiteSpace(Username)) {
            ErrorMessage = "Username cannot be empty.";
            return;
        }

        if (string.IsNullOrWhiteSpace(password)) {
            ErrorMessage = "Password cannot be empty.";
            return;
        }

        try {
            ErrorMessage = string.Empty;
            IsLoggingIn = true;

            var user = await _userService.LoginAsync(Username, password);
            if (user is not null) {
                NotifyUserLoggedIn(user);
            } else {
                ErrorMessage = "Login failed. Please check your credentials.";
            }
        } catch (Exception ex) {
            ErrorMessage = $"Login error: {ex.Message}";
        } finally {
            IsLoggingIn = false;
        }
    }

    private Task ContinueWithDiscordAsync() => ContinueWithProviderAsync(_userService.LoginWithDiscordAsync, DiscordProviderName);

    private Task ContinueWithSteamAsync() => ContinueWithProviderAsync(_userService.LoginWithSteamAsync, SteamProviderName);

    private async Task ContinueWithProviderAsync(Func<CancellationToken, Task<User>> login, string providerName) {

        _logger.LogInformation("Continuing login with {Provider}...", providerName);

        using CancellationTokenSource cts = new();
        _providerLoginCts = cts;

        try {
            ErrorMessage = string.Empty;
            IsLoggingIn = true;
            PendingProvider = providerName;
            StatusMessage = $"Waiting for you to finish signing in with {providerName} in your browser...";
            NotifyUserLoggedIn(await login(cts.Token));
        } catch (OperationCanceledException) {
            _logger.LogInformation("The {Provider} sign-in was cancelled.", providerName);   // A cancel is not an error.
        } catch (Exception ex) {
            ErrorMessage = $"{providerName} login error: {ex.Message}";
        } finally {
            StatusMessage = string.Empty;
            PendingProvider = null;
            IsLoggingIn = false;
            _providerLoginCts = null;
        }

    }

    private void CancelProviderLogin() {
        _logger.LogInformation("Cancelling the browser sign-in.");
        _providerLoginCts?.Cancel();
    }

    public async Task<bool> AutoLoginAsync() {
        _logger.LogInformation("Attempting to auto-login...");
        if (!await _userService.AutoLoginAsync()) {
            _logger.LogWarning("Auto-login failed. Please log in manually.");
        } else {
            _logger.LogInformation("Auto-login successful.");
            var user = await _userService.GetLocalUserAsync();
            if (user == null) {
                _logger.LogError("Auto-login succeeded but no user data was retrieved.");
                return false;
            }
            NotifyUserLoggedIn(user);
            return true;
        }
        return false;
    }

    private void NotifyUserLoggedIn(User user) {
        _userViewModel.UpdateUser(user);
        _homeViewModel.UpdateUser(user);
    }

}
