using System.ComponentModel;

using Battlegrounds.Models;
using Battlegrounds.Services;

namespace Battlegrounds.ViewModels;

public sealed class UserViewModel : INotifyPropertyChanged {
    
    public event PropertyChangedEventHandler? PropertyChanged;

    private readonly IUserService _userService;

    private User? _localUser;

    public bool IsAuthenticated => LocalUser is not null;

    public User? LocalUser {
        get => _localUser;
        private set {
            if (_localUser == value)
                return;
            _localUser = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(LocalUser)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsAuthenticated)));
        }
    }

    public UserViewModel(IUserService userService) {
        _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        OnViewModelInitialized();
    }

    private async void OnViewModelInitialized() {
        LocalUser = await _userService.GetLocalUserAsync();
    }
    
    // Method to update the user after successful login from LoginViewModel
    public void UpdateUser(User user) {
        LocalUser = user;
    }
    
    public async Task LogoutAsync() {
        await _userService.LogOutAsync();
        LocalUser = null;
    }

}
