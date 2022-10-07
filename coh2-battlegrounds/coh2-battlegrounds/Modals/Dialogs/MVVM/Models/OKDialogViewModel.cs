using Battlegrounds.UI;
using Battlegrounds.UI.Modals;

using System;
using System.Windows.Input;

namespace BattlegroundsApp.Modals.Dialogs.MVVM.Models;

public class OKDialogViewModel {

    public string DialogTitle { get; set; }

    public string DialogMessage { get; set; }

    public ICommand OKCommand { get; }

    public OKDialogViewModel(Action<OKDialogViewModel, ModalDialogResult> resultCallback, string title, string message) {

        this.OKCommand = new RelayCommand(() => resultCallback?.Invoke(this, ModalDialogResult.Confirm));

        this.DialogTitle = title;
        this.DialogMessage = message;

    }

    public static void ShowModal(ModalControl control, Action<OKDialogViewModel, ModalDialogResult> resultCallback,
                                 string title, string message) {

        // Create dialog view model
        OKDialogViewModel dialog = new((vm, result) => {
            resultCallback(vm, result);
            control.CloseModal();
        }, title, message);

        control.ModalMaskBehaviour = ModalBackgroundBehaviour.ExitWhenClicked;
        control.ShowModal(dialog);

    }

}
