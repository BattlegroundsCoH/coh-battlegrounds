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
using BattlegroundsApp.Dialogs.RenameCompany;
using BattlegroundsApp.Dialogs.ImportExport;

namespace BattlegroundsApp.Views {
    /// <summary>
    /// Interaction logic for DivisionBuilderView.xaml
    /// </summary>
    public partial class CompanyBuilderView : ViewState, IStateMachine<ViewState> {

        public CompanyBuilderView() {
            InitializeComponent();
        }

        public ViewState State { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public StateChangeRequestHandler GetRequestHandler() => throw new NotImplementedException();

        public void SetState(ViewState state) => throw new NotImplementedException();

        // TODO
        public override void StateOnFocus() {

        }

        public override void StateOnLostFocus() => throw new NotImplementedException();

        bool IStateMachine<ViewState>.StateChangeRequest(object request) => throw new NotImplementedException();

        private void createCompany_Click(object sender, RoutedEventArgs e) {

            var result = CreateCompanyDialogViewModel.ShowCreateCompanyDialog("Create");

            if (result == CreateCompanyDialogResult.Create) {
                //Switch state here
            }

        }

        private void editCompany_Click(object sender, RoutedEventArgs e) {

        }

        private void renameCompany_Click(object sender, RoutedEventArgs e) {

        }

        private void deleteCompany_Click(object sender, RoutedEventArgs e) {

            var result = YesNoDialogViewModel.ShowYesNoDialog("Delete Company", "Are you sure?");

            if (result == YesNoDialogResult.Confirm) {
                //Detele selected company here
            }

        }

        private void copyCompany_Click(object sender, RoutedEventArgs e) {

        }

        private void exportCompany_Click(object sender, RoutedEventArgs e) {
            ImportExportDialogViewModel.ShowExportDialog("Export", "4444");
        }

        private void importCompany_Click(object sender, RoutedEventArgs e) {
            var result = ImportExportDialogViewModel.ShowImportDialog("Export");
            if (result == ImportExportDialogResult.Import) {
                //Import company here
            }
        }
    }
}
