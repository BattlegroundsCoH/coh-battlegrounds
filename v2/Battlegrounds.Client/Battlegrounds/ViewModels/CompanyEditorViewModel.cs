using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Input;

using Battlegrounds.Helpers;
using Battlegrounds.Models.Blueprints;
using Battlegrounds.Models.Blueprints.Extensions;
using Battlegrounds.Models.Companies;
using Battlegrounds.Models.Doctrines;
using Battlegrounds.Models.Playing;
using Battlegrounds.Services;
using Battlegrounds.ViewModels.CompanyHelpers;
using Battlegrounds.ViewModels.Modals;
using Battlegrounds.Views.Modals;

using CommunityToolkit.Mvvm.Input;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using static Battlegrounds.Models.Companies.Squad;

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
    private readonly IUserService _userService;
    private readonly MainWindowViewModel _mainWindowViewModel;
    private readonly ILogger<CompanyEditorViewModel> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly string _faction = string.Empty;
    private readonly Game _game;
    private DoctrineDefinition _doctrine;

    private CompanyEditorViewModelContext _context;
    private bool _isDirty = false; // Indicates if the company has unsaved changes
    private bool _isDoctrineDirty = false; // Indicates if the doctrine needs fixing
    private bool _isEditingName = false;
    private string _companyName = string.Empty;
    private string _editingCompanyName = string.Empty;
    private string _companyState = string.Empty;

    private SquadSelectionViewModel? _selectionViewModel;
    private ItemSelectionViewModel? _itemSelectionViewModel;

    private string _selectionTitle = "No Selection";

    private readonly List<Squad> _startingUnits = [];
    private readonly List<Squad> _skirmishPhaseUnits = [];
    private readonly List<Squad> _battlePhaseUnits = [];
    private readonly List<Squad> _reservesPhaseUnits = [];

    private ICollection<SquadBlueprint> _availableInfantryUnits = Array.Empty<SquadBlueprint>();
    private ICollection<SquadBlueprint> _availableSupportUnits = Array.Empty<SquadBlueprint>();
    private ICollection<SquadBlueprint> _availableArmourUnits = Array.Empty<SquadBlueprint>();
    private ICollection<SquadBlueprint> _availableTransportUnits = Array.Empty<SquadBlueprint>();
    private ICollection<SquadBlueprint> _availableTowTransportUnits = Array.Empty<SquadBlueprint>();

    private readonly List<CapturedItem> _capturedItems = [];
    private readonly HashSet<int> _assignedCapturedItemIds = [];

    public event PropertyChangedEventHandler? PropertyChanged;

    public IAsyncRelayCommand LeaveCommand { get; }

    public IAsyncRelayCommand SaveCommand { get; }

    public ICommand SetSelectedSquadCommand { get; }

    public ICommand BeginRenameCommand { get; }

    public ICommand CommitRenameCommand { get; }

    public ICommand CancelRenameCommand { get; }

    public ICommand SetSelectedCapturedItemCommand { get; }

    public ICommand AddItemToSquadCommand { get; }

    public Game Game => _game;

    public string Faction => _faction;

    public bool IsEditingName {
        get => _isEditingName;
        set {
            if (_isEditingName == value) return;
            _isEditingName = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsEditingName)));
        }
    }

    public string EditingCompanyName {
        get => _editingCompanyName;
        set {
            if (_editingCompanyName == value) return;
            _editingCompanyName = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(EditingCompanyName)));
        }
    }

    public bool IsDirty {
        get => _isDirty;
        set {
            if (_isDirty == value) return;
            _isDirty = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsDirty)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CanSave)));
        }
    }

    public bool IsDoctrineDirty {
        get => _isDoctrineDirty;
        set {
            if (_isDoctrineDirty == value) return;
            _isDoctrineDirty = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsDoctrineDirty)));
        }
    }

    public bool IsValidCompany {
        get;
        set {
            if (value == field) return;
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsValidCompany)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CanSave)));
        }
    }

    public bool CanSave => IsDirty && IsValidCompany;

    public string DoctrineName => _doctrine.Name;

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

    public bool HasSquadSelection => _selectionViewModel is not null;

    public SquadSelectionViewModel? SquadSelectionViewModel {
        get => _selectionViewModel;
        set {
            if (_selectionViewModel == value) return;
            _selectionViewModel = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SquadSelectionViewModel)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(HasSquadSelection)));
        }
    }

    public bool HasItemSelection => _itemSelectionViewModel is not null;

    public ItemSelectionViewModel? ItemSelectionViewModel {
        get => _itemSelectionViewModel;
        set {
            if (_itemSelectionViewModel == value) return;
            _itemSelectionViewModel = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ItemSelectionViewModel)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(HasItemSelection)));
        }
    }

    public int SelectedAvailableUnitTabIndex {
        get;
        set {
            if (field == value) return;
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedAvailableUnitTabIndex)));
        }
    }

    public ICollection<SquadBlueprint> AvailableInfantryUnits => _availableInfantryUnits;

    public ICollection<SquadBlueprint> AvailableSupportUnits => _availableSupportUnits;

    public ICollection<SquadBlueprint> AvailableArmourUnits => _availableArmourUnits;

    public ICollection<SquadBlueprint> AvailableTransportUnits => _availableTransportUnits;

    public ICollection<SquadBlueprint> AvailableTowTransportUnits => _availableTowTransportUnits;

    public IReadOnlyList<CapturedItem> AvailableCapturedItems =>
        [.. _capturedItems.Where(ci => ci.ItemBlueprint is not null && !_assignedCapturedItemIds.Contains(ci.Id))];

    public bool HasCapturedItems => AvailableCapturedItems.Count > 0;

    public IReadOnlyList<PhaseSquadViewModel> StartingUnits => BuildPhaseViewModels(_startingUnits);

    public IReadOnlyList<PhaseSquadViewModel> SkirmishPhaseUnits => BuildPhaseViewModels(_skirmishPhaseUnits);

    public IReadOnlyList<PhaseSquadViewModel> BattlePhaseUnits => BuildPhaseViewModels(_battlePhaseUnits);

    public IReadOnlyList<PhaseSquadViewModel> ReservesPhaseUnits => BuildPhaseViewModels(_reservesPhaseUnits);

    public int StartingUnitsCount => _startingUnits.Count;
    public int StartingUnitsMax => _doctrine.PhaseLimits.Initial;
    public bool CanAddStartingUnit => StartingUnitsCount < StartingUnitsMax;

    public int SkirmishPhaseUnitsCount => _skirmishPhaseUnits.Count;
    public int SkirmishPhaseUnitsMax => _doctrine.PhaseLimits.Skirmish;
    public bool CanAddSkirmishPhaseUnit => SkirmishPhaseUnitsCount < SkirmishPhaseUnitsMax;

    public int BattlePhaseUnitsCount => _battlePhaseUnits.Count;
    public int BattlePhaseUnitsMax => _doctrine.PhaseLimits.Battle;
    public bool CanAddBattlePhaseUnit => BattlePhaseUnitsCount < BattlePhaseUnitsMax;

    public int ReservesPhaseUnitsCount => _reservesPhaseUnits.Count;
    public int ReservesPhaseUnitsMax => _doctrine.PhaseLimits.Reserves;
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
        IServiceProvider serviceProvider,
        ICompanyService companyService,
        IDoctrineService doctrineService,
        IBlueprintService blueprintService,
        IUserService userService,
        IGameService gameService,
        MainWindowViewModel mainWindowViewModel,
        ILogger<CompanyEditorViewModel> logger) {

        ArgumentNullException.ThrowIfNull(context, nameof(context));
        _context = context;
        _logger = logger;
        _serviceProvider = serviceProvider;
        _companyService = companyService;
        _blueprintService = blueprintService;
        _userService = userService;
        _mainWindowViewModel = mainWindowViewModel;

        LeaveCommand = new AsyncRelayCommand(ExitEditor);
        SaveCommand = new AsyncRelayCommand(SaveCompany);
        SetSelectedSquadCommand = new RelayCommand<object>(SetSelectedSquad);
        BeginRenameCommand = new RelayCommand(BeginRename);
        CommitRenameCommand = new RelayCommand(CommitRename);
        CancelRenameCommand = new RelayCommand(CancelRename);
        SetSelectedCapturedItemCommand = new RelayCommand<CapturedItem>(SetSelectedCapturedItem);
        AddItemToSquadCommand = new RelayCommand<ItemSelectionViewModel.SquadAssignable>(AddItemToSquad);

        if (_context.IsNewCompany) {
            _game = _context.Parameters.Game ?? throw new ArgumentNullException(nameof(context), "Game must be provided for a new company.");
            _faction = _context.Parameters.Faction;
            _doctrine = _context.Parameters.Doctrine ?? throw new ArgumentNullException(nameof(context), "Doctrine must be provided for a new company.");
            CompanyName = _context.Parameters.Name;
            CompanyState = $"Creating company {CompanyName}";
        } else {
            _game = gameService.GetGame(_context.Company.GameId) ?? throw new ArgumentNullException(nameof(context), "Game must be provided for an existing company.");
            _faction = _context.Company.Faction;

            if (doctrineService.TryGetDoctrineById(_context.Company.DoctrineId, out var doctrine)) {
                if (doctrine.Version != _context.Company.DoctrineVersion) {
                    _logger.LogWarning("Doctrine {DoctrineId} version mismatch for company {CompanyName}. Expected version {ExpectedVersion}, but found version {ActualVersion}.", doctrine.Id, CompanyName, _context.Company.DoctrineVersion, doctrine.Version);
                    IsDoctrineDirty = true;
                }
                _doctrine = doctrine;
            } else {
                _doctrine = doctrineService.GetBaseDoctrine(_game.Id, _faction);
                IsDoctrineDirty = true;
            }

            CompanyName = _context.Company.Name;
            CompanyState = $"Loaded company {CompanyName}";
            _startingUnits.AddRange(_context.Company.Squads.Where(s => s.Phase == SquadPhase.StartingPhase));
            _skirmishPhaseUnits.AddRange(_context.Company.Squads.Where(s => s.Phase == SquadPhase.SkirmishPhase));
            _battlePhaseUnits.AddRange(_context.Company.Squads.Where(s => s.Phase == SquadPhase.BattlePhase));
            _reservesPhaseUnits.AddRange(_context.Company.Squads.Where(s => s.Phase == SquadPhase.ReservesPhase));
            _capturedItems.AddRange(_context.Company.CapturedItems);
            foreach (var itemId in _context.Company.Squads.SelectMany(GetSquadCaptureItems)) {
                _assignedCapturedItemIds.Add(itemId);
            }
        }

        LoadBlueprints();
        FixDoctrine();
        VerifyCompany();

    }

    private IEnumerable<int> GetSquadCaptureItems(Squad squad) {
        IEnumerable<int> seed = squad.IsCapturedWeapon ?[ squad.CapturedWeapon!.CompanyItemId] : [];
        return seed.Union(squad.SlotItems.Select(x => x.CompanyItemId));
    }

    private void BeginRename() {
        EditingCompanyName = CompanyName;
        IsEditingName = true;
    }

    private void CommitRename() {
        if (!IsEditingName) return;
        IsEditingName = false;
        if (!string.IsNullOrWhiteSpace(EditingCompanyName) && EditingCompanyName != CompanyName) {
            CompanyName = EditingCompanyName;
            IsDirty = true;
        }
    }

    private void CancelRename() {
        IsEditingName = false;
    }

    private void LoadBlueprints() {
        var squadBlueprints = _doctrine.Blueprints.Squads.Select(x => x.Blueprint).ToHashSet();
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
                                      where bp.HasExtension<HoldExtension>(ext => !ext.EnablePassengers) && bp.Category is SquadCategory.Support
                                      select bp];
        _availableTowTransportUnits = [..from bp in squadBlueprints
                                         where bp.HasExtension<HoldExtension>(ext => ext.CanTow) && bp.Category is SquadCategory.Support
                                         select bp];
    }

    private async void FixDoctrine() {

        if (!IsDoctrineDirty)
            return;

        _logger.LogWarning("Doctrine {DoctrineId} is outdated or missing for company {CompanyName}. Prompting the user to select a replacement.", _doctrine.Id, CompanyName);

        FixDoctrineModalView modal = _serviceProvider.GetRequiredService<FixDoctrineModalView>();
        if (modal.DataContext is not FixDoctrineModalViewModel viewModel) {
            _logger.LogError("FixDoctrineModalView does not have a FixDoctrineModalViewModel as its DataContext.");
            return;
        }

        viewModel.SetContext(_game, _faction, CompanyName);

        var result = await _serviceProvider.GetRequiredService<IDialogService>().ShowDialogAsync<FixDoctrineParameters>(modal);

        if (result is { Confirmed: true, Doctrine: not null }) {
            _doctrine = result.Doctrine;
            IsDoctrineDirty = false;
            IsDirty = true;
            LoadBlueprints();
            // Notify all doctrine-dependent properties
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DoctrineName)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(StartingUnitsMax)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CanAddStartingUnit)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SkirmishPhaseUnitsMax)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CanAddSkirmishPhaseUnit)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(BattlePhaseUnitsMax)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CanAddBattlePhaseUnit)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ReservesPhaseUnitsMax)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CanAddReservesPhaseUnit)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AvailableInfantryUnits)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AvailableSupportUnits)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AvailableArmourUnits)));
            VerifyCompany();
        } else {
            // User cancelled — editor remains blocked
            IsValidCompany = false;
            CompanyState = "Doctrine fix required. The company cannot be saved until a valid doctrine is selected.";
        }

    }

    private async void VerifyCompany() {

        if (IsDoctrineDirty) {
            IsValidCompany = false; // If the doctrine is dirty, we cannot verify the company, so we mark it as invalid.
            return;
        }
        
        bool isValid = true;
        if (_startingUnits.Count > _doctrine.PhaseLimits.Initial) {
            _logger.LogWarning("Company {CompanyName} has more starting units than allowed by doctrine {DoctrineId}.", CompanyName, _doctrine.Id);
            CompanyState = $"Company has more starting units than allowed. You need to remove {_startingUnits.Count - _doctrine.PhaseLimits.Initial} unit(s).";
            isValid = false;
        }

        if (_skirmishPhaseUnits.Count > _doctrine.PhaseLimits.Skirmish) {
            _logger.LogWarning("Company {CompanyName} has more skirmish phase units than allowed by doctrine {DoctrineId}.", CompanyName, _doctrine.Id);
            CompanyState = $"Company has more skirmish phase units than allowed. You need to remove {_skirmishPhaseUnits.Count - _doctrine.PhaseLimits.Skirmish} unit(s).";
            isValid = false;
        }

        if (_battlePhaseUnits.Count > _doctrine.PhaseLimits.Battle) {
            _logger.LogWarning("Company {CompanyName} has more battle phase units than allowed by doctrine {DoctrineId}.", CompanyName, _doctrine.Id);
            CompanyState = $"Company has more battle phase units than allowed. You need to remove {_battlePhaseUnits.Count - _doctrine.PhaseLimits.Battle} unit(s).";
            isValid = false;
        }
        
        if (_reservesPhaseUnits.Count > _doctrine.PhaseLimits.Reserves) {
            _logger.LogWarning("Company {CompanyName} has more reserves phase units than allowed by doctrine {DoctrineId}.", CompanyName, _doctrine.Id);
            CompanyState = $"Company has more reserves phase units than allowed. You need to remove {_reservesPhaseUnits.Count - _doctrine.PhaseLimits.Reserves} unit(s).";
            isValid = false;
        }

        // Grab all units
        var allUnits = _startingUnits.Concat(_skirmishPhaseUnits).Concat(_battlePhaseUnits).Concat(_reservesPhaseUnits);
        var typeCounts = allUnits.SelectMany(x => x.Blueprint.TryGetExtension<TypesExtension>(out TypesExtension? ext) ? ext.Values : Enumerable.Empty<string>())
                                 .GroupBy(x => x)
                                 .ToDictionary(g => g.Key, g => g.Count());

        // Check if any unit type exceeds the allowed limit
        foreach (var (ty, max) in _doctrine.TypeLimits) {
            if (typeCounts.TryGetValue(ty, out int count) && count > max) {
                _logger.LogWarning("Company {CompanyName} has more units of type {Type} than allowed by doctrine {DoctrineId}.", CompanyName, ty, _doctrine.Id);
                CompanyState = $"Company has more units of type {ty} than allowed. You need to remove {count - max} unit(s).";
                isValid = false;
            }
        }

        // Mark the company as valid or invalid based on the checks performed
        IsValidCompany = isValid;

        // Clear state message if the company is valid
        if (IsValidCompany) {
            CompanyState = IsDirty ? "Company is valid and ready to save." : string.Empty;
        }

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

            string user = (await _userService.GetLocalUserAsync())?.UserId ?? "Unknown";

            DateTime createdAt;
            string companyId, createdBy;
            uint version;
            if (_context.IsNewCompany) {
                companyId = Guid.CreateVersion7().ToString(); // Generate a new ID for the company
                createdAt = DateTime.UtcNow; // Set the creation time for a new company
                createdBy = user; // Set the creator for a new company
                version = 1; // Start version at 1 for a new company
            } else {
                companyId = _context.Company.Id; // Use the existing company's ID
                createdAt = _context.Company.CreatedAt; // Keep the original creation time
                createdBy = _context.Company.CreatedBy; // Keep the original creator
                version = _context.Company.Version + 1; // Increment version for an existing company
            }

            Company company = new Company {
                Id = companyId,
                Name = CompanyName,
                GameId = _game.Id,
                Faction = _faction,
                UpdatedAt = DateTime.UtcNow,
                CreatedAt = createdAt,
                CreatedBy = createdBy,
                UpdatedBy = user,
                Version = version,
                DoctrineId = _doctrine.Id,
                DoctrineVersion = _doctrine.Version,
                CapturedItems = [.. _capturedItems],
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

    private void AddItemToSquad(ItemSelectionViewModel.SquadAssignable? itemAssignment) {
        ArgumentNullException.ThrowIfNull(itemAssignment, nameof(itemAssignment));

        var item = itemAssignment.ViewModel.Item;
        var squad = itemAssignment.Squad;

        var updatedSquad = SwapSquad(squad, new Squad {
            Id = squad.Id,
            SlotItems = [.. squad.SlotItems, new SlotItem(item.Id, item.ItemBlueprint, null)],
            Upgrades = squad.Upgrades,
            Blueprint = squad.Blueprint,
            Experience = squad.Experience,
            Name = squad.Name,
            Phase = squad.Phase,
            Transport = squad.Transport,
            LastUpdatedAt = DateTime.UtcNow,
            AddedToCompanyAt = squad.AddedToCompanyAt,
            MatchCounts = squad.MatchCounts,
            TotalVehicleKills = squad.TotalVehicleKills,
            TotalInfantryKills = squad.TotalInfantryKills,
            Passenger = squad.Passenger,
            CapturedWeapon = squad.CapturedWeapon
        });

        SetSelectedSquad(updatedSquad); // Update the selection to the squad with the new item

        // Refresh capture items
        _assignedCapturedItemIds.Add(item.Id);

        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AvailableCapturedItems)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(HasCapturedItems)));

        if (!HasCapturedItems) {
            SelectedAvailableUnitTabIndex = 0; // Reset to the first tab if no captured items are left
        }

        IsDirty = true;

    }

    private void SetSelectedCapturedItem(CapturedItem? item) {
        SquadSelectionViewModel = null;
        if (item is CapturedItem { IsTeamWeapon: true }) {
            ItemSelectionViewModel = new ItemSelectionViewModel(this, item);
            SelectionTitle = "Captured Team Weapon";
        } else if (item is CapturedItem { IsTeamWeapon: false}) {
            ItemSelectionViewModel = new ItemSelectionViewModel(this, item);
            SelectionTitle = "Captured Weapon";
        } else {
            ItemSelectionViewModel = null; // Clear selection if not a valid captured item
            SelectionTitle = "No Selection";
        }
    }

    private void SetSelectedSquad(object? any) {
        ItemSelectionViewModel = null; // Clear any existing item selection
        if (any is SquadBlueprint squad) {
            SquadSelectionViewModel = new SquadSelectionViewModel(this, squad);
            SelectionTitle = "Squad Overview";
        } else if (any is PhaseSquadViewModel pvm) {
            SquadSelectionViewModel = new SquadSelectionViewModel(this, pvm.Squad);
            SelectionTitle = $"Squad #{pvm.Id}";
        } else if (any is Squad existingSquad) {
            SquadSelectionViewModel = new SquadSelectionViewModel(this, existingSquad);
            SelectionTitle = $"Squad #{existingSquad.Id}";
        } else {
            SquadSelectionViewModel = null; // Clear selection if not a valid squad or blueprint
            SelectionTitle = "No Selection";
        }
    }

    public void AddCapturedSquadToCompany(SquadPhase phase, CapturedItem capturedItem) {

        if (capturedItem.ItemBlueprint is not EntityBlueprint { TeamWeapon: TeamWeaponExtension teamWeapon }) {
            throw new ArgumentException("Captured item is not a team weapon and cannot be added as a squad.", nameof(capturedItem));
        }

        var squadBlueprint = (SquadBlueprint?)teamWeapon.RecrewSquadBlueprint ?? throw new InvalidOperationException("Captured item does not have a valid squad blueprint for recruewing.");
        var crewBlueprint = _blueprintService.GetBlueprint<CoH3, SquadBlueprint>(_game.GetFactionCrewSquadBlueprint(Faction));

        var transport = GetDefaultTransportSquad(squadBlueprint);

        Squad squad = new Squad {
            Id = GetNextSquadId(),
            Phase = phase,
            Blueprint = squadBlueprint,
            Transport = transport,
            AddedToCompanyAt = DateTime.UtcNow,
            CapturedWeapon = new Squad.CaptureInfo(capturedItem.Id, capturedItem.ItemBlueprint, crewBlueprint)
        };

        // Add to assigned captured items to prevent re-adding the same captured item
        _assignedCapturedItemIds.Add(capturedItem.Id);

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
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AvailableCapturedItems)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(HasCapturedItems)));
        IsDirty = true; // Mark the company as dirty after adding a squad

        SetSelectedCapturedItem(null);

    }

    public void AddSquadToCompany(SquadPhase phase, SquadBlueprint blueprint) {
        var transport = GetDefaultTransportSquad(blueprint);
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
        VerifyCompany();
    }

    private TransportSquad? GetDefaultTransportSquad(SquadBlueprint blueprint) {
        if (blueprint.RequiresTowing) { // Set mandatory transport for squads that require towing
            if (_availableTransportUnits.FirstOrDefault() is not SquadBlueprint defaultTransport) {
                _logger.LogWarning("No available transport units for squad that requires towing. Cannot add squad to company.");
                return null; // No transport available, cannot add squad
            }
            return new TransportSquad(defaultTransport, false);
        }
        return null;
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
        VerifyCompany(); // Re-verify the company after removing a squad
    }

    public void SetSquadDeploymentMethod(Squad refSquad, SquadBlueprint? transport) {

        var squad = FindSquadFromId(refSquad.Id); // The caller doesn't have latest squad data, so we find it by ID
        if (squad.Blueprint.RequiresTowing && transport is null) {
            _logger.LogInformation("Cannot remove transport from a squad that requires towing.");
            return;
        }

        Squad.TransportSquad? transportSquad = null;
        if (transport is not null) {
            transportSquad = new Squad.TransportSquad(transport, true);
        }

        if (squad.HasTransport && transportSquad is not null && squad.Transport.TransportBlueprint == transportSquad.TransportBlueprint) {
            return; // No change needed, already using the same transport
        }

        Squad updatedSquad = new Squad { 
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
            Passenger = squad.Passenger,
            CapturedWeapon = squad.CapturedWeapon,
        };

        SetSelectedSquad(SwapSquad(squad, updatedSquad)); // Update the selection to reflect the new deployment method

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
            TotalInfantryKills = squad.TotalInfantryKills,
            Passenger = squad.Passenger,
            CapturedWeapon = squad.CapturedWeapon,
        };
        SetSelectedSquad(SwapSquad(squad, updatedSquad)); // Update the selection to the upgraded squad
        IsDirty = true; // Mark the company as dirty after applying an upgrade
    }

    public void RemoveItemFromSquad(Squad squad, SlotItem item) {
        Squad updatedSquad = new Squad {
            Id = squad.Id,
            SlotItems = [.. squad.SlotItems.Except([item])],
            Upgrades = squad.Upgrades,
            Blueprint = squad.Blueprint,
            Experience = squad.Experience,
            Name = squad.Name,
            Phase = squad.Phase,
            Transport = squad.Transport,
            LastUpdatedAt = DateTime.UtcNow,
            AddedToCompanyAt = squad.AddedToCompanyAt,
            MatchCounts = squad.MatchCounts,
            TotalVehicleKills = squad.TotalVehicleKills,
            TotalInfantryKills = squad.TotalInfantryKills,
            Passenger = squad.Passenger,
            CapturedWeapon = squad.CapturedWeapon,
        };
        _assignedCapturedItemIds.Remove(item.CompanyItemId); // Remove the item from assigned captured items
        SetSelectedSquad(SwapSquad(squad, updatedSquad)); // Update the selection to new squad
        IsDirty = true; // Mark the company as dirty after removing an item

        // Refresh capture items (since we removed an item, it may now be available again)
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AvailableCapturedItems)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(HasCapturedItems)));

    }

    public IReadOnlyList<Squad> GetPhaseSquads(SquadPhase phase) => phase switch {
        SquadPhase.StartingPhase => _startingUnits.AsReadOnly(),
        SquadPhase.SkirmishPhase => _skirmishPhaseUnits.AsReadOnly(),
        SquadPhase.BattlePhase => _battlePhaseUnits.AsReadOnly(),
        SquadPhase.ReservesPhase => _reservesPhaseUnits.AsReadOnly(),
        _ => Array.Empty<Squad>()
    };

    public IReadOnlySet<int> GetPassengerIds(int excludeSquadId = -1) {
        return _startingUnits.Concat(_skirmishPhaseUnits).Concat(_battlePhaseUnits).Concat(_reservesPhaseUnits)
            .Where(s => s.HasPassenger && s.Id != excludeSquadId)
            .Select(s => s.Passenger!.PassengerSquadId)
            .ToHashSet();
    }

    public void SetSquadPassenger(Squad squad, Squad? passenger) {
        if (squad.HasPassenger && passenger is not null && squad.Passenger.PassengerSquadId == passenger.Id)
            return;
        if (!squad.HasPassenger && passenger is null)
            return;
        Squad updatedSquad = new Squad {
            Id = squad.Id,
            Name = squad.Name,
            Phase = squad.Phase,
            Blueprint = squad.Blueprint,
            Experience = squad.Experience,
            Upgrades = squad.Upgrades,
            SlotItems = [.. squad.SlotItems],
            Transport = squad.Transport,
            Passenger = passenger is not null ? new Squad.PassengerSquad(passenger.Id) : null,
            AddedToCompanyAt = squad.AddedToCompanyAt,
            LastUpdatedAt = DateTime.UtcNow,
            MatchCounts = squad.MatchCounts,
            TotalInfantryKills = squad.TotalInfantryKills,
            TotalVehicleKills = squad.TotalVehicleKills,
            CapturedWeapon = squad.CapturedWeapon,
        };
        SetSelectedSquad(SwapSquad(squad, updatedSquad));
        IsDirty = true;
    }

    private IReadOnlyList<PhaseSquadViewModel> BuildPhaseViewModels(List<Squad> squads) {
        var allSquadsById = _startingUnits.Concat(_skirmishPhaseUnits).Concat(_battlePhaseUnits).Concat(_reservesPhaseUnits)
            .ToDictionary(s => s.Id);
        return [.. squads.Select(s => new PhaseSquadViewModel(s,
            s.HasPassenger ? allSquadsById.GetValueOrDefault(s.Passenger.PassengerSquadId) : null))];
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

    public void DestroyItem(CapturedItem capturedItem) {
        _capturedItems.Remove(capturedItem);
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AvailableCapturedItems)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(HasCapturedItems)));
        if (!HasCapturedItems) {
            SelectedAvailableUnitTabIndex = 0; // Reset to the first tab if no captured items are left
        }
        SetSelectedCapturedItem(null); // Clear the selection if the destroyed item was selected
    }

}
