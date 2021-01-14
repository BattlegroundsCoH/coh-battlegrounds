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
    class CreateCompanyDialogViewModel : DialogWindowBase<CreateCompanyDialogResult> {
    
        public ICommand CreateCommand { get; set; }
        public ICommand CancelCommand { get; set; }

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

        private CreateCompanyDialogViewModel(string title) {

            Title = title;
            CreateCommand = new RelayCommand<DialogWindow>(Create);
            CancelCommand = new RelayCommand<DialogWindow>(Cancel);
            DialogCloseDefault = CreateCompanyDialogResult.Cancel;

        }

        public static CreateCompanyDialogResult ShowCreateCompanyDialog(string title) {
            var dialog = new CreateCompanyDialogViewModel(title);
            return dialog.ShowDialog();
        }

        private void Create(DialogWindow window) {

            CloseDialogWithResult(window, CreateCompanyDialogResult.Create);

        }

        private void Cancel(DialogWindow window) {

            CloseDialogWithResult(window, CreateCompanyDialogResult.Cancel);

        }

        public override void CloseDialogWithResult(DialogWindow dialog, CreateCompanyDialogResult result) {

            DialogResult = result;

            if (dialog != null) dialog.DialogResult = true;

        }
    }
}
