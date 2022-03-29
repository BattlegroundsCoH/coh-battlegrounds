using BattlegroundsApp.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace BattlegroundsApp.Modals.Dialogs.MVVM.Models;

public class HostGameDialogViewModel : INotifyPropertyChanged {

    private string _lobbyName;
    public string LobbyName {
        get => this._lobbyName;
        set {
            this._lobbyName = value;
            OnPropertyChanged(nameof(this.LobbyName));
        }
    }

    private string _lobbyPassword;
    public string LobbyPassword {
        get => this._lobbyPassword;
        set {
            this._lobbyPassword = value;
            this.OnPropertyChanged(nameof(this.LobbyPassword));
        }

    }

    public event PropertyChangedEventHandler PropertyChanged;

    public virtual void OnPropertyChanged(string propertyName)
            => this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    public ICommand HostCommand { get; }

    public ICommand CancelCommand { get; }

    public HostGameDialogViewModel(Action<HostGameDialogViewModel, ModalDialogResult> resultCallback) {

        this.HostCommand = new RelayCommand(() => resultCallback?.Invoke(this, ModalDialogResult.Confirm));
        this.CancelCommand = new RelayCommand(() => resultCallback?.Invoke(this, ModalDialogResult.Cancel));

    }

    public static void ShowModal(ModalControl control, Action<HostGameDialogViewModel, ModalDialogResult> resultCallback) {

        // Create dialog view model
        HostGameDialogViewModel dialog = new((vm, result) => {
            resultCallback(vm, result);
            control.CloseModal();
        });
        control.ModalMaskBehaviour = ModalBackgroundBehaviour.ExitWhenClicked;
        control.ShowModal(dialog);

    }

}
