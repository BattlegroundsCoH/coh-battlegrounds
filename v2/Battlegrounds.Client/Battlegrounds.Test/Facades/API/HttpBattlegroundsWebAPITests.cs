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

}
