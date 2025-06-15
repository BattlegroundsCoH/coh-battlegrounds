using Battlegrounds.Helpers;
using Battlegrounds.ViewModels.Modals;

namespace Battlegrounds.Views.Modals;

/// <summary>
/// Interaction logic for CreateLobbyModalView.xaml
/// </summary>
public partial class CreateLobbyModalView : DialogUserControl {
    
    public CreateLobbyModalView(CreateLobbyModalViewModel viewModel) : base(viewModel) {
        InitializeComponent();
    }

    private void PasswordBox_PasswordChanged(object sender, System.Windows.RoutedEventArgs e) {
        if (DataContext is CreateLobbyModalViewModel viewModel) {
            viewModel.LobbyPassword = PasswordBox.Password;
        }
    }

}
