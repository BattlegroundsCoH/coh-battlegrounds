using System.Net;
using System.Net.Http.Json;

using Battlegrounds.Facades.API;
using Battlegrounds.Models;
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
        var mockUser = new User { UserId = "test-user" };

        _userService.GetLocalUserAsync().Returns(mockUser);
        _userService.GetLocalUserTokenAsync().Returns("test-token");

        var httpResponse = new HttpResponseMessage(HttpStatusCode.OK);
        _httpClient.SendRequestAsync(Arg.Any<HttpRequestMessage>())
            .Returns(httpResponse);

        // Act
        var result = await _api.UploadCompanyAsync(companyId, faction, stream);

        // Assert
        Assert.That(result, Is.True, "Upload should return true on success");
        await _userService.Received(1).GetLocalUserAsync();
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
        var mockUser = new User { UserId = "test-user" };

        _userService.GetLocalUserAsync().Returns(mockUser);
        _userService.GetLocalUserTokenAsync().Returns("test-token");

        var httpResponse = new HttpResponseMessage(HttpStatusCode.InternalServerError);
        _httpClient.SendRequestAsync(Arg.Any<HttpRequestMessage>())
            .Returns(httpResponse);

        // Act
        var result = await _api.UploadCompanyAsync(companyId, faction, stream);

        // Assert
        Assert.That(result, Is.False, "Upload should return false on server error");
    }

    [Test]
    public async Task DeleteCompanyAsync_WhenSuccessful_ReturnsTrue() {
        
        // Arrange
        string companyId = "test-company";
        var mockUser = new User { UserId = "test-user" };
        
        _userService.GetLocalUserAsync().Returns(mockUser);
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
                   req.RequestUri!.ToString().Contains($"guid={companyId}") &&
                   req.RequestUri.ToString().Contains($"userId={mockUser.UserId}")
        ));

    }

    [Test]
    public async Task DeleteCompanyAsync_WhenServerError_ReturnsFalse() {
        
        // Arrange
        string companyId = "test-company";
        var mockUser = new User { UserId = "test-user" };
        
        _userService.GetLocalUserAsync().Returns(mockUser);
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
                   req.Content is JsonContent &&
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

}