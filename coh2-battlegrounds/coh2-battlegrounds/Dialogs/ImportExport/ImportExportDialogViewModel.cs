using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Battlegrounds.Locale;
using BattlegroundsApp.Dialogs.Service;
using BattlegroundsApp.Utilities;
using BattlegroundsApp.ViewModels;

namespace BattlegroundsApp.Dialogs.ImportExport {
    public enum ImportExportDialogType {
        Import,
        Export
    }
    public enum ImportExportDialogResult {
        Import,
        Cancel
    }
    public class ImportExportDialogViewModel : DialogControlBase<ImportExportDialogResult> {

        public ICommand ImportExportCommand { get; set; }
        public ICommand CancelCommand { get; set; }
        public LocaleKey ImportExportButtonContent { get; set; }
        public LocaleKey CancelButtonContent { get; }
        public ImportExportDialogType Type { get; set; }

        private string _companyString;
        public string CompanyString {
            get {
                return _companyString;
            }

            set {
                this._companyString = value;
                OnPropertyChanged(nameof(CompanyString));
            }
        }

        private ImportExportDialogViewModel(ImportExportDialogType type, LocaleKey title) {

            Title = title;

            // Define locales
            CancelButtonContent = new LocaleKey("ImportExportDialogView_Button_Cancel");

            CancelCommand = new RelayCommand<DialogWindow>(Cancel);
            ImportExportButtonContent = (type == ImportExportDialogType.Import) ? new LocaleKey("ImportExportDialogView_Button_Import") : new LocaleKey("ImportExportDialogView_Button_Copy");
            ImportExportCommand = (type == ImportExportDialogType.Import) ? new RelayCommand<DialogWindow>(Import) : new RelayCommand<DialogWindow>(Export);
            Type = (type == ImportExportDialogType.Import) ? ImportExportDialogType.Import : ImportExportDialogType.Export;
            DialogCloseDefault = ImportExportDialogResult.Cancel;

        }

        private ImportExportDialogViewModel(ImportExportDialogType type, LocaleKey title, string companyString) : this(type, title) { this.CompanyString = companyString; }

        public static ImportExportDialogResult ShowImportDialog(LocaleKey title, out string companyString) {
            var dialog = new ImportExportDialogViewModel(ImportExportDialogType.Import, title);
            var result = dialog.ShowDialog();
            companyString = dialog.CompanyString;
            return result;
        }
        public static ImportExportDialogResult ShowExportDialog(LocaleKey title, string companyString) {
            var dialog = new ImportExportDialogViewModel(ImportExportDialogType.Export, title, companyString);
            return dialog.ShowDialog();
        }

        private void Import(DialogWindow window) {

            CloseDialogWithResult(window, ImportExportDialogResult.Import);
        }

        private void Export(DialogWindow window) {

            Clipboard.SetData(DataFormats.Text, CompanyString);

        }

        private void Cancel(DialogWindow window) {

            CloseDialogWithResult(window, ImportExportDialogResult.Cancel);

        }
    }
}
