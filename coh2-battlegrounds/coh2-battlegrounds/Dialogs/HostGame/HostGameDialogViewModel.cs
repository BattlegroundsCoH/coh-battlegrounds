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
    public class HostGameDialogViewModel : ViewModelBase, IDialogViewModelBase<HostGameDialogResult> {

        public ICommand HostCommand { get; private set; }
        public ICommand CancelCommand { get; private set; }
        public string Title { get; set; }
        public HostGameDialogResult DialogResult { get; set; }
        public HostGameDialogResult DialogCloseDefault => HostGameDialogResult.Cancel;

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

        public HostGameDialogViewModel(string title) {

            Title = title;
            HostCommand = new RelayCommand<IDialogWindow>(Host);
            CancelCommand = new RelayCommand<IDialogWindow>(Cancel);

        }

        private void Host(IDialogWindow window) {

            CloseDialogWithResult(window, HostGameDialogResult.Host);

        }

        private void Cancel(IDialogWindow window) {

            CloseDialogWithResult(window, HostGameDialogResult.Cancel);

        }

        public void CloseDialogWithResult(IDialogWindow dialog, HostGameDialogResult result) {

            DialogResult = result;

            if (dialog != null) dialog.DialogResult = true;

        }
    }
}
