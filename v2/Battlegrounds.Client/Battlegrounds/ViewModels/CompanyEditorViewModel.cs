using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Input;

using Battlegrounds.Helpers;
using Battlegrounds.Models.Blueprints;
using Battlegrounds.Models.Blueprints.Extensions;
using Battlegrounds.Models.Companies;
using Battlegrounds.Models.Playing;
using Battlegrounds.Services;
using Battlegrounds.ViewModels.CompanyHelpers;
using Battlegrounds.ViewModels.Modals;

using CommunityToolkit.Mvvm.Input;

using Microsoft.Extensions.Logging;

namespace Battlegrounds.ViewModels;

public sealed record CompanyEditorViewModelContext(
    Company? Company = null,
    CreateCompanyParameters? Parameters = null) {

    [MemberNotNullWhen(true, nameof(Parameters))]
    [MemberNotNullWhen(false, nameof(Company))] // How does this work? - the code literally makes no assertion about Company being null or not
    public bool IsNewCompany => Parameters is not null;
}

public sealed class CompanyEditorViewModel : INotifyPropertyChanged {

    private readonly ICompanyService _companyService;
    private readonly IBlueprintService _blueprintService;
    private readonly MainWindowViewModel _mainWindowViewModel;
    private readonly ILogger<CompanyEditorViewModel> _logger;
    private readonly string _faction = string.Empty;
    private readonly Game _game;

    private CompanyEditorViewModelContext _context;
    private bool _isDirty = false; // Indicates if the company has unsaved changes
    private string _companyName = string.Empty;
    private string _companyState = string.Empty;
    private SelectionViewModel? _selectionViewModel;

    private string _selectionTitle = "No Selection";

    private readonly List<Squad> _startingUnits = [];
    private readonly List<Squad> _skirmishPhaseUnits = [];
    private readonly List<Squad> _battlePhaseUnits = [];
    private readonly List<Squad> _reservesPhaseUnits = [];

    private ICollection<SquadBlueprint> _availableInfantryUnits = Array.Empty<SquadBlueprint>();
    private ICollection<SquadBlueprint> _availableSupportUnits = Array.Empty<SquadBlueprint>();
    private ICollection<SquadBlueprint> _availableArmourUnits = Array.Empty<SquadBlueprint>();
    private ICollection<SquadBlueprint> _availableTransportUnits = Array.Empty<SquadBlueprint>();

    public event PropertyChangedEventHandler? PropertyChanged;

    public IAsyncRelayCommand LeaveCommand { get; }

    public IAsyncRelayCommand SaveCommand { get; }

    public ICommand SetSelectedSquadCommand { get; }

    public Game Game => _game;

    public string Faction => _faction;

    public bool IsDirty {
        get => _isDirty;
        set {
            if (_isDirty == value) return;
            _isDirty = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsDirty)));
        }
    }

    public string CompanyName {
        get => _companyName;
        set {
            if (_companyName == value) return;
            _companyName = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CompanyName)));
        }
    }

    public string CompanyState {
        get => _companyState;
        set {
            if (_companyState == value) return;
            _companyState = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CompanyState)));
        }
    }

    public SelectionViewModel? SelectionViewModel {
        get => _selectionViewModel;
        set {
            if (_selectionViewModel == value) return;
            _selectionViewModel = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectionViewModel)));
        }
    }

    public ICollection<SquadBlueprint> AvailableInfantryUnits => _availableInfantryUnits;

    public ICollection<SquadBlueprint> AvailableSupportUnits => _availableSupportUnits;

    public ICollection<SquadBlueprint> AvailableArmourUnits => _availableArmourUnits;

    public ICollection<SquadBlueprint> AvailableTransportUnits => _availableTransportUnits;

    public ICollection<Squad> StartingUnits => _startingUnits.AsReadOnly();

    public ICollection<Squad> SkirmishPhaseUnits => _skirmishPhaseUnits.AsReadOnly();

    public ICollection<Squad> BattlePhaseUnits => _battlePhaseUnits.AsReadOnly();

    public ICollection<Squad> ReservesPhaseUnits => _reservesPhaseUnits.AsReadOnly();

    public int StartingUnitsCount => StartingUnits.Count;
    public int StartingUnitsMax => 4;
    public bool CanAddStartingUnit => StartingUnitsCount < StartingUnitsMax;

    public int SkirmishPhaseUnitsCount => SkirmishPhaseUnits.Count;
    public int SkirmishPhaseUnitsMax => 8;
    public bool CanAddSkirmishPhaseUnit => SkirmishPhaseUnitsCount < SkirmishPhaseUnitsMax;

    public int BattlePhaseUnitsCount => BattlePhaseUnits.Count;
    public int BattlePhaseUnitsMax => 12;
    public bool CanAddBattlePhaseUnit => BattlePhaseUnitsCount < BattlePhaseUnitsMax;

    public int ReservesPhaseUnitsCount => ReservesPhaseUnits.Count;
    public int ReservesPhaseUnitsMax => 6;
    public bool CanAddReservesPhaseUnit => ReservesPhaseUnitsCount < ReservesPhaseUnitsMax;

    public IBlueprintService BlueprintService => _blueprintService; // Expose the blueprint service for use in the view model

    public int TotalManpowerCost => (int)_startingUnits.Concat(_skirmishPhaseUnits).Concat(_battlePhaseUnits).Concat(_reservesPhaseUnits).Sum(SumManpowerCost);

    public int TotalMunitionsCost => (int)_startingUnits.Concat(_skirmishPhaseUnits).Concat(_battlePhaseUnits).Concat(_reservesPhaseUnits).Sum(SumMunitionsCost);
    public int TotalFuelCost => (int)_startingUnits.Concat(_skirmishPhaseUnits).Concat(_battlePhaseUnits).Concat(_reservesPhaseUnits).Sum(SumFuelCost);

    public string SelectionTitle {
        get => _selectionTitle;
        set {
            if (value == _selectionTitle) return;
            _selectionTitle = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectionTitle)));
        }
    }

    public CompanyEditorViewModel(
        CompanyEditorViewModelContext context, 
        ICompanyService companyService, 
        IBlueprintService blueprintService, 
        IGameService gameService, 
        MainWindowViewModel mainWindowViewModel,
        ILogger<CompanyEditorViewModel> logger) {

        ArgumentNullException.ThrowIfNull(context, nameof(context));
        _context = context;
        _logger = logger;
        _companyService = companyService;
        _blueprintService = blueprintService;
        _mainWindowViewModel = mainWindowViewModel;

        LeaveCommand = new AsyncRelayCommand(ExitEditor);
        SaveCommand = new AsyncRelayCommand(SaveCompany);
        SetSelectedSquadCommand = new RelayCommand<object>(SetSelectedSquad);

        if (_context.IsNewCompany) {
            _game = _context.Parameters.Game ?? throw new ArgumentNullException(nameof(context), "Game must be provided for a new company.");
            _faction = _context.Parameters.Faction;
            CompanyName = _context.Parameters.Name;
            CompanyState = $"Creating company {CompanyName}";
        } else {
            _game = gameService.GetGame(_context.Company.GameId) ?? throw new ArgumentNullException(nameof(context), "Game must be provided for an existing company.");
            _faction = _context.Company.Faction;
            CompanyName = _context.Company.Name;
            CompanyState = $"Loaded company {CompanyName}";
            _startingUnits.AddRange(_context.Company.Squads.Where(s => s.Phase == SquadPhase.StartingPhase));
            _skirmishPhaseUnits.AddRange(_context.Company.Squads.Where(s => s.Phase == SquadPhase.SkirmishPhase));
            _battlePhaseUnits.AddRange(_context.Company.Squads.Where(s => s.Phase == SquadPhase.BattlePhase));
            _reservesPhaseUnits.AddRange(_context.Company.Squads.Where(s => s.Phase == SquadPhase.ReservesPhase));
        }

        LoadBlueprints();

    }

    private void LoadBlueprints() {
        var squadBlueprints = (from bp in _blueprintService.GetBlueprintsForGame<SquadBlueprint>(_game.Id)
                              where bp.FactionAssociation == _faction
                              select bp).ToHashSet();
        _availableInfantryUnits = [..from bp in squadBlueprints
                                  where bp.Category is SquadCategory.Infantry && bp.Enabled is true
                                  select bp];
        _availableSupportUnits = [..from bp in squadBlueprints
                                  where bp.Category is SquadCategory.Support && bp.Enabled is true
                                  select bp];
        _availableArmourUnits = [..from bp in squadBlueprints
                                  where bp.Category is SquadCategory.Armour && bp.Enabled is true
                                  select bp];
        _availableTransportUnits = [..from bp in squadBlueprints
                                      where bp.HasExtension<HoldExtension>() && bp.Category is SquadCategory.Support
                                      select bp];
    }

    private async Task ExitEditor() {

        if (IsDirty) {
            if (await DialogModal.ShowModalAsync(DialogType.YesNo, "Unsaved Changes", "You have unsaved changes. Do you want to save your company before leaving?") == DialogResult.Yes) {
                await SaveCompany();
            }
        }

        _mainWindowViewModel.GoBack(); // Navigate back to the previous view (e.g., Company Browser)

    }

    private async Task SaveCompany() {

        if (!IsDirty) {
            return; // No changes to save
        } else {
            CompanyState = "Building company...";
        }

        try {

            DateTime createdAt;
            string companyId;
            if (_context.IsNewCompany) {
                companyId = Guid.CreateVersion7().ToString(); // Generate a new ID for the company
                createdAt = DateTime.UtcNow; // Set the creation time for a new company
            } else {
                companyId = _context.Company.Id; // Use the existing company's ID
                createdAt = _context.Company.CreatedAt; // Keep the original creation time
            }

            Company company = new Company {
                Id = companyId,
                Name = CompanyName,
                GameId = _game.Id,
                Faction = _faction,
                UpdatedAt = DateTime.UtcNow,
                CreatedAt = createdAt,
                Squads = [.. _startingUnits, .. _skirmishPhaseUnits, .. _battlePhaseUnits, .. _reservesPhaseUnits]
            };

            // Update the context with the new or modified company
            _context = new CompanyEditorViewModelContext(Company: company);

            CompanyState = "Saving company...";
            CompanyState = await _companyService.SaveCompany(company) switch {
                SaveCompanyResult.Success => "Company saved successfully.",
                SaveCompanyResult.FailedSave => "Failed to save company to disk",
                SaveCompanyResult.FailedSync => "Failed to save company to Battlegrounds server...",
                _ => "Unknown result while saving company."
            };

        } catch (Exception ex) {
            CompanyState = $"Error saving company: {ex.Message}";
        } finally {
            IsDirty = false; // Reset dirty state after saving
        }

    }

    private void SetSelectedSquad(object? any) {
        if (any is SquadBlueprint squad) {
            SelectionViewModel = new SelectionViewModel(this, squad);
            SelectionTitle = "Squad Overview";
        } else if (any is Squad existingSquad) {
            SelectionViewModel = new SelectionViewModel(this, existingSquad);
            SelectionTitle = $"Squad #{existingSquad.Id}";
        } else {
            SelectionViewModel = null; // Clear selection if not a valid squad or blueprint
            SelectionTitle = "No Selection";
        }
    }

    public void AddSquadToCompany(SquadPhase phase, SquadBlueprint blueprint) {
        Squad.TransportSquad? transport = null;
        if (blueprint.RequiresTowing) { // Set mandatory transport for squads that require towing
            if (_availableTransportUnits.FirstOrDefault() is not SquadBlueprint defaultTransport) {
                _logger.LogWarning("No available transport units for squad that requires towing. Cannot add squad to company.");
                return; // No transport available, cannot add squad
            }
            transport = new Squad.TransportSquad(defaultTransport, false);
        }
        Squad squad = new Squad() {
            Id = GetNextSquadId(),
            Phase = phase,
            Blueprint = blueprint,
            Transport = transport,
            AddedToCompanyAt = DateTime.UtcNow,
        };
        switch (phase) {
            case SquadPhase.StartingPhase:
                _startingUnits.Add(squad);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(StartingUnits)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(StartingUnitsCount)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CanAddStartingUnit)));
                break;
            case SquadPhase.SkirmishPhase:
                _skirmishPhaseUnits.Add(squad);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SkirmishPhaseUnits)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SkirmishPhaseUnitsCount)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CanAddSkirmishPhaseUnit)));
                break;
            case SquadPhase.BattlePhase:
                _battlePhaseUnits.Add(squad);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(BattlePhaseUnits)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(BattlePhaseUnitsCount)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CanAddBattlePhaseUnit)));
                break;
            case SquadPhase.ReservesPhase:
                _reservesPhaseUnits.Add(squad);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ReservesPhaseUnits)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ReservesPhaseUnitsCount)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CanAddReservesPhaseUnit)));
                break;
        }
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TotalManpowerCost)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TotalMunitionsCost)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TotalFuelCost)));
        IsDirty = true; // Mark the company as dirty after adding a squad
    }

    public void RetireSquadFromCompany(Squad squad) {
        // Remove the squad from its current phase
        if (_startingUnits.Remove(squad)) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(StartingUnits)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(StartingUnitsCount)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CanAddStartingUnit)));
        } else if (_skirmishPhaseUnits.Remove(squad)) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SkirmishPhaseUnits)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SkirmishPhaseUnitsCount)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CanAddSkirmishPhaseUnit)));
        } else if (_battlePhaseUnits.Remove(squad)) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(BattlePhaseUnits)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(BattlePhaseUnitsCount)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CanAddBattlePhaseUnit)));
        } else if (_reservesPhaseUnits.Remove(squad)) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ReservesPhaseUnits)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ReservesPhaseUnitsCount)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CanAddReservesPhaseUnit)));
        }
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TotalManpowerCost)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TotalMunitionsCost)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TotalFuelCost)));
        IsDirty = true; // Mark the company as dirty after removing a squad
        SetSelectedSquad(null); // Clear the selection after retiring a squad
    }

    public void SetSquadDeploymentMethod(Squad refSquad, SquadBlueprint? transport, bool isDropOffOnly) {

        var squad = FindSquadFromId(refSquad.Id); // The caller doesn't have latest squad data, so we find it by ID
        if (squad.Blueprint.RequiresTowing && transport is null) {
            _logger.LogInformation("Cannot remove transport from a squad that requires towing.");
            return;
        }

        Squad.TransportSquad? transportSquad = null;
        if (transport is not null) {
            transportSquad = new Squad.TransportSquad(transport, isDropOffOnly);
        }

        if (squad.HasTransport && transportSquad is not null && squad.Transport.TransportBlueprint == transportSquad.TransportBlueprint) {
            return; // No change needed, already using the same transport
        }

        SwapSquad(squad, new Squad { 
            Id = squad.Id,
            SlotItems = squad.SlotItems,
            Upgrades = squad.Upgrades,
            Blueprint = squad.Blueprint,
            Experience = squad.Experience,
            Name = squad.Name,
            Phase = squad.Phase,
            Transport = transportSquad,
            LastUpdatedAt = DateTime.UtcNow,
            AddedToCompanyAt = squad.AddedToCompanyAt,
            MatchCounts = squad.MatchCounts,
            TotalVehicleKills = squad.TotalVehicleKills,
            TotalInfantryKills = squad.TotalInfantryKills,
        });

        IsDirty = true; // Mark the company as dirty after changing deployment method

    }

    public void ApplyUpgradeToSquad(Squad squad, UpgradeBlueprint upgrade) {
        var upgrades = squad.Upgrades.ToList();
        if (squad.Upgrades.Any(x => x.Id == upgrade.Id)) { // Check if already applied, then remove it
            if (!upgrades.Remove(upgrade)) {
                return; // Upgrade not found, nothing to do
            }
        } else {
            // Otherwise, add the upgrade
            upgrades.Add(upgrade);
        }
        Squad updatedSquad = new Squad {
            Id = squad.Id,
            SlotItems = squad.SlotItems,
            Upgrades = upgrades,
            Blueprint = squad.Blueprint,
            Experience = squad.Experience,
            Name = squad.Name,
            Phase = squad.Phase,
            Transport = squad.Transport,
            LastUpdatedAt = DateTime.UtcNow,
            AddedToCompanyAt = squad.AddedToCompanyAt,
            MatchCounts = squad.MatchCounts,
            TotalVehicleKills = squad.TotalVehicleKills,
            TotalInfantryKills = squad.TotalInfantryKills
        };
        SetSelectedSquad(SwapSquad(squad, updatedSquad)); // Update the selection to the upgraded squad
        IsDirty = true; // Mark the company as dirty after applying an upgrade
    }

    private Squad FindSquadFromId(int id) {
        var allSquads = _startingUnits.Concat(_skirmishPhaseUnits).Concat(_battlePhaseUnits).Concat(_reservesPhaseUnits);
        return allSquads.FirstOrDefault(s => s.Id == id) ?? throw new KeyNotFoundException($"Squad with ID {id} not found.");
    }

    private Squad SwapSquad(Squad oldSquad, Squad newSquad) {
        switch (oldSquad.Phase) {
            case SquadPhase.StartingPhase:
                var startingIndex = _startingUnits.IndexOf(oldSquad);
                if (startingIndex >= 0) {
                    _startingUnits[startingIndex] = newSquad;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(StartingUnits)));
                } else {
                    throw new InvalidOperationException("Squad not found in Starting Phase.");
                }
                break;
            case SquadPhase.SkirmishPhase:
                var skirmishIndex = _skirmishPhaseUnits.IndexOf(oldSquad);
                if (skirmishIndex >= 0) {
                    _skirmishPhaseUnits[skirmishIndex] = newSquad;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SkirmishPhaseUnits)));
                } else {
                    throw new InvalidOperationException("Squad not found in Skirmish Phase.");
                }
                break;
            case SquadPhase.BattlePhase:
                var battleIndex = _battlePhaseUnits.IndexOf(oldSquad);
                if (battleIndex >= 0) {
                    _battlePhaseUnits[battleIndex] = newSquad;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(BattlePhaseUnits)));
                } else {
                    throw new InvalidOperationException("Squad not found in Battle Phase.");
                }
                break;
            case SquadPhase.ReservesPhase:
                var reservesIndex = _reservesPhaseUnits.IndexOf(oldSquad);
                if (reservesIndex >= 0) {
                    _reservesPhaseUnits[reservesIndex] = newSquad;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ReservesPhaseUnits)));
                } else {
                    throw new InvalidOperationException("Squad not found in Reserves Phase.");
                }
                break;
        }
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TotalManpowerCost)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TotalMunitionsCost)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TotalFuelCost)));
        return newSquad; // Return the updated squad
    }

    private int GetNextSquadId() {
        if (_startingUnits.Count == 0 && _skirmishPhaseUnits.Count == 0 && _battlePhaseUnits.Count == 0 && _reservesPhaseUnits.Count == 0) {
            return 1; // If no squads exist, start with ID 1
        }
        var all = _startingUnits.Concat(_skirmishPhaseUnits).Concat(_battlePhaseUnits).Concat(_reservesPhaseUnits);
        return all.Max(x => x.Id) + 1; // Get the next available ID for a new squad
    }

    private static float SumManpowerCost(Squad squad) => squad.Blueprint.Cost.Manpower + squad.Upgrades.Sum(static x => x.Cost.Manpower);
    private static float SumMunitionsCost(Squad squad) => squad.Blueprint.Cost.Munitions + squad.Upgrades.Sum(static x => x.Cost.Munitions);
    private static float SumFuelCost(Squad squad) => squad.Blueprint.Cost.Fuel + squad.Upgrades.Sum(static x => x.Cost.Fuel);

}
