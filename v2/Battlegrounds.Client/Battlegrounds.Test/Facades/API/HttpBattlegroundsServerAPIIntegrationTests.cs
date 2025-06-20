using Battlegrounds.Facades.API;
using Battlegrounds.Models;
using Battlegrounds.Models.Replays;
using Battlegrounds.Serializers;
using Battlegrounds.Test.Services;

namespace Battlegrounds.Test.Facades.API;

/// <summary>
/// Provides integration tests for the <see cref="HttpBattlegroundsServerAPI"/> class.
/// </summary>
/// <remarks>This test class is designed to verify the behavior and functionality of the <see
/// cref="HttpBattlegroundsServerAPI"/>  when interacting with a live or simulated server environment. It ensures that
/// the API methods perform as expected  under various conditions, including handling responses, errors, and edge
/// cases.</remarks>
[TestFixture, TestOf(typeof(HttpBattlegroundsServerAPI))]
public sealed class HttpBattlegroundsServerAPIIntegrationTests : ServerIntegrationTests {

    private HttpBattlegroundsServerAPI? _api;
    private MockIntegrationUserService _mockUserIntegrationService;

    [SetUp]
    public void SetUp() {
        _mockUserIntegrationService = new MockIntegrationUserService("user"); // TODO: Make this also use a docker container based on the mock auth service
        _api = new HttpBattlegroundsServerAPI(
            new TestLogger<HttpBattlegroundsServerAPI>(),
            _mockUserIntegrationService.HttpClient,
            _mockUserIntegrationService,
            new BinaryCompanyDeserializer(new BlueprintFixtureService()),
            new Configuration { BattlegroundsServerHost = $"http://{IntegrationServerHost}", BattlegroundsHttpServerPort = IntegrationServerPort }
        );
    }

    [Test]
    public async Task ReportMatchResults_BasicResult_ShouldSucceed() {

        // Arrange
        MatchResult result = new MatchResult {
            MatchId = "test-match-id",
            LobbyId = "local-singleplayer", // Bypass need for lobby Id validation
            MatchDuration = TimeSpan.FromMinutes(30),
        };

        // Act
        var response = await _api!.ReportMatchResults(result);

        // Assert
        Assert.That(response, Is.True, "Expected ReportMatchResults to succeed for basic result.");

    }

}
