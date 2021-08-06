using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

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
        public string ButtonName { get; set; }
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

        private RenameCopyDialogViewModel(RenameCopyDialogType type, string title) {

            Title = title;
            CancelCommand = new RelayCommand<DialogWindow>(Cancel);
            RenameCopyCommand = (type == RenameCopyDialogType.Rename) ? new RelayCommand<DialogWindow>(Rename) : new RelayCommand<DialogWindow>(Copy);
            ButtonName = (type == RenameCopyDialogType.Rename) ? "Rename" : "Copy";
            Type = (type == RenameCopyDialogType.Rename) ? RenameCopyDialogType.Rename : RenameCopyDialogType.Copy;
            DialogCloseDefault = RenameCopyDialogResult.Cancel;

        }

        public static RenameCopyDialogResult ShowRenameDialog(string title, out string companyName) {
            var dialog = new RenameCopyDialogViewModel(RenameCopyDialogType.Rename, title);
            var result = dialog.ShowDialog();
            companyName = dialog.CompanyName;
            return result;
        }

        public static RenameCopyDialogResult ShowCopyDialog(string title, out string companyName) {
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
