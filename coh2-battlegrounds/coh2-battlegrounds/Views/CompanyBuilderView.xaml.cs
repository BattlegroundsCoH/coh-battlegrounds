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

namespace BattlegroundsApp.Views {
    /// <summary>
    /// Interaction logic for DivisionBuilderView.xaml
    /// </summary>
    public partial class CompanyBuilderView : ViewState, IStateMachine<ViewState> {

        private IDialogService _dialogService;

        public CompanyBuilderView() {
            InitializeComponent();
            _dialogService = new DialogService();
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

            var dialog = new CreateCompanyDialogViewModel("Create Company");
            var result = _dialogService.OpenDialog(dialog);

            if (result == CreateCompanyDialogResult.Create) {
                //Switch state here
            }

        }

        private void editCompany_Click(object sender, RoutedEventArgs e) {

        }

        private void renameCompany_Click(object sender, RoutedEventArgs e) {

            var dialog = new RenameCompanyDialogViewModel("Rename Company");
            var result = _dialogService.OpenDialog(dialog);

            if (result == RenameCompanyDialogResult.Rename) {
                //Rename selected company here
            }

        }

        private void deleteCompany_Click(object sender, RoutedEventArgs e) {

            var dialog = new YesNoDialogViewModel("Delete Company", "Are you sure?");
            var result = _dialogService.OpenDialog(dialog);

            if (result == YesNoDialogResult.Confirm) {
                //Detele selected company here
            }

        }

        private void copyCompany_Click(object sender, RoutedEventArgs e) {

        }

        private void exportCompany_Click(object sender, RoutedEventArgs e) {

        }

        private void importCompany_Click(object sender, RoutedEventArgs e) {

        }
    }
}
