using System.Windows;

namespace Battlegrounds.Controls;

/// <summary>
/// How much horizontal room the layout has to work with.
/// </summary>
public enum Breakpoint {

    /// <summary>Narrower than <c>Size.Breakpoint.Medium</c>. Drop to secondary content.</summary>
    Compact,

    /// <summary>The size the app is designed around.</summary>
    Medium,

    /// <summary>At or above <c>Size.Breakpoint.Expanded</c>. Room for more per row.</summary>
    Expanded,

}

/// <summary>
/// Publishes the current width <see cref="Breakpoint"/> down the visual tree so views can adapt
/// their layout with ordinary <c>DataTrigger</c>s.
/// </summary>
/// <remarks>
/// <para>
/// WPF has no <c>AdaptiveTrigger</c>. This is the equivalent, built the way the rest of this
/// codebase does conditional visuals: an inheriting attached property plus triggers, rather than
/// a <see cref="System.Windows.VisualStateManager"/> (of which there are none here).
/// </para>
/// <para>
/// Set <c>Responsive.IsBreakpointHost="True"</c> on the window. Any descendant can then trigger on
/// <c>Responsive.Breakpoint</c> because the property inherits — the same mechanism
/// <c>TrackedTextBlock.Tracking</c> uses to reach a caption generated inside a template.
/// </para>
/// <para>
/// Thresholds come from <c>Size.Breakpoint.*</c> in Metrics.xaml and are scaled by the UI-scale
/// overlays, so a cramped window at 150% is not misreported as Expanded. Because those values can
/// change without the window resizing (a scale change while maximized), MainWindow calls
/// <see cref="Reevaluate"/> when the scale changes.
/// </para>
/// <para>
/// <b>Constraint:</b> <see cref="TrackedTextBlock"/> cannot wrap or trim, and every heading, label
/// and button caption is one. Adaptations must hide, shorten or restack — never assume tracked
/// text will reflow into a narrower space.
/// </para>
/// </remarks>
public static class Responsive {

    /// <summary>Fallbacks used when the size tokens cannot be resolved (designer, bare tests).</summary>
    private const double DefaultMediumThreshold = 1400;
    private const double DefaultExpandedThreshold = 1800;

    /// <summary>
    /// Live hosts, so a threshold change can be reapplied without a resize. Weak so that a closed
    /// window is still collectable.
    /// </summary>
    private static readonly List<WeakReference<FrameworkElement>> Hosts = [];

    /// <summary>
    /// The width band the element sits in. Inherits, so set it once on the host and read it anywhere.
    /// </summary>
    public static readonly DependencyProperty BreakpointProperty =
        DependencyProperty.RegisterAttached(
            "Breakpoint", typeof(Breakpoint), typeof(Responsive),
            new FrameworkPropertyMetadata(Breakpoint.Medium, FrameworkPropertyMetadataOptions.Inherits));

    public static Breakpoint GetBreakpoint(DependencyObject element) => (Breakpoint)element.GetValue(BreakpointProperty);

    public static void SetBreakpoint(DependencyObject element, Breakpoint value) => element.SetValue(BreakpointProperty, value);

    /// <summary>
    /// Opts an element in to measuring itself and publishing <see cref="BreakpointProperty"/>.
    /// </summary>
    public static readonly DependencyProperty IsBreakpointHostProperty =
        DependencyProperty.RegisterAttached(
            "IsBreakpointHost", typeof(bool), typeof(Responsive),
            new FrameworkPropertyMetadata(false, OnIsBreakpointHostChanged));

    public static bool GetIsBreakpointHost(DependencyObject element) => (bool)element.GetValue(IsBreakpointHostProperty);

    public static void SetIsBreakpointHost(DependencyObject element, bool value) => element.SetValue(IsBreakpointHostProperty, value);

    private static void OnIsBreakpointHostChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {

        if (d is not FrameworkElement element) {
            return;
        }

        if (e.NewValue is true) {
            element.SizeChanged += OnHostSizeChanged;
            element.Loaded += OnHostLoaded;
            Hosts.Add(new WeakReference<FrameworkElement>(element));
            Apply(element);
        } else {
            element.SizeChanged -= OnHostSizeChanged;
            element.Loaded -= OnHostLoaded;
            Hosts.RemoveAll(w => !w.TryGetTarget(out var target) || ReferenceEquals(target, element));
        }
    }

    private static void OnHostLoaded(object sender, RoutedEventArgs e) {
        // Resources resolve reliably only once the element is in a tree, so the value set when the
        // property was first attached may have used the fallbacks.
        if (sender is FrameworkElement element) {
            Apply(element);
        }
    }

    private static void OnHostSizeChanged(object sender, SizeChangedEventArgs e) {
        if (e.WidthChanged && sender is FrameworkElement element) {
            Apply(element);
        }
    }

    /// <summary>
    /// Recomputes every live host. Call after something other than a resize changes the outcome —
    /// in practice, a UI-scale change, which moves the thresholds.
    /// </summary>
    public static void Reevaluate() {

        for (int i = Hosts.Count - 1; i >= 0; i--) {
            if (Hosts[i].TryGetTarget(out var element)) {
                Apply(element);
            } else {
                Hosts.RemoveAt(i);
            }
        }
    }

    private static void Apply(FrameworkElement element) {

        double width = element.ActualWidth;
        if (width <= 0) {
            return; // Not laid out yet; Loaded or the first SizeChanged will catch it.
        }

        double medium = ReadThreshold(element, "Size.Breakpoint.Medium", DefaultMediumThreshold);
        double expanded = ReadThreshold(element, "Size.Breakpoint.Expanded", DefaultExpandedThreshold);

        var breakpoint = width >= expanded ? Breakpoint.Expanded
            : width >= medium ? Breakpoint.Medium
            : Breakpoint.Compact;

        if (GetBreakpoint(element) != breakpoint) {
            SetBreakpoint(element, breakpoint);
        }
    }

    private static double ReadThreshold(FrameworkElement element, string key, double fallback)
        => element.TryFindResource(key) is double value && value > 0 ? value : fallback;

}
