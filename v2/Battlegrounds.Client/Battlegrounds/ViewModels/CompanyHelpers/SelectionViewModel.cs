using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

using Battlegrounds.Models.Blueprints;
using Battlegrounds.Models.Blueprints.Extensions;
using Battlegrounds.Models.Companies;

using CommunityToolkit.Mvvm.Input;

namespace Battlegrounds.ViewModels.CompanyHelpers;

public sealed record RankStar(bool IsEarned);

public sealed class SelectionViewModel : INotifyPropertyChanged {

    private readonly CompanyEditorViewModel _parentViewModel;
    private readonly SquadBlueprint _squadBlueprint;
    private readonly Squad? _squad;
    private readonly List<UpgradeViewModel> _upgrades = [];
    private readonly List<SlotItemViewModel> _items = [];

    private string _customName = string.Empty;

    private bool _hasTransport = false;
    private bool _hasDropOffTransport = false;
    private bool _hasStayTransport = false;
    private SquadBlueprint? _transportBlueprint;

    public event PropertyChangedEventHandler? PropertyChanged;

    [MemberNotNullWhen(true, nameof(_squad))]
    public bool IsSquad => _squad is not null; // Used to determine if the selection is a company squad or just a blueprint.

    public bool IsInfantry => IsSquad && _squad.Blueprint.IsInfantry; // Check if the squad is infantry.

    public SquadBlueprint Blueprint => _squadBlueprint; // The squad blueprint, which is always available.

    public string CustomName {
        get => _customName;
        set {
            if (value == _customName) return;
            _customName = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CustomName)));
        }
    }

    public bool CanGiveCustomName => IsSquad && _squad.Rank == _squad.Blueprint.Veterancy.MaxRank;

    public float Experience {
        get => _squad?.Experience ?? 0f; // Experience is only available if this is a squad, otherwise it's 0.
        set { } // NOP, experience cannot be changed in editor mode.
    }

    public int Rank => _squad?.Rank ?? 0; // Rank is only available if this is a squad, otherwise it's 0.

    public bool IsTransportable => IsSquad && (_squad.Blueprint.IsTowable || _squad.Blueprint.IsInfantry); // Check if the squad is transportable based on its blueprint.

    public bool CanDisableTranspoert => IsSquad && !_squad.Blueprint.RequiresTowing; // Check if the squad requires towing, which disallows disabling transport.

    public bool HasTransport {
        get => _hasTransport;
        set {
            if (value == _hasTransport) return;
            _hasTransport = value;
            if (!value) {
                //_transportBlueprint = null; // Clear the transport blueprint if transport is disabled.
                _parentViewModel.SetSquadDeploymentMethod(_squad!, null, _hasDropOffTransport);
            }
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(HasTransport)));
        }
    }

    public bool HasDropOffTransport {
        get => _hasDropOffTransport;
        set {
            if (value == _hasDropOffTransport) return;
            _hasDropOffTransport = value;
            if (value) {
                TransportBlueprint = _transportBlueprint ?? AvailableTransports.FirstOrDefault(); // Set the transport blueprint to the first available transport if drop-off is enabled.
                _parentViewModel.SetSquadDeploymentMethod(_squad!, _transportBlueprint, value);
            }
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(HasDropOffTransport)));
        }
    }

    public bool HasStayTransport {
        get => _hasStayTransport;
        set {
            if (value == _hasStayTransport) return;
            _hasStayTransport = value;
            if (value) {
                TransportBlueprint = _transportBlueprint ?? AvailableTransports.FirstOrDefault(); // Set the transport blueprint to the first available transport if drop-off is enabled.
                _parentViewModel.SetSquadDeploymentMethod(_squad!, _transportBlueprint, !value);
            }
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(HasStayTransport)));
        }
    }

    public SquadBlueprint? TransportBlueprint {
        get => _transportBlueprint;
        set {
            if (value == _transportBlueprint) return;
            _transportBlueprint = value;
            _parentViewModel.SetSquadDeploymentMethod(_squad!, _transportBlueprint, _hasDropOffTransport);
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TransportBlueprint)));
        }
    }

    public bool HasUpgrades => IsSquad && _upgrades.Count > 0;

    public bool CanUpgrade => IsSquad && _squad.Upgrades.Count < _squad.Blueprint.Upgrades.Slots;

    public bool HasItems => IsSquad && _items.Count > 0;

    public Squad.TransportSquad? Transport => _squad?.Transport; // Transport is only available if this is a squad, otherwise it's null.

    public IReadOnlyList<UpgradeViewModel> Upgrades => _upgrades.AsReadOnly(); // Upgrades are only available if this is a squad, otherwise it's empty.

    public IReadOnlyList<SlotItemViewModel> Items => _items.AsReadOnly(); // Upgrades are only available if this is a squad, otherwise it's empty.

    public ICollection<SquadBlueprint> AvailableTransports => _parentViewModel.AvailableTransportUnits;

    public IReadOnlyList<RankStar> RankStars {
        get {
            if (_squad is null) return [];
            var stars = new List<RankStar>();
            for (int i = 0; i < _squad.Blueprint.Veterancy.MaxRank; i++) {
                stars.Add(new RankStar(i < _squad.Rank));
            }
            return stars;
        }
    }

    public float RankProgress {
        get => _squad?.Blueprint.Veterancy.GetRankProgress(_squad.Experience) ?? 0f; // Rank progress is only available if this is a squad, otherwise it's 0f.
        set { } // NOP
    }

    public float NextRankExperience {
        get => _squad?.Blueprint.Veterancy.GetNextRankExperience(_squad.Experience) ?? 0f; // Next rank experience is only available if this is a squad, otherwise it's 0f.
        set { } // NOP
    }

    public IRelayCommand<SquadPhase>? AddToCompanyCommand { get; }

    public IRelayCommand? RetireSquadCommand { get; }

    public IRelayCommand<UpgradeBlueprint>? UpgradeCommand { get; }

    public SelectionViewModel(CompanyEditorViewModel parentViewModel, SquadBlueprint squadBlueprint) {
        _parentViewModel = parentViewModel ?? throw new ArgumentNullException(nameof(parentViewModel));
        _squadBlueprint = squadBlueprint ?? throw new ArgumentNullException(nameof(squadBlueprint));
        CustomName = string.Empty;        
        AddToCompanyCommand = new RelayCommand<SquadPhase>(x => parentViewModel.AddSquadToCompany(x, squadBlueprint));
        //SetUpgrades(); // Disabled for now (No visuals yet)
        NotifyAll();
    }

    public SelectionViewModel(CompanyEditorViewModel parentViewModel, Squad squad) {
        _parentViewModel = parentViewModel ?? throw new ArgumentNullException(nameof(parentViewModel));
        _squad = squad;
        _squadBlueprint = squad.Blueprint;
        CustomName = squad.HasCustomName ? squad.Name : string.Empty;
        RetireSquadCommand = new RelayCommand(() => parentViewModel.RetireSquadFromCompany(squad));
        UpgradeCommand = new RelayCommand<UpgradeBlueprint>(AddUpgrade);
        InitTransportOptions();
        SetUpgrades();
        SetItems();
        NotifyAll();
    }

    private void InitTransportOptions() {
        if (_squad is null)
            return;
        HasTransport = _squad.HasTransport;
        HasDropOffTransport = _squad.Transport?.DropOffOnly ?? false;
        HasStayTransport = !_squad.Transport?.DropOffOnly ?? false;
        TransportBlueprint = _squad.Transport?.TransportBlueprint;
    }

    private void SetUpgrades() {

        if (!_squadBlueprint.TryGetExtension<UpgradesExtension>(out var upgrades)) {
            return;
        }

        var bps = _parentViewModel.BlueprintService.GetBlueprintsForGame<UpgradeBlueprint>(_parentViewModel.Game.Id).ToDictionary(x => x.Id, x => x);

        List<UpgradeViewModel> availableUpgrades = 
            [..from upg in upgrades.Available 
               select bps.GetValueOrDefault(upg) into bp
               where bp is not null
               select new UpgradeViewModel(bp, IsSquad && _squad.Upgrades.Contains(bp), CanUpgrade, UpgradeCommand)];

        _upgrades.Clear();
        _upgrades.AddRange(availableUpgrades);

    }

    private void SetItems() {
        if (_squad is null) 
            return;
        if (_squad.SlotItems.Count is 0) {
            _items.Clear(); // No slot items, clear the list.
            return;
        }
        List<SlotItemViewModel> availableItems = 
            [..from item in _squad.SlotItems 
               where item.UpgradeBlueprint is not null || item.SlotItemBlueprint is not null
               select new SlotItemViewModel((Blueprint?)item.SlotItemBlueprint ?? (Blueprint?)item.UpgradeBlueprint!, item.Count)];
        _items.Clear();
        _items.AddRange(availableItems);
    }

    private void AddUpgrade(UpgradeBlueprint? upgrade) {
        if (upgrade is null) {
            return; // No upgrade
        }
        if (!IsSquad)
            return; // No squad to upgrade
        if (!CanUpgrade && !_squad.Upgrades.Contains(upgrade))
            return; // Cannot upgrade
        if (_squad is null) {
            throw new InvalidOperationException("Cannot apply upgrade to a squad that does not exist.");
        }
        _parentViewModel.ApplyUpgradeToSquad(_squad, upgrade);
    }

    private void NotifyAll() {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsSquad)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Blueprint)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Upgrades)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Items)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(RankStars)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Experience)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(RankProgress)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(NextRankExperience)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(HasUpgrades)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(HasItems)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AvailableTransports)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CanUpgrade)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsInfantry)));
    }

}
