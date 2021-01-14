using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using BattlegroundsApp.Dialogs.Service;
using BattlegroundsApp.Utilities;
using BattlegroundsApp.ViewModels;

namespace BattlegroundsApp.Dialogs.RenameCompany {
    public enum RenameCompanyDialogResult {
        Rename,
        Cancel
    }
    class RenameCompanyDialogViewModel : ViewModelBase, IDialogViewModelBase<RenameCompanyDialogResult> {
        
        public ICommand RenameCommand { get; set; }
        public ICommand CancelCommand { get; set; }
        public string Title { get; set; }
        public RenameCompanyDialogResult DialogResult { get; set; }

        public RenameCompanyDialogResult DialogCloseDefault => RenameCompanyDialogResult.Cancel;

        private string _newCompanyName;
        public string NewCompanyName {
            get {
                return this._newCompanyName;
            }

            set {
                this._newCompanyName = value;
                OnPropertyChanged(nameof(NewCompanyName));
            }
        }

        public RenameCompanyDialogViewModel(string title) {

            Title = title;
            RenameCommand = new RelayCommand<IDialogWindow>(Rename);
            CancelCommand = new RelayCommand<IDialogWindow>(Cancel);

        }

        private void Rename(IDialogWindow window) {

            CloseDialogWithResult(window, RenameCompanyDialogResult.Rename);

        }

        private void Cancel(IDialogWindow window) {

            CloseDialogWithResult(window, RenameCompanyDialogResult.Cancel);

        }

        public void CloseDialogWithResult(IDialogWindow dialog, RenameCompanyDialogResult result) => throw new NotImplementedException();
    }
}
