using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Input;

using Battlegrounds.Extensions;
using Battlegrounds.Helpers;
using Battlegrounds.Models.Companies;
using Battlegrounds.Models.Doctrines;
using Battlegrounds.Models.Playing;
using Battlegrounds.Services;

using CommunityToolkit.Mvvm.Input;

namespace Battlegrounds.ViewModels.Modals;

public record CreateCompanyParameters(
    [property:MemberNotNull(nameof(Game),nameof(CreateCompanyParameters.Doctrine))] bool Create, 
    string Name, 
    Game? Game, 
    string Faction, 
    DoctrineDefinition? Doctrine);

public sealed class CreateCompanyModalViewModel : INotifyModalDone, INotifyPropertyChanged {
    
    public event ModalDoneEventHandler? ModalDone;
    public event PropertyChangedEventHandler? PropertyChanged;

    private readonly RelayCommand _createCommand;
    private readonly RelayCommand _cancelCommand;

    private readonly IGameService _gameService;
    private readonly ICompanyService _companyService;
    private readonly IDoctrineService _doctrineService;

    private ICollection<string> _availableFactions = [];
    private string _companyName = "New Company";
    private Game _selectedGame;
    private string _faction = string.Empty;
    private DoctrineDefinition? _definition;

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

    public ICollection<string> AvailableFactions {
        get => _availableFactions;
        private set {
            if (_availableFactions == value) {
                return;
            }
            _availableFactions = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AvailableFactions)));
        }
    }

    public string SelectedFaction {
        get => _faction;
        set {
            if (_faction == value) {
                return;
            }
            _faction = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedFaction)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AvailableDoctrines)));
            SelectedDoctrine = AvailableDoctrines.FirstOrDefault();
        }
    }

    public ICollection<DoctrineDefinition> AvailableDoctrines => [.. _doctrineService.GetDoctrinesForFaction(SelectedGame.Id, SelectedFaction).Where(x => x.IsVisible)];

    public DoctrineDefinition? SelectedDoctrine {
        get => _definition;
        set {
            if (_definition == value) {
                return;
            }
            _definition = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedDoctrine)));
        }
    }

    public bool CanCreate => !string.IsNullOrWhiteSpace(CompanyName);

    public ICommand CreateCommand => _createCommand;
    public ICommand CancelCommand => _cancelCommand;

    public CreateCompanyModalViewModel(IGameService gameService, ICompanyService companyService, IDoctrineService doctrineService) {
        _createCommand = new RelayCommand(OnCreate, () => CanCreate);
        _cancelCommand = new RelayCommand(OnCancel);
        _gameService = gameService;
        _companyService = companyService;
        _doctrineService = doctrineService;
        _selectedGame = gameService.GetGame<CoH3>(); // Default to CoH3, can be changed later
        OnUpdateFactionOptions();
    }

    private async void OnUpdateFactionOptions() {
        AvailableFactions = await GetAvailableFactions(_selectedGame);
        SelectedFaction = _availableFactions.FirstOrDefault() ?? string.Empty; // Default to the first faction if available
    }

    private void OnCreate() {
        ModalDone?.Invoke(this, new CreateCompanyParameters(true, CompanyName, SelectedGame, SelectedFaction, SelectedDoctrine));
    }

    private void OnCancel() {
        ModalDone?.Invoke(this, new CreateCompanyParameters(false, string.Empty, null, string.Empty, null));
    }

    private async Task<string[]> GetAvailableFactions(Game game) { // Attempts to limit factions to those defined in the game and those that have fewer than 5 companies
        string[] factions = game.FactionIds;
        if (factions.Length == 0) {
            throw new InvalidOperationException($"Game {game.GameName} does not have any factions defined.");
        }
        var companies = await _companyService.GetLocalCompaniesAsync();
        var factionCompanies = new Dictionary<string, IList<Company>>();
        foreach (var company in companies) {
            if (company.GameId == game.Id) {
                factionCompanies.AddOrInitialize(company.Faction, company);
            }
        }
        foreach (var factionId in factions) {
            if (!factionCompanies.ContainsKey(factionId)) {
                factionCompanies.Add(factionId, []);
            }
        }
        return [..from grouped in factionCompanies
                  where grouped.Value.Count < 5
                  select grouped.Key];
    }

}
