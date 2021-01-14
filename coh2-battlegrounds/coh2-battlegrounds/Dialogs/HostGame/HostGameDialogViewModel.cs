using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using BattlegroundsApp.Dialogs.Service;
using BattlegroundsApp.Utilities;
using BattlegroundsApp.ViewModels;

namespace BattlegroundsApp.Dialogs.HostGame {
    public enum HostGameDialogResult {
        Host,
        Cancel
    }
    public class HostGameDialogViewModel : DialogWindowBase<HostGameDialogResult> {

        public ICommand HostCommand { get; private set; }
        public ICommand CancelCommand { get; private set; }

        private string _lobbyName;
        public string LobbyName {
            get { 

                return this._lobbyName; 

            }

            set {

                this._lobbyName = value;
                OnPropertyChanged(nameof(LobbyName));

            }
        }

        private string _lobbyPassword;
        public string LobbyPassword {
            get { 
                
                return this._lobbyPassword; 
            
            }

            set {

                this._lobbyPassword = value;
                OnPropertyChanged(nameof(LobbyPassword));

            }

        }

        private HostGameDialogViewModel(string title) {

            Title = title;
            HostCommand = new RelayCommand<DialogWindow>(Host);
            CancelCommand = new RelayCommand<DialogWindow>(Cancel);
            DialogCloseDefault = HostGameDialogResult.Cancel;

        }

        public static HostGameDialogResult ShowHostGameDialog(string title, out string lobbyName, out string lobbyPwd) {
            var dialog = new HostGameDialogViewModel(title);
            var result = dialog.ShowDialog();
            lobbyName = dialog.LobbyName;
            lobbyPwd = dialog.LobbyPassword;
            return result;
        }

        private void Host(DialogWindow window) {

            CloseDialogWithResult(window, HostGameDialogResult.Host);

        }

        private void Cancel(DialogWindow window) {

            CloseDialogWithResult(window, HostGameDialogResult.Cancel);

        }
    }
}
