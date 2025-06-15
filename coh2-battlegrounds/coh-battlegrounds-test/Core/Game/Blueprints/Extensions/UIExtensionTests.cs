using Battlegrounds.Game.Blueprints.Extensions;

namespace Battlegrounds.Testing.Core.Game.Blueprints.Extensions;

[TestFixture]
public class UIExtensionTests {

    [Test]
    public void IsEmpty_OnlyTextContentEmpty_ReturnsTrue() {
        UIExtension ui = new UIExtension {
            ScreenName = "",
            ShortDescription = "",
            LongDescription = "",
            Portrait = "portrait.png",
            Icon = "icon.png",
            Symbol = "symbol.png"
        };

        Assert.That(UIExtension.IsEmpty(ui), Is.True);
    }

    [Test]
    public void IsEmpty_TextAndGraphicalContentNotEmpty_ReturnsFalse() {
        UIExtension ui = new UIExtension {
            ScreenName = "Screen",
            ShortDescription = "Short description",
            LongDescription = "Long description",
            Portrait = "portrait.png",
            Icon = "icon.png",
            Symbol = "symbol.png"
        };

        Assert.That(UIExtension.IsEmpty(ui), Is.False);
    }

    [Test]
    public void IsEmpty_WithCompletelyEmptyFlag_TextAndGraphicalContentEmpty_ReturnsTrue() {
        UIExtension ui = new UIExtension {
            ScreenName = "",
            ShortDescription = "",
            LongDescription = "",
            Portrait = "",
            Icon = "",
            Symbol = ""
        };

        Assert.That(UIExtension.IsEmpty(ui, true), Is.True);
    }

    [Test]
    public void IsEmpty_WithCompletelyEmptyFlag_OnlyTextContentEmpty_ReturnsFalse() {
        UIExtension ui = new UIExtension {
            ScreenName = "",
            ShortDescription = "",
            LongDescription = "",
            Portrait = "portrait.png",
            Icon = "icon.png",
            Symbol = "symbol.png"
        };

        Assert.That(UIExtension.IsEmpty(ui, true), Is.False);
    }

}
