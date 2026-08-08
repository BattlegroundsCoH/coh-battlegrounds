using System.Windows;
using System.Windows.Controls;

namespace Battlegrounds.Behaviors;

/// <summary>
/// Keeps a <see cref="ScrollViewer"/> pinned to the bottom as content is appended.
/// </summary>
/// <remarks>
/// Sticky rather than unconditional: the viewer only follows new content while the user is
/// already at the bottom. Scrolling up to read back stops it, and returning to the bottom
/// resumes it. An unconditional ScrollToEnd would yank the log away mid-read.
/// </remarks>
public static class AutoScrollBehavior {

    public static readonly DependencyProperty ToEndProperty = DependencyProperty.RegisterAttached(
        "ToEnd",
        typeof(bool),
        typeof(AutoScrollBehavior),
        new PropertyMetadata(false, OnToEndChanged));

    /// <summary>Whether the viewer is currently following the bottom of the content.</summary>
    private static readonly DependencyProperty IsPinnedProperty = DependencyProperty.RegisterAttached(
        "IsPinned",
        typeof(bool),
        typeof(AutoScrollBehavior),
        new PropertyMetadata(true));

    public static void SetToEnd(DependencyObject element, bool value) =>
        element.SetValue(ToEndProperty, value);

    public static bool GetToEnd(DependencyObject element) =>
        (bool)element.GetValue(ToEndProperty);

    private static void OnToEndChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs args) {
        if (dependencyObject is not ScrollViewer scrollViewer) {
            return;
        }
        scrollViewer.ScrollChanged -= OnScrollChanged;
        if (args.NewValue is true) {
            scrollViewer.ScrollChanged += OnScrollChanged;
        }
    }

    private static void OnScrollChanged(object sender, ScrollChangedEventArgs args) {
        if (sender is not ScrollViewer scrollViewer) {
            return;
        }

        // An unchanged extent means the user moved the viewport rather than the content
        // growing, so this is where the pin is decided. Growth is handled below, using
        // whatever the pin was before the new content arrived.
        if (args.ExtentHeightChange == 0) {
            scrollViewer.SetValue(IsPinnedProperty, scrollViewer.VerticalOffset >= scrollViewer.ScrollableHeight - 1);
            return;
        }

        if ((bool)scrollViewer.GetValue(IsPinnedProperty)) {
            scrollViewer.ScrollToEnd();
        }
    }

}
