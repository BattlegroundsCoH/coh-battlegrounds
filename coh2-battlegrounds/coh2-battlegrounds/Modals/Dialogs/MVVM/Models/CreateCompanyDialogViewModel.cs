using Battlegrounds;
using Battlegrounds.ErrorHandling.CommonExceptions;
using Battlegrounds.Functional;
using Battlegrounds.Game.Gameplay;
using Battlegrounds.Modding;
using Battlegrounds.Modding.Content.Companies;

using BattlegroundsApp.Utilities;

using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;

namespace BattlegroundsApp.Modals.Dialogs.MVVM.Models;

public class CreateCompanyDialogViewModel : INotifyPropertyChanged {

    public record CompanyType(string Name, FactionCompanyType? Type) {
        public static readonly CompanyType None = new("CreateCompanyDialogView_Type_None", null);
        public override string ToString() => BattlegroundsInstance.Localize.GetString(this.Name);
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

    private Faction m_companyFaction = Faction.Soviet;

    public Faction SelectedFaction {
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

    public ObservableCollection<CompanyType> AvailableTypes { get; }

    public ICommand CreateCommand { get; }

    public ICommand CancelCommand { get; }

    public bool CanCreate => this.SelectedName.Length > 0 && this.SelectedType != CompanyType.None;

    public CreateCompanyDialogViewModel(Action<CreateCompanyDialogViewModel, ModalDialogResult> resaultCallback) {
        
        // Set package
        this.m_package = ModManager.GetPackage("mod_bg") ?? throw new ObjectNotFoundException("Mod package 'mod_bg' not found!"); // TODO: Allow users to pick this

        // Create available types
        this.AvailableTypes = new();
        this.FactionChanged();

        // Set commands
        this.CreateCommand = new RelayCommand(() => resaultCallback?.Invoke(this, ModalDialogResult.Confirm));
        this.CancelCommand = new RelayCommand(() => resaultCallback?.Invoke(this, ModalDialogResult.Cancel));
    
    }

    public void OnPropertyChanged(string propertyName)
            => this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    private void FactionChanged() {

        // Update faction
        this.OnPropertyChanged(nameof(SelectedFaction));

        // Clear types
        this.AvailableTypes.Clear();

        // Grab available types
        var types = this.m_package.FactionSettings[this.m_companyFaction].Companies.Types;
        if (types is null || types.Length is 0) {
            this.AvailableTypes.Add(CompanyType.None);
            this.SelectedType = CompanyType.None;
            return;
        }

        // Register
        types.ForEach(x => this.AvailableTypes.Add(new(x.Id, x)));

        // Set default
        this.SelectedType = this.AvailableTypes[0];

    }

    public static void ShowModal(ModalControl control, Action<CreateCompanyDialogViewModel, ModalDialogResult> resaultCallback) {

        // Create dialog view model
        CreateCompanyDialogViewModel dialog = new((vm, result) => {
            resaultCallback(vm, result);
            control.CloseModal();
        });
        control.ModalMaskBehaviour = ModalBackgroundBehaviour.ExitWhenClicked;
        control.ShowModal(dialog);

    }

}
