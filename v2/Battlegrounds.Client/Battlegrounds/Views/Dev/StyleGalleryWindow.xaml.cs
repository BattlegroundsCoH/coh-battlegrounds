using System.Windows;
using System.Windows.Controls;

using Battlegrounds.Services;
using Battlegrounds.Services.Infrastructure;

namespace Battlegrounds.Views.Dev;

/// <summary>
/// Interaction logic for StyleGalleryWindow.xaml
/// </summary>
public partial class StyleGalleryWindow : Window {

    /// <summary>
    /// The gallery runs before DI is built (App.OnStartup short-circuits on --gallery), so it owns
    /// its own scale service rather than resolving one.
    /// </summary>
    private static readonly IUiScaleService ScaleService = new UiScaleService();

    public StyleGalleryWindow() {
        InitializeComponent();
        CurrentScaleLabel.Text = $"SHOWING {ScaleService.CurrentScale}";
    }

    private void OnScaleClicked(object sender, RoutedEventArgs e) {

        if (sender is not Button { Tag: string scale }) {
            return;
        }
        if (string.Equals(scale, ScaleService.CurrentScale, StringComparison.OrdinalIgnoreCase)) {
            return;
        }

        ScaleService.Apply(scale);

        // Reopen rather than mutate: this window references its tokens with {StaticResource}, which
        // resolved when it was parsed and will not pick the overlay up.
        var replacement = new StyleGalleryWindow {
            Left = Left,
            Top = Top,
            Width = Width,
            Height = Height,
            WindowStartupLocation = WindowStartupLocation.Manual,
            WindowState = WindowState,
        };
        replacement.Show();
        Close();
    }

}
