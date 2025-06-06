using System.Windows.Controls;

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
}
