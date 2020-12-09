using System;
using System.Collections.Generic;
using System.Text;

namespace BattlegroundsApp.Dialogs.Service {
    public interface IDialogService {

        T OpenDialog<T>(DialogViewModelBase<T> viewModel);

    }
}
