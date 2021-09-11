using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Battlegrounds.Locale;
using BattlegroundsApp.Dialogs.Service;
using BattlegroundsApp.Utilities;

namespace BattlegroundsApp.Dialogs.RenameCopyDialog {
    public enum RenameCopyDialogType {
        Rename,
        Copy
    }
    public enum RenameCopyDialogResult {
        Rename,
        Copy,
        Cancel
    }
    public class RenameCopyDialogViewModel : DialogControlBase<RenameCopyDialogResult> {

        public ICommand RenameCopyCommand { get; set; }
        public ICommand CancelCommand { get; set; }
        public LocaleKey CompanyNameTextBlockText { get; set; }
        public LocaleKey RenameCopyButtonContent { get; set; }
        public LocaleKey CancelButtonContent { get; set; }
        public RenameCopyDialogType Type { get; set; }
        private string _companyName;
        public string CompanyName {
            get {
                return _companyName;
            }

            set {
                this._companyName = value;
                OnPropertyChanged(nameof(CompanyName));
            }
        }

        private RenameCopyDialogViewModel(RenameCopyDialogType type, LocaleKey title) {

            Title = title;

            // Define locales
            CompanyNameTextBlockText = new LocaleKey("RenameCopyDialogView_Company_Name");
            RenameCopyButtonContent = (type == RenameCopyDialogType.Rename) ? new LocaleKey("RenameCopyDialogView_Rename") : new LocaleKey("RenameCopyDialogView_Copy");
            CancelButtonContent = new LocaleKey("RenameCopyDialogView_Cancel");

            CancelCommand = new RelayCommand<DialogWindow>(Cancel);
            RenameCopyCommand = (type == RenameCopyDialogType.Rename) ? new RelayCommand<DialogWindow>(Rename) : new RelayCommand<DialogWindow>(Copy);
            Type = (type == RenameCopyDialogType.Rename) ? RenameCopyDialogType.Rename : RenameCopyDialogType.Copy;
            DialogCloseDefault = RenameCopyDialogResult.Cancel;

        }

        public static RenameCopyDialogResult ShowRenameDialog(LocaleKey title, out string companyName) {
            var dialog = new RenameCopyDialogViewModel(RenameCopyDialogType.Rename, title);
            var result = dialog.ShowDialog();
            companyName = dialog.CompanyName;
            return result;
        }

        public static RenameCopyDialogResult ShowCopyDialog(LocaleKey title, out string companyName) {
            var dialog = new RenameCopyDialogViewModel(RenameCopyDialogType.Copy, title);
            var result = dialog.ShowDialog();
            companyName = dialog.CompanyName;
            return result;

        }

        private void Rename(DialogWindow window) {
            CloseDialogWithResult(window, RenameCopyDialogResult.Rename);
        }

        private void Copy(DialogWindow window) {
            CloseDialogWithResult(window, RenameCopyDialogResult.Copy);
        }

        private void Cancel(DialogWindow window) {
            CloseDialogWithResult(window, RenameCopyDialogResult.Cancel);
        }
    }
}
