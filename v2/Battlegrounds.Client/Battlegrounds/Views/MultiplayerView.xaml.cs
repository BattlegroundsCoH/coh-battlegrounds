using System.Windows.Controls;

using Battlegrounds.ViewModels;

namespace Battlegrounds.Views;

/// <summary>
/// Interaction logic for MultiplayerView.xaml
/// </summary>
public partial class MultiplayerView : UserControl {
    
    public MultiplayerView(MultiplayerViewModel viewModel) {
        InitializeComponent();
        this.DataContext = viewModel;
    }

}
