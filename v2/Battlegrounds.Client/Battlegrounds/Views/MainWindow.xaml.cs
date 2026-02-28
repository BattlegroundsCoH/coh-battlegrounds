using System.Windows;
using System.Windows.Controls.Primitives;

using Battlegrounds.ViewModels;

namespace Battlegrounds.Views;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window {

    public MainWindow(MainWindowViewModel viewModel) {
        InitializeComponent();
        this.DataContext = viewModel;
        ProfilePopup.CustomPopupPlacementCallback = PlaceProfilePopup;
    }

    private static CustomPopupPlacement[] PlaceProfilePopup(Size popupSize, Size targetSize, Point offset) {
        // Right-align the popup with the right edge of the toggle button so it never overflows the window.
        var x = targetSize.Width - popupSize.Width;
        var y = targetSize.Height + 4; // 4 px gap below the button
        return [new CustomPopupPlacement(new Point(x, y), PopupPrimaryAxis.Vertical)];
    }

    private void LogOutButton_Click(object sender, RoutedEventArgs e) {
        ProfileToggle.IsChecked = false;
    }

    private void SettingsButton_Click(object sender, RoutedEventArgs e) {
        ProfileToggle.IsChecked = false;
    }

}