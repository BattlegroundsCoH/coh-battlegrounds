using System.Windows;
using System.Windows.Controls;

using Battlegrounds.Services;
using Battlegrounds.Services.Infrastructure;

using Microsoft.Extensions.Logging;

namespace Battlegrounds.Views.Dev;

/// <summary>
/// Interaction logic for StyleGalleryWindow.xaml
/// </summary>
public partial class StyleGalleryWindow : Window {

    /// <summary>
    /// The gallery runs before DI is built (App.OnStartup short-circuits on --gallery), so it owns
    /// its own scale service rather than resolving one. Static because switching scale reopens the
    /// window: the selection has to outlive the instance that made it.
    /// </summary>
    private static IUiScaleService? _scaleService;

    private readonly ILoggerFactory _loggerFactory;

    public StyleGalleryWindow(ILoggerFactory loggerFactory) {
        _loggerFactory = loggerFactory;
        _scaleService ??= new UiScaleService(loggerFactory.CreateLogger<UiScaleService>());
        InitializeComponent();
        CurrentScaleLabel.Text = $"SHOWING {_scaleService.CurrentScale}";
    }

    private void OnScaleClicked(object sender, RoutedEventArgs e) {

        if (sender is not Button { Tag: string scale }) {
            return;
        }
        if (string.Equals(scale, _scaleService!.CurrentScale, StringComparison.OrdinalIgnoreCase)) {
            return;
        }

        _scaleService.Apply(scale);

        // Reopen rather than mutate: this window references its tokens with {StaticResource}, which
        // resolved when it was parsed and will not pick the overlay up.
        var replacement = new StyleGalleryWindow(_loggerFactory) {
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
