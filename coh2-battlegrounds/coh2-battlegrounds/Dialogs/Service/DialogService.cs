using System;
using System.Collections.Generic;
using System.Text;

namespace BattlegroundsApp.Dialogs.Service {
    public class DialogService : IDialogService {
        public T OpenDialog<T>(DialogViewModelBase<T> viewModel) {

            IDialogWindow window = new DialogWindow();
            window.DataContext = viewModel;
            window.ShowDialog();
            return viewModel.DialogResult;

        }
    }
}
