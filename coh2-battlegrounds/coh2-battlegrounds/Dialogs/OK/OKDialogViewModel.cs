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
    class OKDialogViewModel : ViewModelBase, IDialogViewModelBase<OKDialogResult> {

        public ICommand OKCommand { get; private set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public OKDialogResult DialogResult { get; set; }
        public OKDialogResult DialogCloseDefault => OKDialogResult.OK;

        public OKDialogViewModel(string title, string message) {

            Title = title;
            Message = message;
            OKCommand = new RelayCommand<IDialogWindow>(OK);

        }

        private void OK(IDialogWindow window) {

            CloseDialogWithResult(window, OKDialogResult.OK);

        }

        public void CloseDialogWithResult(IDialogWindow dialog, OKDialogResult result) {

            DialogResult = result;

            if (dialog != null) dialog.DialogResult = true;

        }

    }
}
