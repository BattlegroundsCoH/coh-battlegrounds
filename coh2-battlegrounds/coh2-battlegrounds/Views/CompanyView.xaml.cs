using System;
using System.Windows;
using System.Collections.ObjectModel;
using System.Diagnostics;

using BattlegroundsApp.Dialogs.CreateCompany;
using BattlegroundsApp.Dialogs.YesNo;
using BattlegroundsApp.Dialogs.ImportExport;
using BattlegroundsApp.Dialogs.RenameCopyDialog;
using BattlegroundsApp.LocalData;

using Battlegrounds.Game.DataCompany;
using Battlegrounds.Game.Gameplay;
using Battlegrounds.Modding;
using Battlegrounds.Locale;

namespace BattlegroundsApp.Views {
    /// <summary>
    /// Interaction logic for DivisionBuilderView.xaml
    /// </summary>
    public partial class CompanyView : ViewState, IStateMachine<ViewState> {

        private ObservableCollection<Company> m_player_companies;

        public LocaleKey CreateButtonContent { get; }
        public LocaleKey EditButtonContent { get; }
        public LocaleKey RenameButtonContent { get; }
        public LocaleKey DeleteButtonContent { get; }
        public LocaleKey CopyButtonContent { get; }
        public LocaleKey ExportButtonContent { get; }
        public LocaleKey ImportButtonContent { get; }
        public LocaleKey NameListWiewHeader { get; }
        public LocaleKey RatingListWiewHeader { get; }
        public LocaleKey TypeListWiewHeader { get; }
        public LocaleKey AllianceListWiewHeader { get; }
        public LocaleKey CountryListWiewHeader { get; }

        public CompanyView() {

            this.DataContext = this;

            // Define locales
            CreateButtonContent = new LocaleKey("CompanyView_Create");
            EditButtonContent = new LocaleKey("CompanyView_Edit");
            RenameButtonContent = new LocaleKey("CompanyView_Rename");
            DeleteButtonContent = new LocaleKey("CompanyView_Delete");
            CopyButtonContent = new LocaleKey("CompanyView_Copy");
            ExportButtonContent = new LocaleKey("CompanyView_Export");
            ImportButtonContent = new LocaleKey("CompanyView_Import");
            NameListWiewHeader = new LocaleKey("CompanyView_Name");
            RatingListWiewHeader = new LocaleKey("CompanyView_Rating");
            TypeListWiewHeader = new LocaleKey("CompanyView_Type");
            AllianceListWiewHeader = new LocaleKey("CompanyView_Alliance");
            CountryListWiewHeader = new LocaleKey("CompanyView_Country");

            InitializeComponent();

            m_player_companies = new ObservableCollection<Company>();

            companyList.ItemsSource = m_player_companies;

        }

        public ViewState State { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public StateChangeRequestHandler GetRequestHandler() => throw new NotImplementedException();

        public void SetState(ViewState state) => throw new NotImplementedException();

        public override void StateOnFocus() {

            UpdateCompanyList();

        }

        public override void StateOnLostFocus() { }

        bool IStateMachine<ViewState>.StateChangeRequest(object request) => true;

        private void createCompany_Click(object sender, RoutedEventArgs e) {

            ModGuid modGuid = ModManager.GetPackage("mod_bg").TuningGUID;
            Trace.TraceWarning("There is currently no method of setting tuning pack. This should be fixed ASAP.");

            if (CreateCompanyDialogViewModel.ShowCreateCompanyDialog(new LocaleKey("CompanyView_CreateCompanyDialog_Title"), out string companyName, out Faction companyFaction, out CompanyType companyType)
                is CreateCompanyDialogResult.Create) {
                //this.StateChangeRequest(new CompanyBuilderView(companyName, companyFaction, companyType, modGuid));
            }

        }

        private void editCompany_Click(object sender, RoutedEventArgs e) {
            //this.StateChangeRequest(new CompanyBuilderView(companyList.SelectedItem as Company));
        }

        private void renameCompany_Click(object sender, RoutedEventArgs e) {
            var result = RenameCopyDialogViewModel.ShowRenameDialog(new LocaleKey("CompanyView_RenameCopyDialog_Rename_Title"), out string companyName);

            if (result == RenameCopyDialogResult.Rename) {
                var builder = new CompanyBuilder();
                var company = builder.CloneCompany(companyList.SelectedItem as Company, companyName, CompanyAvailabilityType.MultiplayerOnly).Commit().Result;
                PlayerCompanies.DeleteCompany(companyList.SelectedItem as Company);
                PlayerCompanies.SaveCompany(company);
                UpdateCompanyList();
            }

        }

        private void deleteCompany_Click(object sender, RoutedEventArgs e) {

            var result = YesNoDialogViewModel.ShowYesNoDialog(new LocaleKey("CompanyView_YesNoDialog_Delete_Company_Title"), new LocaleKey("CompanyView_YesNoDialog_Delete_Company_Message"));

            if (result == YesNoDialogResult.Confirm) {
                PlayerCompanies.DeleteCompany(companyList.SelectedItem as Company);
                UpdateCompanyList();
            }

        }

        private void copyCompany_Click(object sender, RoutedEventArgs e) {
            var result = RenameCopyDialogViewModel.ShowCopyDialog(new LocaleKey("CompanyView_RenameCopyDialog_Copy_Title"), out string companyName);

            if (result == RenameCopyDialogResult.Copy) {
                var builder = new CompanyBuilder();
                builder.CloneCompany(companyList.SelectedItem as Company, companyName, CompanyAvailabilityType.MultiplayerOnly).Commit();
                PlayerCompanies.SaveCompany(builder.Result);
                UpdateCompanyList();
            }
        }

        private void exportCompany_Click(object sender, RoutedEventArgs e)
            => ImportExportDialogViewModel.ShowExportDialog(new LocaleKey("CompanyView_ImportExportDialog_Export_Title"), (CompanyTemplate.FromCompany(companyList.SelectedItem as Company)).ToTemplateString());

        private void importCompany_Click(object sender, RoutedEventArgs e) {
            var result = ImportExportDialogViewModel.ShowImportDialog(new LocaleKey("CompanyView_ImportExportDialog_Import_Title"), out string companyString);
            if (result == ImportExportDialogResult.Import) {
                try {
                    var company = CompanyTemplate.FromString(companyString);
                    PlayerCompanies.SaveCompany(CompanyTemplate.FromTemplate(company));
                    UpdateCompanyList();
                } catch (FormatException err) {
                    Trace.WriteLine(err); // TODO: Show message box featuring a detailed description of the problem 
                    // The causing error will be visible in err (You may need it when deciding on what to do from there).
                }
            }
        }

        private void UpdateCompanyList() {
            PlayerCompanies.LoadAll();
            m_player_companies.Clear();
            foreach (var company in PlayerCompanies.GetAllCompanies()) {
                m_player_companies.Add(company);
            }
        }

    }

}
