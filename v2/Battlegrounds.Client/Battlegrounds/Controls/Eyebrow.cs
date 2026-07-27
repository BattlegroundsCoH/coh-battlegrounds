using System.Windows;
using System.Windows.Controls;

namespace Battlegrounds.Controls;

/// <summary>
/// A short red rule followed by a widely tracked gold label — "— LOBBY BRIEFING".
/// </summary>
/// <remarks>
/// The signature mark of the design system; the website ships the same thing as its
/// <c>.eyebrow</c> class. It labels the section that follows, so it is always paired with a
/// heading directly beneath it.
/// </remarks>
public class Eyebrow : Control {

    /// <summary>The label text. Authored uppercase — nothing here cases it for you.</summary>
    public static readonly DependencyProperty TextProperty =
        DependencyProperty.Register(
            nameof(Text), typeof(string), typeof(Eyebrow),
            new FrameworkPropertyMetadata(string.Empty));

    public string Text {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    static Eyebrow() {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(Eyebrow), new FrameworkPropertyMetadata(typeof(Eyebrow)));
    }

}
