using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

using Battlegrounds.Game.DataCompany;
using Battlegrounds.Game.Gameplay;
using Battlegrounds.Locale;
using BattlegroundsApp.Dialogs.Service;
using BattlegroundsApp.Utilities;
using BattlegroundsApp.ViewModels;

namespace BattlegroundsApp.Dialogs.CreateCompany {
    public enum CreateCompanyDialogResult {
        Create,
        Cancel
    }
    class CreateCompanyDialogViewModel : DialogControlBase<CreateCompanyDialogResult> {

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

        private Faction _companyFaction = Faction.Soviet;
        public Faction CompanyFaction {
            get {
                return this._companyFaction;
            }

            set {
                this._companyFaction = value;
                OnPropertyChanged(nameof(CompanyFaction));
            }
        }

        private CompanyType _companyType = CompanyType.Unspecified;
        public CompanyType CompanyType {
            get {
                return this._companyType;
            }

            set {
                this._companyType = value;
                OnPropertyChanged(nameof(CompanyType));
            }
        }

        public LocaleKey NameTextBlockText { get; }
        public LocaleKey FactionTextBlockText { get; }
        public LocaleKey TypeTextBlockText { get; }
        public LocaleKey CreateButtonContent {  get; }
        public LocaleKey CancelButtonContent {  get; }

        private CreateCompanyDialogViewModel(LocaleKey title) {

            Title = title;

            // Define locales
            NameTextBlockText = new LocaleKey("CreateCompanyDialogView_Name");
            FactionTextBlockText = new LocaleKey("CreateCompanyDialogView_Faction");
            TypeTextBlockText = new LocaleKey("CreateCompanyDialogView_Type");
            CreateButtonContent = new LocaleKey("CreateCompanyDialogView_Button_Create");
            CancelButtonContent = new LocaleKey("CreateCompanyDialogView_Button_Cancel");

            CreateCommand = new RelayCommand<DialogWindow>(Create);
            CancelCommand = new RelayCommand<DialogWindow>(Cancel);
            DialogCloseDefault = CreateCompanyDialogResult.Cancel;

        }

        public static CreateCompanyDialogResult ShowCreateCompanyDialog(LocaleKey title, out string companyName, out Faction faction, out CompanyType type) {
            var dialog = new CreateCompanyDialogViewModel(title);
            var result = dialog.ShowDialog();
            companyName = dialog.CompanyName;
            faction = dialog.CompanyFaction;
            type = dialog.CompanyType;
            return result;
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
