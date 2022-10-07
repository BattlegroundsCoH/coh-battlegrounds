using Battlegrounds;
using Battlegrounds.ErrorHandling.CommonExceptions;
using Battlegrounds.Functional;
using Battlegrounds.Game.Database;
using Battlegrounds.Game.Database.Management;
using Battlegrounds.Game.Gameplay;
using Battlegrounds.Locale;
using Battlegrounds.Modding;
using Battlegrounds.Modding.Content.Companies;
using Battlegrounds.Resources;
using Battlegrounds.UI;
using Battlegrounds.UI.Converters.Icons;
using Battlegrounds.UI.Modals;

using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using System.Windows.Media;

namespace BattlegroundsApp.Modals.Dialogs.MVVM.Models;

public class CreateCompanyDialogViewModel : INotifyPropertyChanged {

    public record CompanyType(string Name, FactionCompanyType? Type) {
        public static readonly CompanyType None = new("CreateCompanyDialogView_Type_None", null);
        private SquadBlueprint[]? m_squads;
        private AbilityBlueprint[]? m_abilities;
        public string Desc => BattlegroundsInstance.Localize.GetString($"{this.Name}_desc");
        public ImageSource Icon => StringToCompanyTypeIcon.GetFromType(this.Type);
        public ImageSource? Unit01 => ResourceHandler.GetIcon("unit_icons", this.m_squads![0].UI.Icon);
        public ImageSource? Unit02 => ResourceHandler.GetIcon("unit_icons", this.m_squads![1].UI.Icon);
        public ImageSource? Unit03 => ResourceHandler.GetIcon("unit_icons", this.m_squads![2].UI.Icon);
        public ImageSource? Ability01 => ResourceHandler.GetIcon("ability_icons", this.m_abilities![0].UI.Icon);
        public ImageSource? Ability02 => ResourceHandler.GetIcon("ability_icons", this.m_abilities![1].UI.Icon);
        public ImageSource? Ability03 => ResourceHandler.GetIcon("ability_icons", this.m_abilities![2].UI.Icon);
        public override string ToString() => BattlegroundsInstance.Localize.GetString(this.Name);
        public CompanyType CacheDisplay() {

            // Init bases
            this.m_squads = new SquadBlueprint[3] { SquadBlueprint.Invalid, SquadBlueprint.Invalid, SquadBlueprint.Invalid };
            this.m_abilities = new AbilityBlueprint[3] { AbilityBlueprint.Invalid, AbilityBlueprint.Invalid, AbilityBlueprint.Invalid };

            // Check if there's data to draw values from
            if (this.Type is not null) {

                // Set first three squads if possible
                this.m_squads =
                    this.Type.UIData.HighlightUnits.Length >= 3 ? this.Type.UIData.HighlightUnits[..3].Map(BlueprintManager.FromBlueprintName<SquadBlueprint>) : this.m_squads;
                
                // Set first three abilities if possible
                this.m_abilities =
                    this.Type.UIData.HighlightAbilities.Length >= 3 ? this.Type.UIData.HighlightAbilities[..3].Map(BlueprintManager.FromBlueprintName<AbilityBlueprint>) : this.m_abilities;
            }

            // Return self
            return this;

        }

    }

    public record FactionType(Faction Self) {
        public ImageSource Icon => StringToFactionIcon.GetIcon(this.Self);
        public override string ToString() => GameLocale.GetString(this.Self.NameKey);
    }

    private ModPackage m_package;

    public ModPackage Package {
        get => this.m_package;
        set {
            this.m_package = value;
            this.OnPropertyChanged(nameof(Package));
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

    public bool CanCreate => this.SelectedName.Length > 0 && this.SelectedType != CompanyType.None;

    public CreateCompanyDialogViewModel(Action<CreateCompanyDialogViewModel, ModalDialogResult> resultCallback) {
        
        // Set package
        this.m_package = ModManager.GetPackage("mod_bg") ?? throw new ObjectNotFoundException("Mod package 'mod_bg' not found!"); // TODO: Allow users to pick this

        // Create available factions
        this.AvailableFactions = new() {
            new FactionType(Faction.Soviet),
            new FactionType(Faction.Wehrmacht)
        };

        // Create available types
        this.AvailableTypes = new();
        this.FactionChanged();

        // Set commands
        this.CreateCommand = new RelayCommand(() => resultCallback?.Invoke(this, ModalDialogResult.Confirm));
        this.CancelCommand = new RelayCommand(() => resultCallback?.Invoke(this, ModalDialogResult.Cancel));
    
    }

    public void OnPropertyChanged(string propertyName)
            => this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    private void FactionChanged() {

        // Update faction
        this.OnPropertyChanged(nameof(SelectedFaction));

        // Clear types
        this.AvailableTypes.Clear();

        // Grab available types
        var types = this.m_package.FactionSettings[this.m_companyFaction.Self].Companies.Types;
        if (types is null || types.Length is 0) {
            this.AvailableTypes.Add(CompanyType.None);
            this.SelectedType = CompanyType.None;
            return;
        }

        // Register
        types.ForEach(x => this.AvailableTypes.Add((new CompanyType(x.Id, x)).CacheDisplay()));

        // Set default
        this.SelectedType = this.AvailableTypes[0];

    }

    public static void ShowModal(ModalControl control, Action<CreateCompanyDialogViewModel, ModalDialogResult> resultCallback) {

        // Create dialog view model
        CreateCompanyDialogViewModel dialog = new((vm, result) => {
            resultCallback(vm, result);
            control.CloseModal();
        });
        control.ModalMaskBehaviour = ModalBackgroundBehaviour.ExitWhenClicked;
        control.ShowModal(dialog);

    }

}
