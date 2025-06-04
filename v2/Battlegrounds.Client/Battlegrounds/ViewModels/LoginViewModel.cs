using System.ComponentModel;
using System.Windows.Controls;

using Battlegrounds.Services;

using CommunityToolkit.Mvvm.Input;

namespace Battlegrounds.ViewModels;

public sealed class LoginViewModel : INotifyPropertyChanged {

    public event PropertyChangedEventHandler? PropertyChanged;
    
    private readonly UserViewModel _userViewModel;
    private readonly IUserService _userService;
    
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
    
    public LoginViewModel(UserViewModel userViewModel, IUserService userService) {
        _userViewModel = userViewModel ?? throw new ArgumentNullException(nameof(userViewModel));
        _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        
        LoginCommand = new RelayCommand<PasswordBox>(ExecuteLogin);
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
            if (user != null) {
                _userViewModel.UpdateUser(user);
            } else {
                ErrorMessage = "Login failed. Please check your credentials.";
            }
        } catch (Exception ex) {
            ErrorMessage = $"Login error: {ex.Message}";
        } finally {
            IsLoggingIn = false;
        }
    }

}
