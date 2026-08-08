using System.Threading;
using System.Windows;

using Battlegrounds.Views;

namespace Battlegrounds.Test.Views;

/// <summary>
/// Loads the lobby views so that their XAML is actually parsed.
/// </summary>
/// <remarks>
/// Neither the compiler nor scripts/check-xaml-resources.py catches a resource that exists but
/// is not reachable from Application.Resources — a {StaticResource} throws only when the view is
/// first constructed, which for the lobby means being in a lobby. Constructing them here is the
/// cheapest place to find out.
///
/// This covers markup only. The data context is left null, so the bindings are never exercised;
/// what is under test is that every template, style and resource reference resolves.
/// </remarks>
[TestOf(typeof(LobbySlotView))]
[Apartment(ApartmentState.STA)]
public sealed class LobbyViewMarkupTests {

    [Test]
    public void LobbySlotView_ResolvesEveryResourceItReferences() {
        EnsureThemedApplication();

        Assert.That(() => new LobbySlotView(), Throws.Nothing);
    }

    [Test]
    public void LobbyView_ResolvesEveryResourceItReferences() {
        EnsureThemedApplication();

        Assert.That(() => new LobbyView(null!), Throws.Nothing);
    }

    /// <summary>
    /// WPF allows one Application per process and the test host does not create one, so this
    /// builds the same single instance App.xaml would and leaves it in place for later tests.
    /// </summary>
    private static void EnsureThemedApplication() {
        var application = Application.Current ?? new Application();
        var themeUri = new Uri("pack://application:,,,/Battlegrounds;component/Themes/Theme.xaml");
        if (!application.Resources.MergedDictionaries.Any(dictionary => dictionary.Source == themeUri)) {
            application.Resources.MergedDictionaries.Add(new ResourceDictionary { Source = themeUri });
        }
    }

}
