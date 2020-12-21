using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using BattlegroundsApp.Dialogs.Service;
using BattlegroundsApp.Utilities;
using BattlegroundsApp.ViewModels;

namespace BattlegroundsApp.Dialogs.HostGame {
    public class HostGameDialogViewModel : DialogViewModelBase<DialogResults> {

        public ICommand HostCommand { get; private set; }
        public ICommand CancelCommand { get; private set; }

        private string _lobbyName;
        public string LobbyName {
            get { return this._lobbyName; }

            set {

                if (!string.Equals(this._lobbyName, value)) {

                    this._lobbyName = value;

                }

            }
        }

        private string _lobbyPassword;
        public string LobbyPassword {
            get { return this._lobbyPassword; }

            set {

                if (!string.Equals(this._lobbyPassword, value)) {

                    this._lobbyPassword = value;

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
