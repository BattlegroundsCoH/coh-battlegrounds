using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Input;

using Battlegrounds.Models.Blueprints;
using Battlegrounds.Models.Companies;
using Battlegrounds.Models.Playing;
using Battlegrounds.Services;
using Battlegrounds.ViewModels.CompanyHelpers;
using Battlegrounds.ViewModels.Modals;

using CommunityToolkit.Mvvm.Input;

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
    private readonly string _faction = string.Empty;
    private readonly Game _game;

    private CompanyEditorViewModelContext _context;
    private bool _isDirty = false; // Indicates if the company has unsaved changes
    private string _companyName = string.Empty;
    private string _companyState = string.Empty;
    private SelectionViewModel? _selectionViewModel;

    private readonly List<Squad> _startingUnits = [];
    private readonly List<Squad> _skirmishPhaseUnits = [];
    private readonly List<Squad> _battlePhaseUnits = [];
    private readonly List<Squad> _reservesPhaseUnits = [];

    private ICollection<SquadBlueprint> _availableInfantryUnits = Array.Empty<SquadBlueprint>();
    private ICollection<SquadBlueprint> _availableSupportUnits = Array.Empty<SquadBlueprint>();
    private ICollection<SquadBlueprint> _availableArmourUnits = Array.Empty<SquadBlueprint>();

    public event PropertyChangedEventHandler? PropertyChanged;

    public ICommand LeaveCommand { get; }

    public IAsyncRelayCommand SaveCommand { get; }

    public ICommand SetSelectedSquadCommand { get; }

    public Game Game => _game;

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

    public ICollection<Squad> StartingUnits => _startingUnits.AsReadOnly();

    public ICollection<Squad> SkirmishPhaseUnits => _skirmishPhaseUnits.AsReadOnly();

    public ICollection<Squad> BattlePhaseUnits => _battlePhaseUnits.AsReadOnly();

    public ICollection<Squad> ReservesPhaseUnits => _reservesPhaseUnits.AsReadOnly();

    public CompanyEditorViewModel(CompanyEditorViewModelContext context, ICompanyService companyService, IBlueprintService blueprintService, IGameService gameService) {
        ArgumentNullException.ThrowIfNull(context, nameof(context));
        _context = context;
        _companyService = companyService;
        _blueprintService = blueprintService;

        LeaveCommand = new RelayCommand(ExitEditor);
        SaveCommand = new AsyncRelayCommand(SaveCompany);
        SetSelectedSquadCommand = new RelayCommand<object>(SetSelectedSquad);

        if (_context.IsNewCompany) {
            _game = _context.Parameters.Game ?? throw new ArgumentNullException(nameof(context), "Game must be provided for a new company.");
            _faction = _context.Parameters.Faction;
            CompanyName = _context.Parameters.Name;
            CompanyState = "Creating new company...";
        } else {
            _game = gameService.GetGame(_context.Company.GameId) ?? throw new ArgumentNullException(nameof(context), "Game must be provided for an existing company.");
            _faction = _context.Company.Faction;
            CompanyName = _context.Company.Name;
            CompanyState = "Loaded existing company...";
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
                                  where bp.Cateogry == SquadCategory.Infantry
                                  select bp];
        _availableSupportUnits = [..from bp in squadBlueprints
                                  where bp.Cateogry == SquadCategory.Support 
                                  select bp];
        _availableArmourUnits = [..from bp in squadBlueprints
                                  where bp.Cateogry == SquadCategory.Armour
                                  select bp];
    }

    private void ExitEditor() {
        // Logic to exit the editor, e.g., close the view or navigate away
        // This could involve raising an event or calling a service method
    }

    private async Task SaveCompany() {

        if (!IsDirty) {
            return; // No changes to save
        } else {
            CompanyState = "Saving company...";
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

            if (await _companyService.SaveCompany(company)) {
                CompanyState = "Company saved successfully.";
            } else {
                CompanyState = "Failed to save company.";
            }

        } catch (Exception ex) {
            CompanyState = $"Error saving company: {ex.Message}";
        } finally {
            IsDirty = false; // Reset dirty state after saving
        }

    }

    private void SetSelectedSquad(object? any) {
        if (any is SquadBlueprint squad) {
            SelectionViewModel = new SelectionViewModel(squad, AddSquadToCompany);
        } else if (any is Squad existingSquad) {
            SelectionViewModel = new SelectionViewModel(existingSquad, RetireSquadFromCompany);
        } else {
            SelectionViewModel = null; // Clear selection if not a valid squad or blueprint
        }
    }

    private void AddSquadToCompany(SquadPhase phase, SquadBlueprint blueprint) {
        Squad squad = new Squad() {
            Id = GetNextSquadId(),
            Phase = phase,
            Blueprint = blueprint,
        };
        switch (phase) {
            case SquadPhase.StartingPhase:
                _startingUnits.Add(squad);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(StartingUnits)));
                break;
            case SquadPhase.SkirmishPhase:
                _skirmishPhaseUnits.Add(squad);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SkirmishPhaseUnits)));
                break;
            case SquadPhase.BattlePhase:
                _battlePhaseUnits.Add(squad);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(BattlePhaseUnits)));
                break;
            case SquadPhase.ReservesPhase:
                _reservesPhaseUnits.Add(squad);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ReservesPhaseUnits)));
                break;
        }
        IsDirty = true; // Mark the company as dirty after adding a squad
    }

    private void RetireSquadFromCompany(Squad squad) {
        // Remove the squad from its current phase
        if (_startingUnits.Remove(squad)) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(StartingUnits)));
        } else if (_skirmishPhaseUnits.Remove(squad)) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SkirmishPhaseUnits)));
        } else if (_battlePhaseUnits.Remove(squad)) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(BattlePhaseUnits)));
        } else if (_reservesPhaseUnits.Remove(squad)) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ReservesPhaseUnits)));
        }
        IsDirty = true; // Mark the company as dirty after removing a squad
        SetSelectedSquad(null); // Clear the selection after retiring a squad
    }

    private int GetNextSquadId() {
        if (_startingUnits.Count == 0 && _skirmishPhaseUnits.Count == 0 && _battlePhaseUnits.Count == 0 && _reservesPhaseUnits.Count == 0) {
            return 1; // If no squads exist, start with ID 1
        }
        var all = _startingUnits.Concat(_skirmishPhaseUnits).Concat(_battlePhaseUnits).Concat(_reservesPhaseUnits);
        return all.Max(x => x.Id) + 1; // Get the next available ID for a new squad
    }

}
