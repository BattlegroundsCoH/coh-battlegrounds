using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace BattlegroundsApp.Dialogs.Service {
    public interface IDialogService {

        T OpenDialog<T>(IDialogViewModelBase<T> viewModel) where T : System.Enum;

    }
}
