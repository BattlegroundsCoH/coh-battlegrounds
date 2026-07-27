using System.Windows;
using System.Windows.Controls;

namespace Battlegrounds.Controls;

/// <summary>
/// The title row at the top of a page: uppercase heading on the left, actions on the right.
/// </summary>
/// <remarks>
/// <see cref="HeaderedContentControl.Header"/> is the title; the content is the action
/// cluster, normally a horizontal <c>StackPanel</c> of buttons. Set <see cref="Eyebrow"/> to
/// put a tracked gold eyebrow above the title, as the Login and Lobby screens do.
/// </remarks>
public class PageHeader : HeaderedContentControl {

    /// <summary>Optional eyebrow above the title. Null or empty collapses it.</summary>
    public static readonly DependencyProperty EyebrowProperty =
        DependencyProperty.Register(
            nameof(Eyebrow), typeof(string), typeof(PageHeader),
            new FrameworkPropertyMetadata(null));

    public string? Eyebrow {
        get => (string?)GetValue(EyebrowProperty);
        set => SetValue(EyebrowProperty, value);
    }

    static PageHeader() {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(PageHeader), new FrameworkPropertyMetadata(typeof(PageHeader)));
    }

}
