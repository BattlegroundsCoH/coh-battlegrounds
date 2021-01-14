using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using BattlegroundsApp.Dialogs.Service;
using BattlegroundsApp.Utilities;
using BattlegroundsApp.ViewModels;

namespace BattlegroundsApp.Dialogs.OK {
    public enum OKDialogResult {
        OK,
    }
    class OKDialogViewModel : DialogWindowBase<OKDialogResult> {

        public ICommand OKCommand { get; private set; }
        public string Message { get; set; }

        private OKDialogViewModel(string title, string message) {

            Title = title;
            Message = message;
            OKCommand = new RelayCommand<DialogWindow>(OK);
            DialogCloseDefault = OKDialogResult.OK;

        }

        public static OKDialogResult ShowOKDialog(string title, string message) {
            var dialog = new OKDialogViewModel(title, message);
            return dialog.ShowDialog();
        }

        private void OK(DialogWindow window) {

            CloseDialogWithResult(window, OKDialogResult.OK);

        }

    }
}
