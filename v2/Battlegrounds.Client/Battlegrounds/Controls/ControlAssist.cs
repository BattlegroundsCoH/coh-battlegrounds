using System.Windows;
using System.Windows.Media;

namespace Battlegrounds.Controls;

/// <summary>
/// Attached properties that let one control template serve many visual variants.
/// </summary>
/// <remarks>
/// <para>
/// A WPF <c>Setter.Value</c> cannot be a binding, so the usual way to give a button a
/// different hover colour is to copy the whole <c>ControlTemplate</c> and edit the trigger
/// — which is how the old Buttons.xaml ended up with seven near-identical templates that
/// all had to be updated in lockstep.
/// </para>
/// <para>
/// Instead, the state brushes hang off the control as attached properties and the single
/// shared template binds through <see cref="Converters.StateBrushConverter"/>. Adding a
/// variant is then a handful of setters rather than another copy of the template.
/// </para>
/// <para>
/// Leaving a state brush unset is fine: the converter falls back to the base brush, so a
/// variant only declares the states where it actually differs.
/// </para>
/// </remarks>
public static class ControlAssist {

    /// <summary>Background while the pointer is over the control.</summary>
    public static readonly DependencyProperty HoverBackgroundProperty =
        DependencyProperty.RegisterAttached(
            "HoverBackground", typeof(Brush), typeof(ControlAssist),
            new FrameworkPropertyMetadata(null));

    /// <summary>Background while the control is pressed.</summary>
    public static readonly DependencyProperty PressedBackgroundProperty =
        DependencyProperty.RegisterAttached(
            "PressedBackground", typeof(Brush), typeof(ControlAssist),
            new FrameworkPropertyMetadata(null));

    /// <summary>Border brush while the pointer is over the control.</summary>
    public static readonly DependencyProperty HoverBorderBrushProperty =
        DependencyProperty.RegisterAttached(
            "HoverBorderBrush", typeof(Brush), typeof(ControlAssist),
            new FrameworkPropertyMetadata(null));

    /// <summary>Foreground while the pointer is over the control.</summary>
    public static readonly DependencyProperty HoverForegroundProperty =
        DependencyProperty.RegisterAttached(
            "HoverForeground", typeof(Brush), typeof(ControlAssist),
            new FrameworkPropertyMetadata(null));

    /// <summary>
    /// Hint text shown by a text input while it is empty. Replaces the pair of
    /// near-duplicate TextBox styles that previously differed only in this string.
    /// </summary>
    public static readonly DependencyProperty PlaceholderProperty =
        DependencyProperty.RegisterAttached(
            "Placeholder", typeof(string), typeof(ControlAssist),
            new FrameworkPropertyMetadata(string.Empty));

    public static Brush? GetHoverBackground(DependencyObject e) => (Brush?)e.GetValue(HoverBackgroundProperty);
    public static void SetHoverBackground(DependencyObject e, Brush? v) => e.SetValue(HoverBackgroundProperty, v);

    public static Brush? GetPressedBackground(DependencyObject e) => (Brush?)e.GetValue(PressedBackgroundProperty);
    public static void SetPressedBackground(DependencyObject e, Brush? v) => e.SetValue(PressedBackgroundProperty, v);

    public static Brush? GetHoverBorderBrush(DependencyObject e) => (Brush?)e.GetValue(HoverBorderBrushProperty);
    public static void SetHoverBorderBrush(DependencyObject e, Brush? v) => e.SetValue(HoverBorderBrushProperty, v);

    public static Brush? GetHoverForeground(DependencyObject e) => (Brush?)e.GetValue(HoverForegroundProperty);
    public static void SetHoverForeground(DependencyObject e, Brush? v) => e.SetValue(HoverForegroundProperty, v);

    public static string GetPlaceholder(DependencyObject e) => (string)e.GetValue(PlaceholderProperty);
    public static void SetPlaceholder(DependencyObject e, string v) => e.SetValue(PlaceholderProperty, v);

}
