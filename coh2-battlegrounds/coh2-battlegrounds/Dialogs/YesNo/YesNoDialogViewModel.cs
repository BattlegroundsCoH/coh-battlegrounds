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
    class YesNoDialogViewModel : ViewModelBase, IDialogViewModelBase<YesNoDialogResult> {

        public ICommand ConfirmCommand { get; set; }
        public ICommand CancelCommand { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public YesNoDialogResult DialogResult { get; set; }
        public YesNoDialogResult DialogCloseDefault => YesNoDialogResult.Cancel;

        public YesNoDialogViewModel(string title, string message) {

            Title = title;
            Message = message;
            ConfirmCommand = new RelayCommand<IDialogWindow>(Confirm);
            CancelCommand = new RelayCommand<IDialogWindow>(Cancel);

        }

        private void Confirm(IDialogWindow window) {

            CloseDialogWithResult(window, YesNoDialogResult.Confirm);

        }

        private void Cancel(IDialogWindow window) {

            CloseDialogWithResult(window, YesNoDialogResult.Cancel);

        }

        public void CloseDialogWithResult(IDialogWindow dialog, YesNoDialogResult result) {

            DialogResult = result;

            if (dialog != null) dialog.DialogResult = true;

        }

    }
}
