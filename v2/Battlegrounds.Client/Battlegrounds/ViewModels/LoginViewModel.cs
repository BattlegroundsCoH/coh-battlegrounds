using System.ComponentModel;
using System.Windows.Controls;

using Battlegrounds.Models;
using Battlegrounds.Services;

using CommunityToolkit.Mvvm.Input;

using Microsoft.Extensions.Logging;

namespace Battlegrounds.ViewModels;

public sealed class LoginViewModel : INotifyPropertyChanged {

    public event PropertyChangedEventHandler? PropertyChanged;
    
    private readonly ILogger<LoginViewModel> _logger;
    private readonly UserViewModel _userViewModel;
    private readonly HomeViewModel _homeViewModel;
    private readonly IUserService _userService;
    private readonly IBrowserService _browserService;

    private string _username = string.Empty;
    private string _errorMessage = string.Empty;
    private bool _isLoggingIn;
    
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
    
    public bool IsLoggingIn {
        get => _isLoggingIn;
        set {
            if (_isLoggingIn != value) {
                _isLoggingIn = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsLoggingIn)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CanLogin)));
            }
        }
    }
    
    public bool CanLogin => !string.IsNullOrWhiteSpace(Username) && !IsLoggingIn;
    
    public IRelayCommand LoginCommand { get; }

    public IAsyncRelayCommand ContinueWithDiscordCommand { get; }
    
    public IAsyncRelayCommand ContinueWithSteamCommand { get; }

    public LoginViewModel(ILogger<LoginViewModel> logger, UserViewModel userViewModel, HomeViewModel homeViewModel, IUserService userService, IBrowserService browserService) {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _userViewModel = userViewModel ?? throw new ArgumentNullException(nameof(userViewModel));
        _homeViewModel = homeViewModel ?? throw new ArgumentNullException(nameof(homeViewModel));
        _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        _browserService = browserService ?? throw new ArgumentNullException(nameof(browserService));

        LoginCommand = new RelayCommand<PasswordBox>(ExecuteLogin);
        ContinueWithDiscordCommand = new AsyncRelayCommand(ContinueWithDiscordAsync);
        ContinueWithSteamCommand = new AsyncRelayCommand(ContinueWithSteamAsync);
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

    private async Task ContinueWithDiscordAsync() {
        _logger.LogInformation("Continuing login with Discord...");
        try {
            var user = await _userService.LoginWithDiscordAsync();
            if (user is not null) {
                NotifyUserLoggedIn(user);
            } else {
                ErrorMessage = "Discord login failed. Please try again.";
            }
        } catch (Exception ex) {
            ErrorMessage = $"Discord login error: {ex.Message}";
        }
    }

    private async Task ContinueWithSteamAsync() {
        _logger.LogInformation("Continuing login with Steam...");
        throw new NotImplementedException("Steam login is not implemented yet.");
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

    public void RedirectRegisterNow() {
        _logger.LogInformation("Redirecting to registration page...");
        _browserService.OpenUrl("https://cohbattlegrounds.com/register");
    }

    private void NotifyUserLoggedIn(User user) {
        _userViewModel.UpdateUser(user);
        _homeViewModel.UpdateUser(user);
    }

}
