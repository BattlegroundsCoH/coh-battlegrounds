using System.Windows;

using Battlegrounds.ViewModels;

namespace Battlegrounds.Views;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window {
    
    public MainWindow(MainWindowViewModel viewModel) {
        InitializeComponent();
        this.DataContext = viewModel;

    }

}