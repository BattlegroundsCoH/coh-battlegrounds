using Battlegrounds.Core.Games;
using Battlegrounds.Core.Lobbies;
using Battlegrounds.Core.Services;
using Battlegrounds.Core.Services.Standard;
using Battlegrounds.Core.Test.Companies;
using Battlegrounds.Core.Test.Games.Gamemodes;
using Battlegrounds.Core.Test.Lobbies;

namespace Battlegrounds.Core.Test.Services.Standard;

[TestFixture]
public class MatchModBuilderServiceTest {

    private MockLobby lobby;
    private IMatchModBuilderService matchModBuilderService;
    private IGamemodeService gamemodeService;
    private ICompanyService companyService;
    private IFileSystemService fileSystemService;

    private MockLobby.MockPlayer opponentPlayer;

    [SetUp]
    public void SetUp() {
        opponentPlayer = new MockLobby.MockPlayer(100ul, "Test Opponent");
        lobby = LobbyFixture.CreateLobby(builder => {
            builder.MatchCompanies[LobbyFixture.LobbyDefaultHost.PlayerId] = CompanyFixture.DesertRats;
            builder.MatchCompanies[100ul] = CompanyFixture.AfrikaKorps;
            builder.Settings = new Dictionary<string, string> {
                ["gamemode"] = "victory_points"
            };
            builder.Team1.Slot(0, s => { s.Occupy(LobbyFixture.LobbyDefaultHost.Clone()); s.Player!.CompanyId = CompanyFixture.DesertRats.Id.ToString(); });
            builder.Team2.Slot(0, s => { s.Occupy(opponentPlayer); });
        });
        gamemodeService = Substitute.For<IGamemodeService>();
        companyService = Substitute.For<ICompanyService>();
        fileSystemService = Substitute.For<IFileSystemService>();
        matchModBuilderService = new MatchModBuilderService(gamemodeService, companyService, fileSystemService, CoreTest.Services, CoreTest.Services.GetService<ILogger<MatchModBuilderService>>()!);
    }

    [Test]
    public void CanCompileSimpleCoH3Lobby() {

        // Prepare
        companyService.GetCompany(CompanyFixture.DesertRats.Id.ToString()).Returns(CompanyFixture.DesertRats);
        gamemodeService.GetGamemode(CoH3.COH3_NAME, "victory_points").Returns(GamemodeFixture.CoH3_VictoryPoints);
        MemoryStream scarStream = new MemoryStream();
        fileSystemService.OpenWriteTempFile("match_data.scar").Returns(scarStream);

        // Do
        var built = matchModBuilderService.BuildMatchGamemode(lobby).Result;
        
        // Assert
        Assert.That(built, Is.True);

    }

}
