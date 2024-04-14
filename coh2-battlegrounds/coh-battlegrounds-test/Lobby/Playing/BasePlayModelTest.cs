using Battlegrounds.Game.Gameplay;
using Battlegrounds.Game.Match;
using Battlegrounds.Game.Scenarios.CoH3;
using Battlegrounds.Lobby.Components;
using Battlegrounds.Lobby.Playing;
using Battlegrounds.Modding;
using Battlegrounds.Modding.Battlegrounds;
using Battlegrounds.Networking.LobbySystem;
using Battlegrounds.Networking.LobbySystem.Json;
using Battlegrounds.Steam;
using Battlegrounds.Testing.TestUtil;
using Battlegrounds.Util.Threading;

using Moq;

namespace Battlegrounds.Testing.Lobby.Playing;

[TestFixture]
public class BasePlayModelTest : TestWithMockedBattlegroundsContext {

    private class TestPlayModel : BasePlayModel {
        public SessionInfo SessionInfo => m_info;
        public TestPlayModel(ILobbyHandle handle, IChatSpectator lobbyChat, IDispatcher dispatcher) : base(handle, lobbyChat, dispatcher) {}
        public void TestCreateMatchInfo(IModPackage package) {
            base.CreateMatchInfo(package);
        }
    }

    private Mock<ILobbyHandle> mockLobbyHandle;
    private Mock<IChatSpectator> mockChatSpectator;
    private Mock<IDispatcher> mockDispatcher;
    private Mock<IModPackage> mockModPackage;
    private TestPlayModel playModel; // Assume this is a concrete implementation of BasePlayModel

    private readonly ModGuid gamemodeGuid = ModGuid.FromGuid(Guid.NewGuid());
    private readonly ModGuid tuningGuid = ModGuid.FromGuid(Guid.NewGuid());

    [SetUp]
    public void SetUp() {

        // Set up the mocked dependencies
        this.mockLobbyHandle = new Mock<ILobbyHandle>();
        this.mockChatSpectator = new Mock<IChatSpectator>();
        this.mockDispatcher = new Mock<IDispatcher>();
        this.mockModPackage = new Mock<IModPackage>();

        // Initialize the class under test
        this.playModel = new TestPlayModel(mockLobbyHandle.Object, mockChatSpectator.Object, mockDispatcher.Object);

    }

    [Test]
    public void CreateMatchInfo_SetsCorrectInfo_FromModPackage() {

        // Arrange: Set up the mod package to return specific settings
        var scenario = "TestScenario";
        var gamemode = "TestGamemode";
        var gamemodeOption = 1;
        var enableSupply = true;
        var enableWeather = true;
        var settings = new Dictionary<string, string>
        {
            {"selected_map", scenario},
            {"selected_wc", gamemode},
            {"selected_wco", gamemodeOption.ToString()},
            {"selected_supply", enableSupply ? "1" : "0"},
            {"selected_daynight", enableWeather ? "1" : "0"}
        };

        this.mockLobbyHandle.Setup(lh => lh.Settings).Returns(settings);
        this.mockLobbyHandle.Setup(lh => lh.Allies).Returns(new JsonLobbyTeam(new JsonLobbySlot[] {
            new JsonLobbySlot(0, 0, LobbyConstants.STATE_OCCUPIED, new JsonLobbyMember(0, "Alfredo", 0, 0, LobbyMemberState.Waiting, 
                new JsonLobbyCompany(false, false, "Desert Rats", Faction.FactionStrBritishAfrica, 0, "rifles")))
        }, 0, 1, "allies"));
        this.mockLobbyHandle.Setup(lh => lh.Axis).Returns(new JsonLobbyTeam(new JsonLobbySlot[] {
            new JsonLobbySlot(0, 1, LobbyConstants.STATE_OCCUPIED, new JsonLobbyMember(1, "Alfredo's Friend", 0, 0, LobbyMemberState.Waiting,
                new JsonLobbyCompany(false, false, "Afrika Korps", Faction.FactionStrAfrikaKorps, 0, "mechanised")))
        }, 1, 1, "axis"));
        this.mockLobbyHandle.Setup(lh => lh.Self).Returns(SteamUser.CreateTempUser(0, "Alfredo"));
        this.mockLobbyHandle.Setup(lh => lh.Game).Returns(Game.GameCase.CompanyOfHeroes3);

        this.mockModPackage.Setup(mp => mp.ID).Returns("test_mod");
        this.mockModPackage.Setup(mp => mp.GamemodeGUID).Returns(gamemodeGuid);
        this.mockModPackage.Setup(mp => mp.TuningGUID).Returns(tuningGuid);

        this.mockDbManager.Setup(dbm => dbm.GetScenarioList(this.mockModPackage.Object, Game.GameCase.CompanyOfHeroes3)).Returns(mockScenarioList.Object);
        this.mockDbManager.Setup(dbm => dbm.GetGamemodeList(this.mockModPackage.Object, Game.GameCase.CompanyOfHeroes3)).Returns(mockGamemodeList.Object);

        this.mockScenarioList.Setup(sl => sl.FromFilename(scenario)).Returns(new CoH3Scenario() { Name = scenario });

        this.mockGamemodeList.Setup(gl => gl.GetGamemodeByName(gamemodeGuid, gamemode)).Returns(new Wincondition(gamemode, gamemodeGuid));

        this.mockModManager.Setup(mm => mm.GetMod<ITuningMod>(tuningGuid)).Returns(new BattlegroundsTuning(mockModPackage.Object));

        // Act: Call the method under test
        this.playModel.TestCreateMatchInfo(mockModPackage.Object);

        // Assert: Verify that the method set the correct info on the play model
        Assert.Multiple(() => { 
            
            // Assert basics
            Assert.That(this.playModel.SessionInfo.SelectedScenario.Name, Is.EqualTo(scenario));
            Assert.That(this.playModel.SessionInfo.SelectedGamemode.Name, Is.EqualTo(gamemode));
            Assert.That(this.playModel.SessionInfo.SelectedGamemodeOption, Is.EqualTo(gamemodeOption));
            Assert.That(this.playModel.SessionInfo.EnableSupply, Is.EqualTo(enableSupply));
            Assert.That(this.playModel.SessionInfo.EnableDayNightCycle, Is.EqualTo(enableWeather));

            // Assert Allies
            Assert.That(this.playModel.SessionInfo.Allies, Has.Length.EqualTo(1));
            Assert.That(this.playModel.SessionInfo.Allies[0].UserID, Is.EqualTo(0));
            Assert.That(this.playModel.SessionInfo.Allies[0].IsHuman, Is.True);
            Assert.That(this.playModel.SessionInfo.Allies[0].UserDisplayname, Is.EqualTo("Alfredo"));

            // Assert Axis
            Assert.That(this.playModel.SessionInfo.Axis, Has.Length.EqualTo(1));
            Assert.That(this.playModel.SessionInfo.Axis[0].UserID, Is.EqualTo(1));
            Assert.That(this.playModel.SessionInfo.Axis[0].IsHuman, Is.True);
            Assert.That(this.playModel.SessionInfo.Axis[0].UserDisplayname, Is.EqualTo("Alfredo's Friend"));

        });

    }

}
