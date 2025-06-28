using System.Windows.Controls;
using System.Windows.Input;

using Battlegrounds.ViewModels;

namespace Battlegrounds.Views;

/// <summary>
/// Interaction logic for LoginView.xaml
/// </summary>
public partial class LoginView : UserControl {

    public LoginView() {
        InitializeComponent();
    }

    private void RegisterNow_MouseDown(object sender, MouseButtonEventArgs e) {
        if (DataContext is not LoginViewModel viewModel) {
            return;
        }
        viewModel.RedirectRegisterNow();
    }

}
