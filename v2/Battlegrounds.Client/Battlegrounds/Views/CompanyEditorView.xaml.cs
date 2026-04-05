using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using Battlegrounds.ViewModels;

namespace Battlegrounds.Views;

/// <summary>
/// Interaction logic for CompanyEditorView.xaml
/// </summary>
public partial class CompanyEditorView : UserControl {
    public CompanyEditorView(CompanyEditorViewModel companyEditorViewModel) {
        InitializeComponent();
        DataContext = companyEditorViewModel;
    }

    private void CompanyNameTextBox_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e) {
        if (sender is TextBox tb && (bool)e.NewValue) {
            tb.SelectAll();
            tb.Focus();
        }
    }

    private void CompanyNameTextBox_KeyDown(object sender, KeyEventArgs e) {
        if (DataContext is not CompanyEditorViewModel vm) return;
        if (e.Key == Key.Enter) {
            vm.CommitRenameCommand.Execute(null);
            e.Handled = true;
        } else if (e.Key == Key.Escape) {
            vm.CancelRenameCommand.Execute(null);
            e.Handled = true;
        }
    }

    private void CompanyNameTextBox_LostFocus(object sender, RoutedEventArgs e) {
        if (DataContext is CompanyEditorViewModel vm) {
            vm.CommitRenameCommand.Execute(null);
        }
    }
}
