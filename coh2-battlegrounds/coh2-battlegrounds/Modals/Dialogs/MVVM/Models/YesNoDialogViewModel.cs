using BattlegroundsApp.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace BattlegroundsApp.Modals.Dialogs.MVVM.Models;

public class YesNoDialogViewModel {

    public string DialogTitle { get; set; }

    public string DialogMessage { get; set; }

    public ICommand YesCommand { get; }

    public ICommand NoCommand { get; }

    public YesNoDialogViewModel(Action<YesNoDialogViewModel, ModalDialogResult> resaultCallback, string title, string message) {

        this.YesCommand = new RelayCommand(() => resaultCallback?.Invoke(this, ModalDialogResult.Confirm));
        this.NoCommand = new RelayCommand(() => resaultCallback?.Invoke(this, ModalDialogResult.Cancel));

        this.DialogTitle = title;
        this.DialogMessage = message;

    }

    public static void ShowModal(ModalControl control, Action<YesNoDialogViewModel, ModalDialogResult> resaultCallback,
                                 string title, string message) {

        // Create dialog view model
        YesNoDialogViewModel dialog = new((vm, result) => {
            resaultCallback(vm, result);
            control.CloseModal();
        }, title, message);

        control.ModalMaskBehaviour = ModalBackgroundBehaviour.None;
        control.ShowModal(dialog);

    }

}
