using System.ComponentModel;

using Battlegrounds.Models;
using Battlegrounds.Services;

namespace Battlegrounds.ViewModels;

public sealed class HomeViewModel : INotifyPropertyChanged {

    public event PropertyChangedEventHandler? PropertyChanged;

    private string _welcomeMessage = "Welcome back, Commander!";

    public string WelcomeMessage => _welcomeMessage;

    public int TotalMatches { get; private set; } = 0;
    public int TotalVictories { get; private set; } = 0;
    public int WinRate => TotalMatches > 0 ? (int)((double)TotalVictories / TotalMatches * 100) : 0;
    public string Rank { get; private set; } = "Recruit";

    public HomeViewModel() {
        OnViewModelInitialized();
    }

    public void UpdateUser(User user) {
        _welcomeMessage = $"Welcome back, {user?.UserDisplayName ?? "Commander"}!";
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(WelcomeMessage)));
    }

    private async void OnViewModelInitialized() {
        TotalMatches = 42;
        TotalVictories = 28;
        Rank = "Sergeant";

        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(WelcomeMessage)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TotalMatches)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TotalVictories)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(WinRate)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Rank)));
    }

}
