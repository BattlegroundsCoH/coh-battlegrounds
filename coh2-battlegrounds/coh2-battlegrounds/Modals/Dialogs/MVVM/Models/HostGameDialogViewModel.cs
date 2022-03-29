using BattlegroundsApp.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace BattlegroundsApp.Modals.Dialogs.MVVM.Models;

public class HostGameDialogViewModel {

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
