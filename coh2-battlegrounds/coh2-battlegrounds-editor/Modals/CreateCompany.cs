using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Input;
using System.Windows.Media;
using System.Linq;

using Battlegrounds.Game.Gameplay;
using Battlegrounds.Modding.Content.Companies;
using Battlegrounds.Modding;
using Battlegrounds.Resources;
using Battlegrounds.UI.Converters.Icons;
using Battlegrounds.UI.Modals;
using Battlegrounds.UI;
using Battlegrounds.Functional;
using Battlegrounds.Game.Blueprints;
using Battlegrounds.Game;
using Battlegrounds.Modding.Vanilla;
using Battlegrounds.Resources.Extensions;

namespace Battlegrounds.Editor.Modals;

/// <summary>
/// 
/// </summary>
/// <param name="sender"></param>
/// <param name="result"></param>
public delegate void CreateCompanyCallback(CreateCompany sender, ModalDialogResult result);

/// <summary>
/// 
/// </summary>
public sealed class CreateCompany : INotifyPropertyChanged {

    public record FactionType(Faction Self) {
        public ImageSource Icon => StringToFactionIcon.GetIcon(this.Self);
        public override string ToString() => BattlegroundsContext.DataSource.GetLocale(Self.RequiredDLC.Game)?.GetString(this.Self.NameKey) ?? "Unknown Army";
    }

    public record GamePick(GameCase Game, IList<FactionType> Factions) {
        public override string ToString() => Game switch {
            GameCase.CompanyOfHeroes2 => "Company of Heroes 2",
            GameCase.CompanyOfHeroes3 => "Company of Heroes 3",
            _ => throw new InvalidEnumArgumentException(nameof(Game)),
        };
    }

    public record PackagePick(IModPackage Package) {
        public override string ToString() => Package.PackageName;
    }

    public record CompanyType(string Name, FactionCompanyType? Type) {
        public static readonly CompanyType None = new("CreateCompanyDialogView_Type_None", null);
        private SquadBlueprint[]? m_squads = new SquadBlueprint[3] { SquadBlueprint.Invalid, SquadBlueprint.Invalid, SquadBlueprint.Invalid };
        private AbilityBlueprint[]? m_abilities = new AbilityBlueprint[3] { AbilityBlueprint.Invalid, AbilityBlueprint.Invalid, AbilityBlueprint.Invalid };
        public string Desc => BattlegroundsContext.Localize.GetString($"{this.Name}_desc");
        public ImageSource Icon => StringToCompanyTypeIcon.GetFromType(this.Type);
        public ImageSource? Unit01 => ResourceHandler.GetIcon(this.m_squads![0].GetUnitIcon());
        public ImageSource? Unit02 => ResourceHandler.GetIcon(this.m_squads![1].GetUnitIcon());
        public ImageSource? Unit03 => ResourceHandler.GetIcon(this.m_squads![2].GetUnitIcon());
        public ImageSource? Ability01 => ResourceHandler.GetIcon(this.m_abilities![0].GetAbilityIcon());
        public ImageSource? Ability02 => ResourceHandler.GetIcon(this.m_abilities![1].GetAbilityIcon());
        public ImageSource? Ability03 => ResourceHandler.GetIcon(this.m_abilities![2].GetAbilityIcon());
        public override string ToString() => BattlegroundsContext.Localize.GetString(this.Name);
        public CompanyType CacheDisplay() {

            // Init bases
            this.m_squads = new SquadBlueprint[3] { SquadBlueprint.Invalid, SquadBlueprint.Invalid, SquadBlueprint.Invalid };
            this.m_abilities = new AbilityBlueprint[3] { AbilityBlueprint.Invalid, AbilityBlueprint.Invalid, AbilityBlueprint.Invalid };

            // Check if there's data to draw values from
            if (this.Type is not null) {

                // Get datasource
                var ds = BattlegroundsContext.DataSource.GetBlueprints(this.Type.FactionData!.Package!, this.Type.FactionData.Game)!;

                // Set first three squads if possible
                this.m_squads =
                    this.Type.UIData.HighlightUnits.Length >= 3 ? this.Type.UIData.HighlightUnits[..3].Map(ds.FromBlueprintName<SquadBlueprint>) : this.m_squads;

                // Set first three abilities if possible
                this.m_abilities =
                    this.Type.UIData.HighlightAbilities.Length >= 3 ? this.Type.UIData.HighlightAbilities[..3].Map(ds.FromBlueprintName<AbilityBlueprint>) : this.m_abilities;
            
            }

            // Return self
            return this;

        }

    }

    private string m_companyName = string.Empty;

    public string SelectedName {
        get => this.m_companyName;
        set {
            this.m_companyName = value;
            this.OnPropertyChanged(nameof(CanCreate));
            this.OnPropertyChanged(nameof(SelectedName));
        }
    }

    private FactionType m_companyFaction = new(Faction.Soviet);

    public FactionType SelectedFaction {
        get => this.m_companyFaction;
        set {
            this.m_companyFaction = value;
            this.OnPropertyChanged(nameof(CanCreate));
            this.FactionChanged();
        }
    }

    private CompanyType m_companyType = CompanyType.None;

    public CompanyType SelectedType {
        get => this.m_companyType;
        set {
            this.m_companyType = value;
            this.OnPropertyChanged(nameof(CanCreate));
            this.OnPropertyChanged(nameof(SelectedType));
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public ObservableCollection<FactionType> AvailableFactions { get; }

    public ObservableCollection<CompanyType> AvailableTypes { get; }

    public ICommand CreateCommand { get; }

    public ICommand CancelCommand { get; }

    public IList<GamePick> Games { get; }

    public ObservableCollection<PackagePick> Packages { get; }

    public bool CanCreate => this.SelectedName.Length > 0 && this.SelectedType != CompanyType.None;

    /// <summary>
    /// 
    /// </summary>
    public IModPackage Package => Packages[SelectedPackageIndex].Package;

    private IModPackage? PackageOrNull => SelectedPackageIndex >= 0 ? Packages[SelectedPackageIndex].Package : null;

    /// <summary>
    /// 
    /// </summary>
    public GameCase Game => Games[SelectedGameIndex].Game;

    private int _packageIdx = 0;

    public int SelectedPackageIndex {
        get => _packageIdx;
        set {
            _packageIdx = value;
            RefreshCompanyTypes();
        }
    }

    private int _gameIdx;

    public int SelectedGameIndex {
        get => _gameIdx;
        set {
            _gameIdx = value;
            GameChanged();
        }
    }

    private CreateCompany(CreateCompanyCallback resultCallback) {

        // Create games pick
        this.Games = new GamePick[] {
            new GamePick(GameCase.CompanyOfHeroes2, new FactionType[] { 
                new(Faction.Soviet), 
                new(Faction.Wehrmacht) 
            }),
            new GamePick(GameCase.CompanyOfHeroes3, new FactionType[] { 
                new(Faction.AfrikaKorps),
                new(Faction.Americans),
                new(Faction.BritishAfrica),
                new(Faction.Germans) 
            })
        };

        // Create package picks
        this.Packages = new();
        RefreshPackages();

        // Create available factions
        this.AvailableFactions = new ObservableCollection<FactionType>(this.Games[0].Factions);

        // Create available types
        this.AvailableTypes = new();
        this.FactionChanged();

        // Set commands
        this.CreateCommand = new RelayCommand(() => resultCallback?.Invoke(this, ModalDialogResult.Confirm));
        this.CancelCommand = new RelayCommand(() => resultCallback?.Invoke(this, ModalDialogResult.Cancel));

    }

    private void RefreshPackages() {
        Packages.Clear();
        BattlegroundsContext.ModManager.GetPackages()
            .Where(x => x is not VanillaModPackage && x.SupportedGames.HasFlag(Game))
            .Select(x => new PackagePick(x))
            .ForEach(Packages.Add);
    }

    public void OnPropertyChanged(string propertyName)
            => this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    private void FactionChanged() {

        // Update faction
        OnPropertyChanged(nameof(SelectedFaction));

        // Refresh company types
        RefreshCompanyTypes();

    }

    private void RefreshCompanyTypes() {

        // Clear types
        this.AvailableTypes.Clear();

        if (this.m_companyFaction is null || this.PackageOrNull is null) {
            this.AvailableTypes.Add(CompanyType.None);
            this.SelectedType = CompanyType.None;
            return; 
        }

        // Define base types
        if (!this.PackageOrNull.FactionSettings.TryGetValue(this.m_companyFaction.Self, out var factionData)) {
            this.AvailableTypes.Add(CompanyType.None);
            this.SelectedType = CompanyType.None;
            return;
        }

        // Grab available types
        var types = this.SelectedPackageIndex == -1  ? System.Array.Empty<FactionCompanyType>() : factionData.Companies.Types;
        if (types is null || types.Length is 0) {
            this.AvailableTypes.Add(CompanyType.None);
            this.SelectedType = CompanyType.None;
            return;
        }

        // Register
        types.ForEach(x => this.AvailableTypes.Add(new CompanyType(x.Id, x).CacheDisplay()));

        // Set default
        this.SelectedType = this.AvailableTypes[0];

    }

    private void GameChanged() {
        
        RefreshPackages();
        
        SelectedPackageIndex = Packages.Count > 0 ? 0 : -1;
        
        OnPropertyChanged(nameof(SelectedPackageIndex));

        // Clear factions
        AvailableFactions.Clear();
        Games[SelectedGameIndex].Factions.ForEach(AvailableFactions.Add);
        if (AvailableFactions.Count > 0) {
            SelectedFaction = AvailableFactions[0];
            OnPropertyChanged(nameof(SelectedFaction));
        }

    }

    public static void Show(CreateCompanyCallback resultCallback)
        => Show(AppContext.GetModalControl(), resultCallback);

    public static void Show(ModalControl control, CreateCompanyCallback resultCallback) {

        // Create dialog view model
        CreateCompany dialog = new((vm, result) => {
            resultCallback(vm, result);
            control.CloseModal();
        });
        control.ModalMaskBehaviour = ModalBackgroundBehaviour.ExitWhenClicked;
        control.ShowModal(dialog);

    }

}
