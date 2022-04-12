using Battlegrounds.Game.DataCompany;
using Battlegrounds.Game.Gameplay;
using BattlegroundsApp.MVVM;
using BattlegroundsApp.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace BattlegroundsApp.Modals.Dialogs.MVVM.Models;

public class CreateCompanyDialogViewModel : INotifyPropertyChanged {

    private string _companyName;
    public string CompanyName {
        get {
            return this._companyName;
        }

        set {
            this._companyName = value;
            OnPropertyChanged(nameof(CompanyName));
        }
    }

    private Faction _companyFaction = Faction.Soviet;
    public Faction CompanyFaction {
        get {
            return this._companyFaction;
        }

        set {
            this._companyFaction = value;
            OnPropertyChanged(nameof(CompanyFaction));
        }
    }

    private CompanyType _companyType = CompanyType.Infantry;

    public event PropertyChangedEventHandler? PropertyChanged;

    public void OnPropertyChanged(string propertyName)
            => this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    public CompanyType CompanyType {
        get {
            return this._companyType;
        }

        set {
            this._companyType = value;
            OnPropertyChanged(nameof(CompanyType));
        }
    }

    public ICommand CreateCommand { get; }

    public ICommand CancelCommand { get; }

    public CreateCompanyDialogViewModel(Action<CreateCompanyDialogViewModel, ModalDialogResult> resaultCallback) {
        this.CreateCommand = new RelayCommand(() => resaultCallback?.Invoke(this, ModalDialogResult.Confirm));
        this.CancelCommand = new RelayCommand(() => resaultCallback?.Invoke(this, ModalDialogResult.Cancel));
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
