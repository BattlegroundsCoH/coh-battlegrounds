using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using BattlegroundsApp.Dialogs.Service;
using BattlegroundsApp.Utilities;

namespace BattlegroundsApp.Dialogs.HostGame {
    public class HostGameDialogViewModel : DialogViewModelBase<DialogResults> {

        public ICommand HostCommand { get; private set; }
        public ICommand CancelCommand { get; private set; }

        private string lobbyName;
        public string LobbyName {
            get { return this.lobbyName; }

            set {

                if (!string.Equals(this.lobbyName, value)) {

                    this.lobbyName = value;

                }

            }
        }

        private string lobbyPassword;
        public string LobbyPassword {
            get { return this.lobbyPassword; }

            set {

                if (!string.Equals(this.lobbyPassword, value)) {

                    this.lobbyPassword = value;

                }

            }

        }

        public HostGameDialogViewModel(string title) : base(title) {

            HostCommand = new RelayCommand<IDialogWindow>(Host);
            CancelCommand = new RelayCommand<IDialogWindow>(Cancel);

        }

        private void Host(IDialogWindow window) {

            CloseDialogWithResult(window, DialogResults.Host);

        }

        private void Cancel(IDialogWindow window) {

            CloseDialogWithResult(window, DialogResults.Cancel);

        }

    }
}
