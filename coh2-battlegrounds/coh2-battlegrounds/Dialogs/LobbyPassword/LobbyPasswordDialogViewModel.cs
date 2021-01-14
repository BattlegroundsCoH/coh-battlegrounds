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
    class LobbyPasswordDialogViewModel : DialogWindowBase<LobbyPasswordDialogResult> {

        public ICommand JoinCommand { get; private set; }
        public ICommand CandelCommand { get; private set; }

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

        private LobbyPasswordDialogViewModel(string title) {

            Title = title;
            JoinCommand = new RelayCommand<DialogWindow>(Join);
            CandelCommand = new RelayCommand<DialogWindow>(Cancel);
            DialogCloseDefault = LobbyPasswordDialogResult.Cancel;

        }

        public static LobbyPasswordDialogResult ShowLobbyPasswordDialog(string title, out string password) {
            var dialog = new LobbyPasswordDialogViewModel(title);
            var result = dialog.ShowDialog();
            password = dialog.Password;
            return result;
        }

        private void Join(DialogWindow window) {

            CloseDialogWithResult(window, LobbyPasswordDialogResult.Join);

        }

        private void Cancel(DialogWindow window) {

            CloseDialogWithResult(window, LobbyPasswordDialogResult.Cancel);

        }
    }
}
