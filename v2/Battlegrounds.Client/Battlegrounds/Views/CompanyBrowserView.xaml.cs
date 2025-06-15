using System.Windows.Controls;

using Battlegrounds.ViewModels;

namespace Battlegrounds.Views;

/// <summary>
/// Interaction logic for CompanyBrowserView.xaml
/// </summary>
public partial class CompanyBrowserView : UserControl {

    public CompanyBrowserView(CompanyBrowserViewModel companyBrowserViewModel) {
        InitializeComponent();
        DataContext = companyBrowserViewModel;
    }

    private void CompanySelectionTreeView_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e) {
        if (DataContext is CompanyBrowserViewModel vm) {
            var selectedItem = CompanySelectionTreeView.SelectedItem;
            if (selectedItem is not null) {
                vm.EditCompanyCommand.Execute(selectedItem);
            }
        }
    }

}
