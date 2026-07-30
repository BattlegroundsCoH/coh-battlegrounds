using System.Windows;
using System.Windows.Controls;

namespace Battlegrounds.Controls;

/// <summary>
/// A panel surface: card background, hairline border, square corners, standard padding.
/// </summary>
/// <remarks>
/// The redesign replaced drop-shadowed rounded panels with flat hairline-bordered ones, and
/// every screen had been hand-assembling its own <c>Border</c> for the job — HomeView alone
/// carried a local <c>DashboardCard</c> style. This is that surface, once.
/// <para>
/// Set <see cref="Header"/> to get a section title above the content; leave it null and the
/// header collapses.
/// </para>
/// </remarks>
public class Card : HeaderedContentControl {

    static Card() {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(Card), new FrameworkPropertyMetadata(typeof(Card)));
    }

}
