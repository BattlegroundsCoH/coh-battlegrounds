using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows.Input;
using BattlegroundsApp.Dialogs.HostGame;
using BattlegroundsApp.Dialogs.Service;
using BattlegroundsApp.Utilities;

namespace BattlegroundsApp.ViewModels {
    public class GameBrowserViewModel {

        private IDialogService _dialogService;

        public ICommand HostGameCommand { get; private set; }

        public GameBrowserViewModel() {

            // TODO: Make this with injection
            _dialogService = new DialogService();

            HostGameCommand = new RelayCommand(HostGame);

        }

        public void HostGame() {

            var dialog = new HostGameDialogViewModel("Host Game");
            var result = _dialogService.OpenDialog(dialog);

            // TODO
            if (result == Dialogs.DialogResults.Host) {

            }

        }

    }
}
