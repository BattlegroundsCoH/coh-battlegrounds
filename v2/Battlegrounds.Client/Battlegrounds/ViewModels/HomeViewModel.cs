using System.ComponentModel;

namespace Battlegrounds.ViewModels;

public sealed class HomeViewModel : INotifyPropertyChanged {

    public event PropertyChangedEventHandler? PropertyChanged;

    private readonly UserViewModel _userViewModel;

    public string WelcomeMessage => $"Welcome back, {_userViewModel.LocalUser?.UserDisplayName ?? "Commander"}!";

    public int TotalMatches { get; private set; } = 0;
    public int TotalVictories { get; private set; } = 0;
    public int WinRate => TotalMatches > 0 ? (int)((double)TotalVictories / TotalMatches * 100) : 0;
    public string Rank { get; private set; } = "Recruit";

    public HomeViewModel(UserViewModel userViewModel) {
        _userViewModel = userViewModel ?? throw new ArgumentNullException(nameof(userViewModel));
        LoadPlayerStats();
    }

    private void LoadPlayerStats() {
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
