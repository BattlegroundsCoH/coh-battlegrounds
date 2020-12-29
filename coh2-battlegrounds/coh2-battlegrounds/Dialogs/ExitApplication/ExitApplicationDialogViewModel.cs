using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using BattlegroundsApp.Dialogs.Service;
using BattlegroundsApp.Utilities;
using BattlegroundsApp.ViewModels;

namespace BattlegroundsApp.Dialogs.ExitApplication {
    public enum ExitApplicationDialogResult {
        Exit,
        Cancel
    }
    public class ExitApplicationDialogViewModel : ViewModelBase, IDialogViewModelBase<ExitApplicationDialogResult> {

        public ICommand ExitCommand { get; set; }
        public ICommand CancelCommand { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public ExitApplicationDialogResult DialogResult { get; set; }
        public ExitApplicationDialogResult DialogCloseDefault => ExitApplicationDialogResult.Cancel;

        public ExitApplicationDialogViewModel(string title, string message) {

            Title = title;
            Message = message;
            ExitCommand = new RelayCommand<IDialogWindow>(Exit);
            CancelCommand = new RelayCommand<IDialogWindow>(Cancel);

        }

        private void Exit(IDialogWindow window) {

            CloseDialogWithResult(window, ExitApplicationDialogResult.Exit);

        }

        private void Cancel(IDialogWindow window) {

            CloseDialogWithResult(window, ExitApplicationDialogResult.Cancel);

        }

        public void CloseDialogWithResult(IDialogWindow dialog, ExitApplicationDialogResult result) {

            DialogResult = result;

            if (dialog != null) dialog.DialogResult = true;

        }
    }
}
