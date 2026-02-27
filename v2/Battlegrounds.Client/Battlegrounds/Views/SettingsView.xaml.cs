using System.Windows.Controls;

using Battlegrounds.ViewModels;

namespace Battlegrounds.Views;

public partial class SettingsView : UserControl {

    public SettingsView(SettingsViewModel viewModel) {
        InitializeComponent();
        DataContext = viewModel;
    }

}
