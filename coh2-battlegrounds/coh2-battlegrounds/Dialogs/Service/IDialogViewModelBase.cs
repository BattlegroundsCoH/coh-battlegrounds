using Battlegrounds.Locale;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BattlegroundsApp.Dialogs.Service {
    public interface IDialogViewModelBase<T> {

        public LocaleKey Title { get; set; }
        public T DialogResult { get; set; }
        public T DialogCloseDefault { get; }
        public void CloseDialogWithResult(DialogWindow dialog, T result);

    }
}
