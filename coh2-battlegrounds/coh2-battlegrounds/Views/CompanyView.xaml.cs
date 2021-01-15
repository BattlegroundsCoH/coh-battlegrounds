using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using BattlegroundsApp.Dialogs.CreateCompany;
using BattlegroundsApp.Dialogs.Service;
using BattlegroundsApp.Dialogs.YesNo;
using BattlegroundsApp.Dialogs.ImportExport;
using BattlegroundsApp.Dialogs.RenameCopyDialog;
using BattlegroundsApp.LocalData;
using Battlegrounds.Game.DataCompany;
using Battlegrounds.Verification;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Battlegrounds.Game.Gameplay;

namespace BattlegroundsApp.Views {
    /// <summary>
    /// Interaction logic for DivisionBuilderView.xaml
    /// </summary>
    public partial class CompanyView : ViewState, IStateMachine<ViewState> {

        private ObservableCollection<Company>  m_player_companies;

        public CompanyView() {
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

        public override void StateOnLostFocus() {}

        bool IStateMachine<ViewState>.StateChangeRequest(object request) => true;

        private void createCompany_Click(object sender, RoutedEventArgs e) {

            var result = CreateCompanyDialogViewModel.ShowCreateCompanyDialog("Create", out string companyName, out Faction companyFaction, out CompanyType companyType);

            if (result == CreateCompanyDialogResult.Create) {
                this.StateChangeRequest(new CompanyBuilderView(companyName, companyFaction, companyType));
            }

        }

        private void editCompany_Click(object sender, RoutedEventArgs e) {
            this.StateChangeRequest(new CompanyBuilderView(companyList.SelectedItem as Company));
        }

        private void renameCompany_Click(object sender, RoutedEventArgs e) {
            var result = RenameCopyDialogViewModel.ShowRenameDialog("Rename", out string companyName);

            if (result == RenameCopyDialogResult.Rename) {
                var builder = new CompanyBuilder();
                var company = builder.CloneCompany(companyList.SelectedItem as Company, companyName, CompanyAvailabilityType.MultiplayerOnly).Commit().Result;
                PlayerCompanies.DeleteCompany(companyList.SelectedItem as Company);
                PlayerCompanies.SaveCompany(company);
                UpdateCompanyList();
            }

        }

        private void deleteCompany_Click(object sender, RoutedEventArgs e) {

            var result = YesNoDialogViewModel.ShowYesNoDialog("Delete Company", "Are you sure?");

            if (result == YesNoDialogResult.Confirm) {
                PlayerCompanies.DeleteCompany(companyList.SelectedItem as Company);
                UpdateCompanyList();
            }

        }

        private void copyCompany_Click(object sender, RoutedEventArgs e) {
            var result = RenameCopyDialogViewModel.ShowCopyDialog("Copy", out string companyName);

            if (result == RenameCopyDialogResult.Copy) {
                var builder = new CompanyBuilder();
                builder.CloneCompany(companyList.SelectedItem as Company, companyName, CompanyAvailabilityType.MultiplayerOnly).Commit();
                PlayerCompanies.SaveCompany(builder.Result);
                UpdateCompanyList();
            }
        }

        private void exportCompany_Click(object sender, RoutedEventArgs e) 
            => ImportExportDialogViewModel.ShowExportDialog("Export", (CompanyTemplate.FromCompany(companyList.SelectedItem as Company)).ToTemplateString());

        private void importCompany_Click(object sender, RoutedEventArgs e) {
            var result = ImportExportDialogViewModel.ShowImportDialog("Export", out string companyString);
            if (result == ImportExportDialogResult.Import) {
                try {
                    var company = CompanyTemplate.FromString(companyString);
                    PlayerCompanies.SaveCompany(CompanyTemplate.FromTemplate(company));
                    UpdateCompanyList();
                } catch(FormatException err) {
                    Trace.WriteLine(err); // TODO: Show message box featuring a detailed description of the problem 
                    // The causing error will be visible in err (You may need it when deciding on what to do from there).
                }
            }
        }

        private void UpdateCompanyList() {
            PlayerCompanies.LoadAll();
            m_player_companies.Clear();
            foreach (var company in PlayerCompanies.GetAllCompanyes()) {
                m_player_companies.Add(company);
            }
        }

    }

}
