using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Battlegrounds.Locale;
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
        public LocaleKey Message { get; set; }
        public LocaleKey ConfirmButtonContent { get; }
        public LocaleKey CancelButtonContent { get; }

        private YesNoDialogViewModel(LocaleKey title, LocaleKey message) {

            // Define locales
            ConfirmButtonContent = new LocaleKey("YesNoDialogView_Confirm");
            CancelButtonContent = new LocaleKey("YesNoDialogView_Cancel");

            Title = title;
            Message = message;
            ConfirmCommand = new RelayCommand<DialogWindow>(Confirm);
            CancelCommand = new RelayCommand<DialogWindow>(Cancel);
            DialogCloseDefault = YesNoDialogResult.Cancel;

        }

        public static YesNoDialogResult ShowYesNoDialog(LocaleKey title, LocaleKey message) {
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
