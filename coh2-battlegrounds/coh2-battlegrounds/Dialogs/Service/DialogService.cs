using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace BattlegroundsApp.Dialogs.Service {
    public class DialogService : IDialogService {
        public T OpenDialog<T>(IDialogViewModelBase<T> viewModel) where T : Enum {

            IDialogWindow window = new DialogWindow();
            window.DataContext = viewModel;
            if (window.ShowDialog() == true) { 
                return viewModel.DialogResult;
            } else {
                return viewModel.DialogCloseDefault;
            }
        }
    }
}
