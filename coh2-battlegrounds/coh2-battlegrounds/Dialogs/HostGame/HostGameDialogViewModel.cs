using System.Windows.Input;

using BattlegroundsApp.Dialogs.Service;
using BattlegroundsApp.Utilities;

namespace BattlegroundsApp.Dialogs.HostGame {

    public enum HostGameDialogResult {
        Host,
        Cancel
    }

    public class HostGameDialogViewModel : DialogControlBase<HostGameDialogResult> {

        public ICommand HostCommand { get; private set; }
        public ICommand CancelCommand { get; private set; }

        private string _lobbyName;
        public string LobbyName {
            get => this._lobbyName;
            set {
                this._lobbyName = value;
                OnPropertyChanged(nameof(this.LobbyName));
            }
        }

        private string _lobbyPassword;
        public string LobbyPassword {
            get => this._lobbyPassword;
            set {
                this._lobbyPassword = value;
                this.OnPropertyChanged(nameof(this.LobbyPassword));
            }

        }

        private HostGameDialogViewModel(string title) {

            this.Title = title;
            this.HostCommand = new RelayCommand<DialogWindow>(this.Host);
            this.CancelCommand = new RelayCommand<DialogWindow>(this.Cancel);
            this.DialogCloseDefault = HostGameDialogResult.Cancel;

        }

        public static HostGameDialogResult ShowHostGameDialog(string title, out string lobbyName, out string lobbyPwd) {
            var dialog = new HostGameDialogViewModel(title);
            var result = dialog.ShowDialog();
            lobbyName = dialog.LobbyName;
            lobbyPwd = dialog.LobbyPassword;
            return result;
        }

        private void Host(DialogWindow window) => this.CloseDialogWithResult(window, HostGameDialogResult.Host);

        private void Cancel(DialogWindow window) => this.CloseDialogWithResult(window, HostGameDialogResult.Cancel);

    }

}
