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
