using System.ComponentModel;
using System.Windows;

using Battlegrounds.Controls;
using Battlegrounds.Models;
using Battlegrounds.Services;
using Battlegrounds.ViewModels;

using Microsoft.Extensions.Logging;

namespace Battlegrounds.Views;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window {

    /// <summary>
    /// How much of the work area the window may occupy on first run.
    /// </summary>
    private const double FirstRunWorkAreaFraction = 0.9;

    /// <summary>
    /// How much of a restored window must land on-screen for the placement to be considered usable.
    /// Guards against a window saved on a monitor that is no longer connected.
    /// </summary>
    private const double MinVisibleWidth = 160;
    private const double MinVisibleHeight = 80;

    private readonly Configuration _configuration;
    private readonly BattlegroundsApp _app;
    private readonly IUiScaleService _uiScaleService;
    private readonly ILogger<MainWindow>? _logger;

    public MainWindow(MainWindowViewModel viewModel, Configuration configuration, BattlegroundsApp app, IUiScaleService uiScaleService, ILogger<MainWindow>? logger = null) {
        InitializeComponent();
        this.DataContext = viewModel;
        _configuration = configuration;
        _app = app;
        _uiScaleService = uiScaleService;
        _logger = logger;

        // Minimums come from scaled tokens, so they have to be re-read whenever the scale changes.
        ApplyWindowMinimums();
        _lastScaleFactor = _uiScaleService.CurrentFactor;
        _uiScaleService.ScaleChanged += OnScaleChanged;

        // Placement is applied before Show so that WindowStartupLocation="CenterScreen" — which reads the
        // final size when it centres — sees the size we actually want.
        ApplyPlacement();

        Closing += OnClosing;
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

    private void ApplyPlacement() {

        var saved = _configuration.WindowPlacement;
        if (saved.HasValue && TryClampToVirtualScreen(saved, out var bounds)) {
            WindowStartupLocation = WindowStartupLocation.Manual;
            Left = bounds.Left;
            Top = bounds.Top;
            Width = bounds.Width;
            Height = bounds.Height;
            if (saved.Maximized) {
                WindowState = WindowState.Maximized;
            }
            return;
        }

        // First run, or a saved placement that no longer lands on any connected display. Width/Height come
        // from Size.Window.Default* in XAML; 1720x980 overflows a 1080p screen at 125% scaling, where the
        // usable viewport is only ~1536x864 DIPs, so clamp to the work area.
        var work = SystemParameters.WorkArea;
        _logger?.LogInformation("Default placement: XAML size {Width}x{Height}, work area {WorkWidth}x{WorkHeight}, minimums {MinWidth}x{MinHeight}.",
            Width, Height, work.Width, work.Height, MinWidth, MinHeight);
        Width = Math.Max(MinWidth, Math.Min(Width, work.Width * FirstRunWorkAreaFraction));
        Height = Math.Max(MinHeight, Math.Min(Height, work.Height * FirstRunWorkAreaFraction));
    }

    /// <summary>
    /// Fits a saved placement onto the current virtual screen, returning false if it cannot be made visible.
    /// </summary>
    private bool TryClampToVirtualScreen(Configuration.WindowPlacementConfiguration saved, out Rect bounds) {

        bounds = Rect.Empty;

        // HasValue has already established these are non-null and finite.
        double savedLeft = saved.Left!.Value;
        double savedTop = saved.Top!.Value;

        double screenLeft = SystemParameters.VirtualScreenLeft;
        double screenTop = SystemParameters.VirtualScreenTop;
        double screenRight = screenLeft + SystemParameters.VirtualScreenWidth;
        double screenBottom = screenTop + SystemParameters.VirtualScreenHeight;

        double width = Math.Clamp(saved.Width!.Value, MinWidth, Math.Max(MinWidth, SystemParameters.VirtualScreenWidth));
        double height = Math.Clamp(saved.Height!.Value, MinHeight, Math.Max(MinHeight, SystemParameters.VirtualScreenHeight));

        // Reject rather than nudge when barely any of the window would be reachable — the user's monitor
        // layout has changed enough that centring on the primary display is the friendlier answer.
        bool horizontallyVisible = savedLeft + width > screenLeft + MinVisibleWidth && savedLeft < screenRight - MinVisibleWidth;
        bool verticallyVisible = savedTop + height > screenTop && savedTop < screenBottom - MinVisibleHeight;
        if (!horizontallyVisible || !verticallyVisible) {
            return false;
        }

        // Pull a partially off-screen window fully into view where that is possible.
        double left = Math.Clamp(savedLeft, screenLeft, Math.Max(screenLeft, screenRight - width));
        double top = Math.Clamp(savedTop, screenTop, Math.Max(screenTop, screenBottom - height));

        bounds = new Rect(left, top, width, height);
        return true;
    }

    private void OnClosing(object? sender, CancelEventArgs e) {

        // RestoreBounds is the normal-state rect, which is what we want to reopen at when the user is
        // currently maximized or minimized. It is only meaningful once the window has been shown.
        var bounds = WindowState == WindowState.Normal
            ? new Rect(Left, Top, Width, Height)
            : RestoreBounds;

        // Never persist a non-finite rect: System.Text.Json throws on NaN/Infinity, which would take
        // config saving down with it.
        if (bounds.IsEmpty || !IsFinite(bounds.Left) || !IsFinite(bounds.Top)
            || !IsFinite(bounds.Width) || !IsFinite(bounds.Height)) {
            return;
        }

        var placement = _configuration.WindowPlacement;
        placement.Left = bounds.Left;
        placement.Top = bounds.Top;
        placement.Width = bounds.Width;
        placement.Height = bounds.Height;
        placement.Maximized = WindowState == WindowState.Maximized;

        try {
            _app.SaveConfiguration();
        } catch (Exception) {
            // Losing the window position is not worth blocking shutdown over.
        }
    }

    private static bool IsFinite(double value) => !double.IsNaN(value) && !double.IsInfinity(value);

}
