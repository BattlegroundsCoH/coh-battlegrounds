using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using BattlegroundsApp.Dialogs.Service;
using BattlegroundsApp.Utilities;
using BattlegroundsApp.ViewModels;

namespace BattlegroundsApp.Dialogs.CreateCompany {
    public enum CreateCompanyDialogResult {
        Create,
        Cancel
    }
    class CreateCompanyDialogViewModel : ViewModelBase, IDialogViewModelBase<CreateCompanyDialogResult> {
    
        public ICommand CreateCommand { get; set; }
        public ICommand CancelCommand { get; set; }
        public string Title { get; set; }
        public CreateCompanyDialogResult DialogResult { get; set; }
        public CreateCompanyDialogResult DialogCloseDefault => CreateCompanyDialogResult.Cancel;

        private string _companyName;
        public string CompanyName {
            get {
                return this._companyName;
            }

            set {
                this._companyName = value;
                OnPropertyChanged(nameof(CompanyName));
            }
        }

        // TODO: From combobox selection conver to string or whatever will be needed
        private string _companyType;
        public string CompanyType {
            get {
                return this._companyType;
            }

            set {
                this._companyType = value;
                OnPropertyChanged(nameof(CompanyType));
            }
        }

        public CreateCompanyDialogViewModel(string title) {

            Title = title;
            CreateCommand = new RelayCommand<IDialogWindow>(Create);
            CancelCommand = new RelayCommand<IDialogWindow>(Cancel);

        }

        private void Create(IDialogWindow window) {

            CloseDialogWithResult(window, CreateCompanyDialogResult.Create);

        }

        private void Cancel(IDialogWindow window) {

            CloseDialogWithResult(window, CreateCompanyDialogResult.Cancel);

        }

        public void CloseDialogWithResult(IDialogWindow dialog, CreateCompanyDialogResult result) {

            DialogResult = result;

            if (dialog != null) dialog.DialogResult = true;

        }
    
    }
}
