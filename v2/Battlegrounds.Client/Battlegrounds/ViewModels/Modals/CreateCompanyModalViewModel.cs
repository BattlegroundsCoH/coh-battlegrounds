using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Input;

using Battlegrounds.Helpers;
using Battlegrounds.Models.Playing;
using Battlegrounds.Services;

using CommunityToolkit.Mvvm.Input;

namespace Battlegrounds.ViewModels.Modals;

public record CreateCompanyParameters([property:MemberNotNull(nameof(Game))] bool Create, string Name, Game? Game, string Faction);

public sealed class CreateCompanyModalViewModel : INotifyModalDone, INotifyPropertyChanged {
    
    public event ModalDoneEventHandler? ModalDone;
    public event PropertyChangedEventHandler? PropertyChanged;

    private readonly RelayCommand _createCommand;
    private readonly RelayCommand _cancelCommand;

    private readonly IGameService _gameService;

    private string _companyName = "New Company";
    private Game _selectedGame;
    private string _faction = string.Empty;

    public string CompanyName {
        get => _companyName;
        set {
            if (_companyName == value) {
                return;
            }
            _companyName = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CompanyName)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CanCreate)));
            _createCommand.NotifyCanExecuteChanged();
        }
    }

    public ICollection<Game> AvailableGames => _gameService.GetGames();

    public Game SelectedGame {
        get => _selectedGame;
        set {
            if (_selectedGame == value) {
                return;
            }
            _selectedGame = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedGame)));
        }
    }

    public ICollection<string> AvailableFactions => _selectedGame.FactionIds;

    public string SelectedFaction {
        get => _faction;
        set {
            if (_faction == value) {
                return;
            }
            _faction = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedFaction)));
        }
    }

    public bool CanCreate => !string.IsNullOrWhiteSpace(CompanyName);

    public ICommand CreateCommand => _createCommand;
    public ICommand CancelCommand => _cancelCommand;

    public CreateCompanyModalViewModel(IGameService gameService) {
        _createCommand = new RelayCommand(OnCreate, () => CanCreate);
        _cancelCommand = new RelayCommand(OnCancel);
        _gameService = gameService;
        _selectedGame = gameService.GetGame<CoH3>(); // Default to CoH3, can be changed later
        _faction = _selectedGame.FactionIds.FirstOrDefault() ?? string.Empty; // Default to the first faction if available
    }

    private void OnCreate() {
        ModalDone?.Invoke(this, new CreateCompanyParameters(true, CompanyName, SelectedGame, SelectedFaction));
    }

    private void OnCancel() {
        ModalDone?.Invoke(this, new CreateCompanyParameters(false, string.Empty, null, string.Empty));
    }

}
