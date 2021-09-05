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

namespace BattlegroundsApp.Dialogs.OK {
    public enum OKDialogResult {
        OK,
    }
    class OKDialogViewModel : DialogControlBase<OKDialogResult> {

        public ICommand OKCommand { get; private set; }
        public LocaleKey Message { get; set; }
        public LocaleKey OKButtonContent { get; }

        private OKDialogViewModel(LocaleKey title, LocaleKey message) {

            Title = title;
            Message = message;
            OKButtonContent = new LocaleKey("OKDialogView_OK");
            OKCommand = new RelayCommand<DialogWindow>(OK);
            DialogCloseDefault = OKDialogResult.OK;

        }

        public static OKDialogResult ShowOKDialog(LocaleKey title, LocaleKey message) {
            var dialog = new OKDialogViewModel(title, message);
            return dialog.ShowDialog();
        }

        private void OK(DialogWindow window) {

            CloseDialogWithResult(window, OKDialogResult.OK);

        }

    }
}
