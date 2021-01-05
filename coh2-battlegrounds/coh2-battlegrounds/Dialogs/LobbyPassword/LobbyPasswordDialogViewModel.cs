using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using BattlegroundsApp.Dialogs.Service;
using BattlegroundsApp.Utilities;
using BattlegroundsApp.ViewModels;

namespace BattlegroundsApp.Dialogs.LobbyPassword {
    public enum LobbyPasswordDialogResult {
        Join,
        Cancel
    }
    class LobbyPasswordDialogViewModel :ViewModelBase, IDialogViewModelBase<LobbyPasswordDialogResult> {

        public ICommand JoinCommand { get; private set; }
        public ICommand CandelCommand { get; private set; }
        public string Title { get; set; }
        public LobbyPasswordDialogResult DialogResult { get; set; }
        public LobbyPasswordDialogResult DialogCloseDefault => LobbyPasswordDialogResult.Cancel;

        private string _password;
        public string Password {
            get {
                return this._password;
            }

            set {
                this._password = value;
                OnPropertyChanged(nameof(Password));
            }
        }

        public LobbyPasswordDialogViewModel(string title) {

            Title = title;
            JoinCommand = new RelayCommand<IDialogWindow>(Join);
            CandelCommand = new RelayCommand<IDialogWindow>(Cancel);

        }

        private void Join(IDialogWindow window) {

            CloseDialogWithResult(window, LobbyPasswordDialogResult.Join);

        }

        private void Cancel(IDialogWindow window) {

            CloseDialogWithResult(window, LobbyPasswordDialogResult.Cancel);

        }

        public void CloseDialogWithResult(IDialogWindow dialog, LobbyPasswordDialogResult result) {

            DialogResult = result;

            if (dialog != null) dialog.DialogResult = true;

        }
    }
}
