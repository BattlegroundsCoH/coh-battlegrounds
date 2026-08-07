using System.Net;
using System.Text;

using Battlegrounds.Facades.API;
using Battlegrounds.Models;
using Battlegrounds.Models.Companies;
using Battlegrounds.Models.Replays;
using Battlegrounds.Serializers;
using Battlegrounds.Services;
using Battlegrounds.Test.Models.Companies;

using NSubstitute;

namespace Battlegrounds.Test.Facades.API;

[TestOf(typeof(HttpBattlegroundsServerAPI))]
public class HttpBattlegroundsServerAPITests {

    private HttpBattlegroundsServerAPI _api;
    private IAsyncHttpClient _httpClient;
    private IUserService _userService;
    private ICompanyDeserializer _companyDeserializer;
    private TestLogger<HttpBattlegroundsServerAPI> _logger;
    private Configuration _configuration;

    [SetUp]
    public void SetUp() {
        // Set up mocks and test subjects for each test
        _httpClient = Substitute.For<IAsyncHttpClient>();
        _userService = Substitute.For<IUserService>();
        _companyDeserializer = Substitute.For<ICompanyDeserializer>();
        _logger = new TestLogger<HttpBattlegroundsServerAPI>();

        // Configure port from container
        _configuration = new Configuration();

        _api = new HttpBattlegroundsServerAPI(_logger, _httpClient, _userService, _companyDeserializer, _configuration);
    }

    [TearDown]
    public void TearDown() {
        // Clean up after each test
        _httpClient.ClearReceivedCalls();
        _userService.ClearReceivedCalls();
        _companyDeserializer.ClearReceivedCalls();

        _logger.Dispose();
    }

    [Test]
    public async Task GetCompanyAsync_WhenCompanyExists_ReturnsCompany() {
        // Arrange
        string companyId = "desert_rats";
        string userId = "test-user-id";
        var company = CompanyFixture.DESERT_RATS;
        var expectedRequestUri = $"{_api.BaseUrl}{HttpBattlegroundsServerAPI.DownloadCompanyEndpoint}?guid={companyId}&userId={userId}";

        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK);
        var memoryStream = new MemoryStream();

        _httpClient.SendRequestAsync(Arg.Any<HttpRequestMessage>())
            .Returns(httpResponse);
        httpResponse.Content = new StreamContent(memoryStream);
        _companyDeserializer.DeserializeCompany(Arg.Any<Stream>())
            .Returns(company);

        // Act
        var result = await _api.GetCompanyAsync(companyId, userId);

        // Assert
        Assert.That(result, Is.Not.Null, "Company should not be null");
        await _httpClient.Received(1).SendRequestAsync(Arg.Is<HttpRequestMessage>(
            req => req.Method == HttpMethod.Get && req.RequestUri!.ToString() == expectedRequestUri
        ));
    }

    [Test]
    public async Task GetCompanyAsync_WhenCompanyDoesNotExist_ReturnsNull() {
        // Arrange
        string companyId = "nonexistent-company";
        string userId = "test-user-id";

        var httpResponse = new HttpResponseMessage(HttpStatusCode.NotFound);

        _httpClient.SendRequestAsync(Arg.Any<HttpRequestMessage>())
            .Returns(httpResponse);

        // Act
        var result = await _api.GetCompanyAsync(companyId, userId);

        // Assert
        Assert.That(result, Is.Null, "Result should be null for non-existent company");
    }

    [Test]
    public async Task UploadCompanyAsync_WhenSuccessful_ReturnsTrue() {
        // Arrange
        string companyId = "test-company";
        string faction = "british_africa";
        var stream = new MemoryStream();

        _userService.GetLocalUserTokenAsync().Returns("test-token");

        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK);
        _httpClient.SendRequestAsync(Arg.Any<HttpRequestMessage>())
            .Returns(httpResponse);

        // Act
        var result = await _api.UploadCompanyAsync(companyId, faction, 1, stream);

        // Assert
        Assert.That(result, Is.True, "Upload should return true on success");
        await _userService.Received().GetLocalUserTokenAsync();
        await _httpClient.Received(1).SendRequestAsync(Arg.Is<HttpRequestMessage>(
            req => req.Method == HttpMethod.Post &&
                   req.Content is StreamContent &&
                   req.Headers.Any(kvp => kvp.Key == "Authorization" &&
                                          kvp.Value.First() == "Bearer test-token") &&
                   req.RequestUri!.ToString().Contains($"guid={companyId}") &&
                   req.RequestUri.ToString().Contains($"faction={faction}")
        ));
    }

    [Test]
    public async Task UploadCompanyAsync_WhenServerError_ReturnsFalse() {
        // Arrange
        string companyId = "test-company";
        string faction = "british_africa";
        var stream = new MemoryStream();

        _userService.GetLocalUserTokenAsync().Returns("test-token");

        var httpResponse = new HttpResponseMessage(HttpStatusCode.InternalServerError);
        _httpClient.SendRequestAsync(Arg.Any<HttpRequestMessage>())
            .Returns(httpResponse);

        // Act
        var result = await _api.UploadCompanyAsync(companyId, faction, 1, stream);

        // Assert
        Assert.That(result, Is.False, "Upload should return false on server error");
    }

    [Test]
    public async Task DeleteCompanyAsync_WhenSuccessful_ReturnsTrue() {

        // Arrange
        string companyId = "test-company";

        _userService.GetLocalUserTokenAsync().Returns("test-token");

        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK);
        _httpClient.SendRequestAsync(Arg.Any<HttpRequestMessage>())
            .Returns(httpResponse);

        // Act
        var result = await _api.DeleteCompanyAsync(companyId);

        // Assert
        Assert.That(result, Is.True, "Delete should return true on success");
        await _httpClient.Received(1).SendRequestAsync(Arg.Is<HttpRequestMessage>(
            req => req.Method == HttpMethod.Delete &&
                   req.RequestUri!.ToString().Contains($"guid={companyId}")
        ));

    }

    [Test]
    public async Task DeleteCompanyAsync_WhenServerError_ReturnsFalse() {

        // Arrange
        string companyId = "test-company";

        _userService.GetLocalUserTokenAsync().Returns("test-token");

        var httpResponse = new HttpResponseMessage(HttpStatusCode.InternalServerError);
        _httpClient.SendRequestAsync(Arg.Any<HttpRequestMessage>())
            .Returns(httpResponse);

        // Act
        var result = await _api.DeleteCompanyAsync(companyId);

        // Assert
        Assert.That(result, Is.False, "Delete should return false on server error");

    }

    [Test]
    public void BaseUrl_ShouldReturnCorrectUrl() {
        // Arrange
        string expectedBaseUrl = $"{_configuration.BattlegroundsServerHost}:{_configuration.BattlegroundsHttpServerPort}";
        // Act
        string actualBaseUrl = _api.BaseUrl;
        // Assert
        Assert.That(actualBaseUrl, Is.EqualTo(expectedBaseUrl), "Base URL should match the configured host and port");
    }

    [Test]
    public async Task ReportMatchResultsAsync_WhenSuccessful_ReturnsTrue() {
        // Arrange
        var matchResults = new MatchResult {
            MatchId = "test-match",
            LobbyId = "test-lobby",
            MatchDuration = TimeSpan.FromMinutes(30)
        };
        var mockUser = new User { UserId = "test-user" };
        
        _userService.GetLocalUserAsync().Returns(mockUser);
        _userService.GetLocalUserTokenAsync().Returns("test-token");
        
        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK);
        _httpClient.SendRequestAsync(Arg.Any<HttpRequestMessage>())
            .Returns(httpResponse);
        
        // Act
        var result = await _api.ReportMatchResults(matchResults);

        // Assert
        Assert.That(result, Is.True, "Report should return true on success");
        await _httpClient.Received(1).SendRequestAsync(Arg.Is<HttpRequestMessage>(
            req => req.Method == HttpMethod.Post &&
                   req.Content is StreamContent &&
                   req.Content.Headers.ContentType != null &&
                   req.Content.Headers.ContentType.MediaType == "application/json" &&
                   req.Headers.Any(kvp => kvp.Key == "Authorization" &&
                                          kvp.Value.First() == "Bearer test-token") &&
                   req.RequestUri!.ToString().Contains("/api/v1/match/report")
        ));
    }

    [Test]
    public async Task ReportMatchResultsAsync_WhenServerError_ReturnsFalse() {
        // Arrange
        var matchResults = new MatchResult {
            MatchId = "test-match",
            MatchDuration = TimeSpan.FromMinutes(30)
        };
        var mockUser = new User { UserId = "test-user" };

        _userService.GetLocalUserAsync().Returns(mockUser);
        _userService.GetLocalUserTokenAsync().Returns("test-token");

        var httpResponse = new HttpResponseMessage(HttpStatusCode.InternalServerError);
        _httpClient.SendRequestAsync(Arg.Any<HttpRequestMessage>())
            .Returns(httpResponse);

        // Act
        var result = await _api.ReportMatchResults(matchResults);

        // Assert
        Assert.That(result, Is.False, "Report should return false on server error");

    }

    [Test]
    public async Task GetLatestMatchResult_WhenSuccessful_ReturnsDeserializedMatchResult() {
        // Arrange
        const string json = """
            {
                "isValid": true,
                "lobbyId": "test-lobby",
                "gameId": "",
                "matchId": "test-match",
                "modVersion": "1.0",
                "scenario": "test-scenario",
                "matchDuration": "00:30:00",
                "companyModifiers": {
                    "player-1": [{ "squadId": 1, "eventType": "kill_squad" }]
                },
                "playerCompanies": { "player-1": "company-a" },
                "winners": ["player-1"],
                "losers": [],
                "players": ["player-1"],
                "concluded": true
            }
            """;

        var content = new StringContent(json, Encoding.UTF8, "application/json");
        _httpClient.SendRequestAsync(Arg.Any<HttpRequestMessage>())
            .Returns(new HttpResponseMessage(HttpStatusCode.OK) { Content = content });

        // Act
        var result = await _api.GetLatestMatchResult("test-lobby");

        // Assert
        Assert.That(result, Is.Not.Null);
        using (Assert.EnterMultipleScope()) {
            Assert.That(result.LobbyId, Is.EqualTo("test-lobby"));
            Assert.That(result.MatchId, Is.EqualTo("test-match"));
            Assert.That(result.Scenario, Is.EqualTo("test-scenario"));
            Assert.That(result.MatchDuration, Is.EqualTo(TimeSpan.FromMinutes(30)));
            Assert.That(result.Concluded, Is.True);
            Assert.That(result.Winners, Is.EquivalentTo(new[] { "player-1" }));
            Assert.That(result.Losers, Is.Empty);
            Assert.That(result.Players, Is.EquivalentTo(new[] { "player-1" }));
            Assert.That(result.PlayerCompanies["player-1"], Is.EqualTo("company-a"));
            Assert.That(result.CompanyModifiers["player-1"].First!.Value.SquadId, Is.EqualTo(1));
            Assert.That(result.CompanyModifiers["player-1"].First!.Value.EventType, Is.EqualTo(CompanyEventModifier.EVENT_TYPE_KILL_SQUAD));
        }
        await _httpClient.Received(1).SendRequestAsync(Arg.Is<HttpRequestMessage>(
            req => req.Method == HttpMethod.Get &&
                   req.RequestUri!.ToString().Contains(HttpBattlegroundsServerAPI.GetLobbyResultEndpoint) &&
                   req.RequestUri.ToString().Contains("lobbyId=test-lobby")
        ));
    }

    [Test]
    public async Task GetLatestMatchResult_WhenServerError_ReturnsNull() {
        // Arrange
        _httpClient.SendRequestAsync(Arg.Any<HttpRequestMessage>())
            .Returns(new HttpResponseMessage(HttpStatusCode.NotFound));

        // Act
        var result = await _api.GetLatestMatchResult("missing-lobby");

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task DownloadGamemodeAsync_WhenSuccessful_WritesFileAndReturnsTrue() {
        // Arrange
        string lobbyId = "test-lobby";
        string destinationPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.sga");
        byte[] payload = Encoding.UTF8.GetBytes("gamemode-payload-content");

        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK) {
            Content = new ByteArrayContent(payload)
        };

        _httpClient.SendRequestAsync(Arg.Any<HttpRequestMessage>())
            .Returns(httpResponse);

        var progressUpdates = new List<(long Downloaded, long? Total)>();
        DownloadProgressUpdateDelegate progressCallback = (downloaded, total) => progressUpdates.Add((downloaded, total));

        try {
            // Act
            var result = await _api.DownloadGamemodeAsync(lobbyId, destinationPath, progressCallback);

            // Assert
            Assert.That(result, Is.True, "Download should return true on success");
            Assert.That(File.Exists(destinationPath), Is.True, "File should be written to destination path");

            byte[] writtenBytes = await File.ReadAllBytesAsync(destinationPath);
            Assert.That(writtenBytes, Is.EqualTo(payload));

            Assert.That(progressUpdates, Is.Not.Empty, "Progress callback should be invoked at least once");
            Assert.That(progressUpdates[^1].Downloaded, Is.EqualTo(payload.Length));
            Assert.That(progressUpdates[^1].Total, Is.EqualTo(payload.Length));

            await _httpClient.Received(1).SendRequestAsync(Arg.Is<HttpRequestMessage>(
                req => req.Method == HttpMethod.Get &&
                       req.RequestUri!.ToString().Contains(HttpBattlegroundsServerAPI.DownloadGamemodeEndpoint) &&
                       req.RequestUri.ToString().Contains($"guid={lobbyId}") &&
                       req.Headers.Any(kvp => kvp.Key == "User-Agent")
            ));
        } finally {
            if (File.Exists(destinationPath)) {
                File.Delete(destinationPath);
            }
        }
    }

    [Test]
    public async Task DownloadGamemodeAsync_WithoutProgressCallback_StillDownloadsSuccessfully() {
        // Arrange
        string lobbyId = "test-lobby";
        string destinationPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.sga");
        byte[] payload = Encoding.UTF8.GetBytes("no-progress-callback-payload");

        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK) {
            Content = new ByteArrayContent(payload)
        };

        _httpClient.SendRequestAsync(Arg.Any<HttpRequestMessage>())
            .Returns(httpResponse);

        try {
            // Act
            var result = await _api.DownloadGamemodeAsync(lobbyId, destinationPath);

            // Assert
            Assert.That(result, Is.True);
            Assert.That(File.Exists(destinationPath), Is.True);

            byte[] writtenBytes = await File.ReadAllBytesAsync(destinationPath);
            Assert.That(writtenBytes, Is.EqualTo(payload));
        } finally {
            if (File.Exists(destinationPath)) {
                File.Delete(destinationPath);
            }
        }
    }

    [Test]
    public async Task DownloadGamemodeAsync_WhenServerError_ReturnsFalseAndDoesNotCreateFile() {
        // Arrange
        string lobbyId = "missing-lobby";
        string destinationPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.sga");

        _httpClient.SendRequestAsync(Arg.Any<HttpRequestMessage>())
            .Returns(new HttpResponseMessage(HttpStatusCode.NotFound));

        try {
            // Act
            var result = await _api.DownloadGamemodeAsync(lobbyId, destinationPath);

            // Assert
            Assert.That(result, Is.False, "Download should return false on server error");
            Assert.That(File.Exists(destinationPath), Is.False, "No file should be created on failure");
        } finally {
            if (File.Exists(destinationPath)) {
                File.Delete(destinationPath);
            }
        }
    }

}
