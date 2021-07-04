using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using BattlegroundsApp.Dialogs.Service;
using BattlegroundsApp.Utilities;
using BattlegroundsApp.ViewModels;

namespace BattlegroundsApp.Dialogs.YesNo {
    public enum YesNoDialogResult {
        Confirm,
        Cancel
    }
    class YesNoDialogViewModel : DialogControlBase<YesNoDialogResult> {

        public ICommand ConfirmCommand { get; set; }
        public ICommand CancelCommand { get; set; }
        public string Message { get; set; }

        private YesNoDialogViewModel(string title, string message) {

            Title = title;
            Message = message;
            ConfirmCommand = new RelayCommand<DialogWindow>(Confirm);
            CancelCommand = new RelayCommand<DialogWindow>(Cancel);
            DialogCloseDefault = YesNoDialogResult.Cancel;

        }

        public static YesNoDialogResult ShowYesNoDialog(string title, string message) {
            var dialog = new YesNoDialogViewModel(title, message);
            return dialog.ShowDialog();
        }

        private void Confirm(DialogWindow window) {

            CloseDialogWithResult(window, YesNoDialogResult.Confirm);

        }

        private void Cancel(DialogWindow window) {

            CloseDialogWithResult(window, YesNoDialogResult.Cancel);

        }
    }
}
