using System.Net;
using System.Text;

using Battlegrounds.Facades.API;
using Battlegrounds.Models;

using NSubstitute;

namespace Battlegrounds.Test.Facades.API;

[TestOf(typeof(HttpBattlegroundsWebAPI))]
public class HttpBattlegroundsWebAPITests {

    private HttpBattlegroundsWebAPI _api;
    private IAsyncHttpClient _httpClient;
    private TestLogger<HttpBattlegroundsWebAPI> _logger;
    private Configuration _configuration;

    private string BaseUrl => _configuration.API.BaseUrl.TrimEnd('/');

    [SetUp]
    public void SetUp() {
        _httpClient = Substitute.For<IAsyncHttpClient>();
        _logger = new TestLogger<HttpBattlegroundsWebAPI>();
        _configuration = new Configuration();
        _api = new HttpBattlegroundsWebAPI(_logger, _httpClient, _configuration);
    }

    [TearDown]
    public void TearDown() {
        _httpClient.ClearReceivedCalls();
        _logger.Dispose();
    }

    private static HttpResponseMessage JsonResponse(string json) => new(HttpStatusCode.OK) {
        Content = new StringContent(json, Encoding.UTF8, "application/json")
    };

    [Test]
    public async Task GetLatestNewsAsync_WhenFeedHasEntries_ReturnsThem() {

        // Arrange
        var expectedRequestUri = $"{BaseUrl}{HttpBattlegroundsWebAPI.LatestNewsEndpoint}";
        _httpClient.SendRequestAsync(Arg.Any<HttpRequestMessage>()).Returns(JsonResponse("""
            [
              {
                "id": "3f2a", "slug": "patch-1-2", "title": "Patch 1.2", "description": "Balance pass",
                "category": "Patch", "author": "Ragnar", "authorRole": "Developer", "isFeatured": true,
                "publishedAt": "2026-07-19T10:00:00Z", "createdAt": "2026-07-18T09:00:00Z",
                "resources": ["cover-id", "second-id"]
              }
            ]
            """));

        // Act
        var result = await _api.GetLatestNewsAsync();

        // Assert
        using (Assert.EnterMultipleScope()) {
            Assert.That(result, Has.Count.EqualTo(1), "One entry should be returned");
            Assert.That(result[0].Slug, Is.EqualTo("patch-1-2"), "Slug should be deserialized");
            Assert.That(result[0].Title, Is.EqualTo("Patch 1.2"), "Title should be deserialized");
            Assert.That(result[0].IsFeatured, Is.True, "IsFeatured should be deserialized");
            Assert.That(result[0].Resources, Is.EqualTo(new[] { "cover-id", "second-id" }), "Resources should be deserialized in order");
        }

        await _httpClient.Received(1).SendRequestAsync(Arg.Is<HttpRequestMessage>(
            req => req.Method == HttpMethod.Get && req.RequestUri!.ToString() == expectedRequestUri
        ));

    }

    [Test]
    public async Task GetLatestNewsAsync_WhenRequestFails_ReturnsEmptyList() {

        // Arrange
        _httpClient.SendRequestAsync(Arg.Any<HttpRequestMessage>())
            .Returns(new HttpResponseMessage(HttpStatusCode.ServiceUnavailable));

        // Act
        var result = await _api.GetLatestNewsAsync();

        // Assert
        Assert.That(result, Is.Empty, "A failed request should yield no entries rather than throwing");

    }

    [Test]
    public async Task GetLatestNewsAsync_WhenResponseIsNotJson_ReturnsEmptyList() {

        // Arrange — a proxy or error page returning 200 with HTML must not take the app down
        _httpClient.SendRequestAsync(Arg.Any<HttpRequestMessage>())
            .Returns(JsonResponse("<html>not json</html>"));

        // Act
        var result = await _api.GetLatestNewsAsync();

        // Assert
        Assert.That(result, Is.Empty, "Undeserializable content should yield no entries rather than throwing");

    }

    [Test]
    public async Task GetNewsPageAsync_RequestsTheRequestedPage_AndReturnsIt() {

        // Arrange
        var expectedRequestUri = $"{BaseUrl}{HttpBattlegroundsWebAPI.NewsPageEndpoint}?page=2&pageSize=9";
        _httpClient.SendRequestAsync(Arg.Any<HttpRequestMessage>()).Returns(JsonResponse("""
            {
              "items": [
                {
                  "id": "3f2a", "slug": "patch-1-2", "title": "Patch 1.2", "description": "",
                  "category": "Patch", "author": "Ragnar", "authorRole": "Developer", "isFeatured": false,
                  "publishedAt": "2026-07-19T10:00:00Z", "createdAt": "2026-07-18T09:00:00Z",
                  "resources": []
                }
              ],
              "page": 2, "pageSize": 9, "total": 27, "hasMore": true
            }
            """));

        // Act
        var result = await _api.GetNewsPageAsync(2, 9);

        // Assert
        using (Assert.EnterMultipleScope()) {
            Assert.That(result, Is.Not.Null, "A page should be returned");
            Assert.That(result!.Page, Is.EqualTo(2), "Page should be deserialized");
            Assert.That(result.Total, Is.EqualTo(27), "Total should be deserialized");
            Assert.That(result.HasMore, Is.True, "HasMore should be deserialized");
            Assert.That(result.Items, Has.Count.EqualTo(1), "Items should be deserialized");
        }

        await _httpClient.Received(1).SendRequestAsync(Arg.Is<HttpRequestMessage>(
            req => req.Method == HttpMethod.Get && req.RequestUri!.ToString() == expectedRequestUri
        ));

    }

    [Test]
    public async Task GetNewsPageAsync_WhenRequestFails_ReturnsNull() {

        // Arrange
        _httpClient.SendRequestAsync(Arg.Any<HttpRequestMessage>())
            .Returns(new HttpResponseMessage(HttpStatusCode.RequestTimeout));

        // Act
        var result = await _api.GetNewsPageAsync(1, 9);

        // Assert
        Assert.That(result, Is.Null, "A failed request should yield no page rather than throwing");

    }

    [Test]
    public void GetResourceUrl_UsesTheUnversionedResourceRoute() {

        // Act
        string url = _api.GetResourceUrl("cover-id");

        // Assert — the route is unversioned on purpose: the API stores this exact path inside
        // article markdown, so it has to stay stable across API versions.
        Assert.That(url, Is.EqualTo($"{BaseUrl}/api/news/resources/cover-id"), "Resource URL should target the unversioned route");

    }

    [Test]
    public async Task DownloadResourceAsync_WhenResourceExists_ReturnsBytes() {

        // Arrange
        byte[] content = [0x89, 0x50, 0x4E, 0x47];
        var expectedRequestUri = $"{BaseUrl}{HttpBattlegroundsWebAPI.NewsResourceEndpoint}/cover-id";
        _httpClient.SendRequestAsync(Arg.Any<HttpRequestMessage>())
            .Returns(new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent(content) });

        // Act
        var result = await _api.DownloadResourceAsync("cover-id");

        // Assert
        Assert.That(result, Is.EqualTo(content), "The resource content should be returned verbatim");
        await _httpClient.Received(1).SendRequestAsync(Arg.Is<HttpRequestMessage>(
            req => req.Method == HttpMethod.Get && req.RequestUri!.ToString() == expectedRequestUri
        ));

    }

    [Test]
    public async Task DownloadResourceAsync_WhenRequestFails_ReturnsNull() {

        // Arrange
        _httpClient.SendRequestAsync(Arg.Any<HttpRequestMessage>())
            .Returns(new HttpResponseMessage(HttpStatusCode.NotFound));

        // Act
        var result = await _api.DownloadResourceAsync("missing-id");

        // Assert
        Assert.That(result, Is.Null, "A failed download should yield null rather than throwing");

    }

    #region Browser sign-in

    /// <summary>
    /// A facade whose poll runs fast enough for a test to wait on.
    /// </summary>
    private HttpBattlegroundsWebAPI FastPollingApi(TimeSpan? budget = null)
        => new(_logger, _httpClient, _configuration, TimeSpan.FromMilliseconds(5), budget ?? TimeSpan.FromSeconds(2));

    private const string SuccessPayload = """
        {
          "token": "header.payload.signature", "refreshToken": "refresh-1", "expiresIn": 3600,
          "expiresAt": "2026-08-09T12:00:00Z",
          "user": { "bgId": "bg-1", "username": "ragnar", "displayName": "Ragnar" }
        }
        """;

    [Test]
    public async Task StartAuthAsync_WithAReturnUrl_EscapesItIntoTheQuery() {

        // Arrange
        var returnUrl = "http://127.0.0.1:54321/auth/callback";
        var expectedRequestUri = $"{BaseUrl}/auth/v1/discord/start?returnUrl=http%3A%2F%2F127.0.0.1%3A54321%2Fauth%2Fcallback";
        _httpClient.SendRequestAsync(Arg.Any<HttpRequestMessage>()).Returns(JsonResponse("""
            { "loginSessionId": "session-1", "authLink": "https://discord.com/oauth2", "verifier": "v-1" }
            """));

        // Act
        var result = await _api.StartAuthAsync(AuthProvider.Discord, returnUrl);

        // Assert
        Assert.That(result?.SessionId, Is.EqualTo("session-1"), "The login session should be returned");
        await _httpClient.Received(1).SendRequestAsync(Arg.Is<HttpRequestMessage>(
            req => req.Method == HttpMethod.Get && req.RequestUri!.ToString() == expectedRequestUri
        ));

    }

    [Test]
    public async Task StartAuthAsync_WithNoReturnUrl_SendsNoQueryString() {

        // Arrange
        var expectedRequestUri = $"{BaseUrl}/auth/v1/steam/start";
        _httpClient.SendRequestAsync(Arg.Any<HttpRequestMessage>()).Returns(JsonResponse("""
            { "loginSessionId": "session-1", "authLink": "https://steamcommunity.com/openid", "verifier": "v-1" }
            """));

        // Act
        await _api.StartAuthAsync(AuthProvider.Steam);

        // Assert
        await _httpClient.Received(1).SendRequestAsync(Arg.Is<HttpRequestMessage>(
            req => req.RequestUri!.ToString() == expectedRequestUri
        ));

    }

    /// <summary>
    /// BaseUrl is hand-edited in config.json. A trailing slash there used to reach these two endpoints unmodified
    /// and produce a doubled separator, which the API answers with a 404 that reads as an auth outage.
    /// </summary>
    [Test]
    public async Task StartAuthAsync_WhenBaseUrlHasATrailingSlash_DoesNotDoubleTheSeparator() {

        // Arrange
        _configuration.API.BaseUrl = "https://api.test.cohbattlegrounds.com/";
        _httpClient.SendRequestAsync(Arg.Any<HttpRequestMessage>()).Returns(JsonResponse("""
            { "loginSessionId": "session-1", "authLink": "https://discord.com/oauth2", "verifier": "v-1" }
            """));

        // Act
        await _api.StartAuthAsync(AuthProvider.Discord);

        // Assert
        await _httpClient.Received(1).SendRequestAsync(Arg.Is<HttpRequestMessage>(
            req => req.RequestUri!.ToString() == "https://api.test.cohbattlegrounds.com/auth/v1/discord/start"
        ));

    }

    [Test]
    public async Task EndAuthAsync_WhenTheSessionCompletes_ReturnsTheTokens() {

        // Arrange
        _httpClient.SendRequestAsync(Arg.Any<HttpRequestMessage>())
            .Returns(new HttpResponseMessage(HttpStatusCode.Accepted) { Content = new StringContent("""{"status":"Pending"}""") },
                     JsonResponse(SuccessPayload));

        // Act
        var result = await FastPollingApi().EndAuthAsync(AuthProvider.Discord, "session-1", "verifier-1");

        // Assert
        using (Assert.EnterMultipleScope()) {
            Assert.That(result.Outcome, Is.EqualTo(AuthStatusOutcome.Success), "A payload carrying a token is a success");
            Assert.That(result.Response?.Token, Is.EqualTo("header.payload.signature"), "The access token should be deserialized");
            Assert.That(result.Response?.User.Id, Is.EqualTo("bg-1"), "The user should be deserialized from bgId");
        }

    }

    [Test]
    public async Task EndAuthAsync_SendsTheSessionAndVerifier() {

        // Arrange
        var expectedRequestUri = $"{BaseUrl}/auth/v1/discord/status?id=session-1&verifier=verifier-1";
        _httpClient.SendRequestAsync(Arg.Any<HttpRequestMessage>()).Returns(JsonResponse(SuccessPayload));

        // Act
        await FastPollingApi().EndAuthAsync(AuthProvider.Discord, "session-1", "verifier-1");

        // Assert
        await _httpClient.Received(1).SendRequestAsync(Arg.Is<HttpRequestMessage>(
            req => req.Method == HttpMethod.Get && req.RequestUri!.ToString() == expectedRequestUri
        ));

    }

    /// <summary>
    /// A refused sign-in arrives as 200 OK once the API has accepted a return URL, discriminated only by a status
    /// member. Deserializing it as a success yields a null token, which used to surface to the user as
    /// "Token cannot be null or empty".
    /// </summary>
    [Test]
    public async Task EndAuthAsync_WhenTheStatusIsFailed_ReturnsFailedWithTheCodeAndDescription() {

        // Arrange
        _httpClient.SendRequestAsync(Arg.Any<HttpRequestMessage>()).Returns(JsonResponse("""
            { "status": "Failed", "code": "Auth.Discord.Cancelled", "description": "The sign-in was cancelled." }
            """));

        // Act
        var result = await FastPollingApi().EndAuthAsync(AuthProvider.Discord, "session-1", "verifier-1");

        // Assert
        using (Assert.EnterMultipleScope()) {
            Assert.That(result.Outcome, Is.EqualTo(AuthStatusOutcome.Failed), "A status of Failed is not a success despite the 200");
            Assert.That(result.Code, Is.EqualTo("Auth.Discord.Cancelled"), "The error code should be carried through for logging");
            Assert.That(result.Description, Is.EqualTo("The sign-in was cancelled."), "The description is shown to the user");
            Assert.That(result.Response, Is.Null, "A refusal carries no tokens");
        }

    }

    [Test]
    public async Task EndAuthAsync_WhenTheStatusIsMergeRequired_ReturnsMergeRequired() {

        // Arrange
        _httpClient.SendRequestAsync(Arg.Any<HttpRequestMessage>()).Returns(JsonResponse("""
            {
              "status": "MergeRequired", "provider": "discord", "mergeToken": "merge-1",
              "otherAccount": { "username": "ragnar", "displayName": "Ragnar", "createdAt": "2026-01-01T00:00:00Z", "hasGameData": true }
            }
            """));

        // Act
        var result = await FastPollingApi().EndAuthAsync(AuthProvider.Discord, "session-1", "verifier-1");

        // Assert
        using (Assert.EnterMultipleScope()) {
            Assert.That(result.Outcome, Is.EqualTo(AuthStatusOutcome.MergeRequired), "A merge offer is neither a success nor a failure");
            Assert.That(result.Response, Is.Null, "A merge offer carries no tokens");
        }

    }

    /// <summary>
    /// The loop catches broadly so a transient error does not end the wait. That catch must not also swallow the
    /// cancellation an aborted delay throws, or a cancel would spin out the whole budget logging an error per pass.
    /// </summary>
    [Test]
    public async Task EndAuthAsync_WhenCancelled_StopsImmediatelyInsteadOfLoopingThroughTheBudget() {

        // Arrange
        _httpClient.SendRequestAsync(Arg.Any<HttpRequestMessage>())
            .Returns(_ => new HttpResponseMessage(HttpStatusCode.Accepted) { Content = new StringContent("""{"status":"Pending"}""") });
        using CancellationTokenSource cts = new();

        // Act
        Task<AuthStatusResult> pending = FastPollingApi(TimeSpan.FromMinutes(5)).EndAuthAsync(AuthProvider.Discord, "session-1", "verifier-1", cts.Token);
        await cts.CancelAsync();
        var result = await pending;
        int callsAtCancellation = _httpClient.ReceivedCalls().Count();
        await Task.Delay(100);

        // Assert
        using (Assert.EnterMultipleScope()) {
            Assert.That(result.Outcome, Is.EqualTo(AuthStatusOutcome.Cancelled), "Cancelling should be reported as such, not as a timeout or a failure");
            Assert.That(_httpClient.ReceivedCalls().Count(), Is.EqualTo(callsAtCancellation), "No further requests should be sent once the wait has returned");
        }

    }

    [Test]
    public async Task EndAuthAsync_WhenTheSessionNeverResolves_ReturnsTimedOutAtTheBudget() {

        // Arrange
        _httpClient.SendRequestAsync(Arg.Any<HttpRequestMessage>())
            .Returns(_ => new HttpResponseMessage(HttpStatusCode.Accepted) { Content = new StringContent("""{"status":"Pending"}""") });

        // Act
        var result = await FastPollingApi(TimeSpan.FromMilliseconds(150)).EndAuthAsync(AuthProvider.Discord, "session-1", "verifier-1");

        // Assert
        Assert.That(result.Outcome, Is.EqualTo(AuthStatusOutcome.TimedOut), "A session that never resolves should time out rather than hang");

    }

    /// <summary>
    /// The API answers 404 both for a session it cannot see yet and for one that has expired, deliberately
    /// indistinguishably. Treating it as terminal would end a sign-in that had not started resolving.
    /// </summary>
    [Test]
    public async Task EndAuthAsync_WhenTheSessionIsNotFoundYet_KeepsWaiting() {

        // Arrange
        _httpClient.SendRequestAsync(Arg.Any<HttpRequestMessage>())
            .Returns(new HttpResponseMessage(HttpStatusCode.NotFound) { Content = new StringContent("""{"status":"Expired or Invalid Session"}""") },
                     JsonResponse(SuccessPayload));

        // Act
        var result = await FastPollingApi().EndAuthAsync(AuthProvider.Discord, "session-1", "verifier-1");

        // Assert
        Assert.That(result.Outcome, Is.EqualTo(AuthStatusOutcome.Success), "A 404 should be waited out, not treated as the end of the session");

    }

    #endregion

}
