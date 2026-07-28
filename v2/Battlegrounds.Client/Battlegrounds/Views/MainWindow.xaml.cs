using System.Windows;

using Battlegrounds.Controls;
using Battlegrounds.Services;
using Battlegrounds.ViewModels;

namespace Battlegrounds.Views;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window {

    private readonly IUiScaleService _uiScaleService;

    public MainWindow(MainWindowViewModel viewModel, IUiScaleService uiScaleService) {
        InitializeComponent();
        this.DataContext = viewModel;
        _uiScaleService = uiScaleService;

        // Minimums come from scaled tokens, so they have to be re-read whenever the scale changes.
        ApplyWindowMinimums();
        _lastScaleFactor = _uiScaleService.CurrentFactor;
        _uiScaleService.ScaleChanged += OnScaleChanged;
    }

    /// <summary>
    /// Tracks the scale the current window size corresponds to, so a change can resize proportionally.
    /// </summary>
    private double _lastScaleFactor = 1.0;

    private void OnScaleChanged(object? sender, EventArgs e) {

        ApplyWindowMinimums();

        // Grow the window with the scale. Without this, raising the scale makes everything bigger inside
        // a window that stayed the same size, so the user sees *less* content — the opposite of the
        // intent. A maximized window is already as big as it gets, so leave it alone.
        double factor = _uiScaleService.CurrentFactor;
        if (WindowState == WindowState.Normal && factor > 0 && _lastScaleFactor > 0
            && IsFinite(Width) && IsFinite(Height)) {

            double ratio = factor / _lastScaleFactor;
            var work = SystemParameters.WorkArea;

            // MinWidth/MinHeight were just capped to the work area, so these bounds cannot invert.
            Width = Math.Clamp(Width * ratio, MinWidth, work.Width);
            Height = Math.Clamp(Height * ratio, MinHeight, work.Height);
        }

        _lastScaleFactor = factor;

        // Size.Breakpoint.* moved with the scale. A window that did not resize — a maximized one —
        // would otherwise keep reporting the band it was in under the old thresholds.
        Responsive.Reevaluate();
    }

    /// <summary>
    /// Reads the scaled window minimums and applies them, capped to what the display can actually show.
    /// </summary>
    /// <remarks>Set in code rather than bound in XAML because the cap has no resource to come from:
    /// Size.Window.Min* reaches 1770x1080 at 150%, which does not fit on a 1080p screen. Without the cap
    /// a user at a high scale on a small display would get a window they cannot fit or resize.</remarks>
    private void ApplyWindowMinimums() {

        var work = SystemParameters.WorkArea;

        MinWidth = Math.Min(ReadSizeToken("Size.Window.MinWidth", 1180), work.Width);
        MinHeight = Math.Min(ReadSizeToken("Size.Window.MinHeight", 720), work.Height);
    }

    private double ReadSizeToken(string key, double fallback)
        => TryFindResource(key) is double value && value > 0 ? value : fallback;

    private static bool IsFinite(double value) => !double.IsNaN(value) && !double.IsInfinity(value);

}
