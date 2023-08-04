using Battlegrounds.Game;
using Battlegrounds.Game.Gameplay;

namespace Battlegrounds.Testing.Core.Game.Gameplay;

public class FactionTest {

    [Test]
    public void CanGetCoH2Factions() {

        IList<Faction> factions = Faction.GetFactions(GameCase.CompanyOfHeroes2);
        CollectionAssert.Contains(factions, Faction.Soviet);
        CollectionAssert.Contains(factions, Faction.Wehrmacht);
        CollectionAssert.Contains(factions, Faction.OberkommandoWest);
        CollectionAssert.Contains(factions, Faction.America);
        CollectionAssert.Contains(factions, Faction.British);

    }

    [Test]
    public void CanGetCoH3Factions() {

        IList<Faction> factions = Faction.GetFactions(GameCase.CompanyOfHeroes3);
        CollectionAssert.Contains(factions, Faction.BritishAfrica);
        CollectionAssert.Contains(factions, Faction.Germans);
        CollectionAssert.Contains(factions, Faction.AfrikaKorps);
        CollectionAssert.Contains(factions, Faction.Americans);

    }

    [Test]
    public void GetCoH3IsCoH3List() {
        Assert.That(Faction.CoH3Factions, Is.EqualTo(Faction.GetFactions(GameCase.CompanyOfHeroes3)));
    }

    [Test]
    public void GetCoH2IsCoH2List() {
        Assert.That(Faction.CoH2Factions, Is.EqualTo(Faction.GetFactions(GameCase.CompanyOfHeroes2)));
    }

    [Test]
    public void CanGetCoH3Faction() {
        Assert.Multiple(() => {
            Assert.That(Faction.AfrikaKorps, Is.EqualTo(Faction.FromName("dak", GameCase.CompanyOfHeroes3)));
            Assert.That(Faction.Americans, Is.EqualTo(Faction.FromName("americans", GameCase.CompanyOfHeroes3)));
        });
    }

    [Test]
    public void GetsCoH2BeforeCoH3() {
        Assert.That(Faction.British, Is.EqualTo(Faction.FromName("ukf", GameCase.Any)));
    }

    [Test]
    public void CanCheckGame() {
        Assert.Multiple(() => {
            Assert.That(Faction.AreSameGame(Faction.Germans, Faction.Wehrmacht), Is.False);
            Assert.That(Faction.AreSameGame(Faction.America, Faction.Americans), Is.False);
            Assert.That(Faction.AreSameGame(Faction.OberkommandoWest, Faction.Wehrmacht), Is.True);
        });
    }

    [Test]
    public void CanCheckAllies() {
        Assert.Multiple(() => {
            Assert.That(Faction.AreSameTeam(Faction.Germans, Faction.Wehrmacht), Is.True);
            Assert.That(Faction.AreSameTeam(Faction.America, Faction.Americans), Is.True);
            Assert.That(Faction.AreSameTeam(Faction.OberkommandoWest, Faction.Wehrmacht), Is.True);
            Assert.That(Faction.AreSameTeam(Faction.OberkommandoWest, Faction.British), Is.False);
            Assert.That(Faction.AreSameTeam(Faction.BritishAfrica, Faction.Germans), Is.False);
        });
    }

}
