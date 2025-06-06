using System.Windows;
using System.Windows.Controls;

using Battlegrounds.Models.Companies;
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

    private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e) {
        if (this.DataContext is CompanyBrowserViewModel viewModel) {
            viewModel.SelectedCompany = e.NewValue as Company;
        }
    }

    private void TreeView_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e) {
        if (this.DataContext is CompanyBrowserViewModel viewModel && viewModel.SelectedCompany is not null) {
            viewModel.EditCompany(viewModel.SelectedCompany);   
        }
    }

}
