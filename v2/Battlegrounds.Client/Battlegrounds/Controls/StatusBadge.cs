using System.Windows;
using System.Windows.Controls;

namespace Battlegrounds.Controls;

/// <summary>
/// How a <see cref="StatusBadge"/> is tinted.
/// </summary>
public enum BadgeTone {
    /// <summary>Green. A win, a completed action, an available slot.</summary>
    Success,
    /// <summary>Red. A loss, a casualty, an unavailable slot.</summary>
    Danger,
    /// <summary>Amber. Something that needs attention but has not failed.</summary>
    Caution,
    /// <summary>Gold. Emphasis without a verdict attached.</summary>
    Accent,
    /// <summary>Grey. Present but unremarkable.</summary>
    Neutral,
}

/// <summary>
/// A small square chip carrying a one-word verdict — VICTORY, DEFEAT, KIA.
/// </summary>
/// <remarks>
/// Chips are filled, so the foreground is chosen for contrast against the fill rather than
/// inherited: dark text on green, amber and gold, white on red. That pairing lives in the
/// default style, keyed off <see cref="Tone"/>.
/// </remarks>
public class StatusBadge : ContentControl {

    public static readonly DependencyProperty ToneProperty =
        DependencyProperty.Register(
            nameof(Tone), typeof(BadgeTone), typeof(StatusBadge),
            new FrameworkPropertyMetadata(BadgeTone.Neutral));

    public BadgeTone Tone {
        get => (BadgeTone)GetValue(ToneProperty);
        set => SetValue(ToneProperty, value);
    }

    static StatusBadge() {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(StatusBadge), new FrameworkPropertyMetadata(typeof(StatusBadge)));
    }

}
