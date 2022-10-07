using System;
using System.ComponentModel;
using System.Windows.Input;

using Battlegrounds.UI;
using Battlegrounds.UI.Modals;

namespace BattlegroundsApp.Modals.Dialogs.MVVM.Models;

public class LobbyJoinDialogViewModel : INotifyPropertyChanged {

    private string? _password;

    public string Password {
        get {
            return this._password ?? string.Empty;
        }

        set {
            this._password = value;
            OnPropertyChanged(nameof(Password));
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public virtual void OnPropertyChanged(string propertyName)
            => this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    public ICommand JoinCommand { get; }
    public ICommand CancelCommand { get; }

    public LobbyJoinDialogViewModel(Action<LobbyJoinDialogViewModel, ModalDialogResult> resultCallback) {

        this.JoinCommand = new RelayCommand(() => resultCallback?.Invoke(this, ModalDialogResult.Confirm));
        this.CancelCommand = new RelayCommand(() => resultCallback?.Invoke(this, ModalDialogResult.Cancel));

    }

    public static void ShowModal(ModalControl control, Action<LobbyJoinDialogViewModel, ModalDialogResult> resultCallback) {

        // Create dialog view model
        LobbyJoinDialogViewModel dialog = new((vm, result) => {
            resultCallback(vm, result);
            control.CloseModal();
        });
        control.ModalMaskBehaviour = ModalBackgroundBehaviour.ExitWhenClicked;
        control.ShowModal(dialog);

    }

}
