using Battlegrounds.Facades.API;
using Battlegrounds.Models;
using Battlegrounds.Services.Data;

using Microsoft.Extensions.Time.Testing;

using NSubstitute;

namespace Battlegrounds.Test.Services;

[TestOf(typeof(NewsService))]
public class NewsServiceTests {

    private NewsService _service;
    private IBattlegroundsWebAPI _webApi;
    private TestLogger<NewsService> _logger;
    private Configuration _configuration;
    private FakeTimeProvider _timeProvider;

    [SetUp]
    public void SetUp() {
        _webApi = Substitute.For<IBattlegroundsWebAPI>();
        _webApi.GetResourceUrl(Arg.Any<string>()).Returns(x => $"https://api.example.com/api/news/resources/{x.Arg<string>()}");
        _logger = new TestLogger<NewsService>();
        _configuration = new Configuration { WebsiteUrl = "https://cohbattlegrounds.com" };
        _timeProvider = new FakeTimeProvider(new DateTimeOffset(2026, 8, 8, 12, 0, 0, TimeSpan.Zero));
        _service = new NewsService(_logger, _webApi, _configuration, _timeProvider);
    }

    [TearDown]
    public void TearDown() {
        _webApi.ClearReceivedCalls();
        _logger.Dispose();
    }

    private static NewsPreviewResponse Preview(
        string slug = "patch-1-2",
        DateTime? publishedAt = null,
        DateTime? createdAt = null,
        params string[] resources) => new(
            Id: "3f2a",
            Slug: slug,
            Title: "Patch 1.2",
            Description: "Balance pass",
            Category: "Patch",
            Author: "Ragnar",
            AuthorRole: "Developer",
            IsFeatured: false,
            PublishedAt: publishedAt,
            CreatedAt: createdAt ?? new DateTime(2026, 7, 18, 9, 0, 0),
            Resources: resources);

    [Test]
    public async Task GetLatestAsync_BuildsTheCoverUrlFromTheFirstResource() {

        // Arrange — the API returns resource ids, never image URLs, and index 0 is the cover
        _webApi.GetLatestNewsAsync().Returns([Preview(resources: ["cover-id", "second-id"])]);

        // Act
        var result = await _service.GetLatestAsync(3);

        // Assert
        Assert.That(result[0].CoverImageUrl, Is.EqualTo("https://api.example.com/api/news/resources/cover-id"),
            "The cover should be built from the first resource id");

    }

    [Test]
    public async Task GetLatestAsync_WhenTheEntryHasNoResources_HasNoCover() {

        // Arrange
        _webApi.GetLatestNewsAsync().Returns([Preview()]);

        // Act
        var result = await _service.GetLatestAsync(3);

        // Assert
        Assert.That(result[0].CoverImageUrl, Is.Null, "An entry with no resources should have no cover");

    }

    [Test]
    public async Task GetLatestAsync_BuildsTheArticleUrlFromTheWebsiteHost() {

        // Arrange
        _webApi.GetLatestNewsAsync().Returns([Preview(slug: "patch 1.2 & more")]);

        // Act
        var result = await _service.GetLatestAsync(3);

        // Assert
        Assert.That(result[0].ArticleUrl, Is.EqualTo("https://cohbattlegrounds.com/news/patch%201.2%20%26%20more"),
            "The article URL should point at the website with the slug escaped");

    }

    [Test]
    public async Task GetLatestAsync_TreatsAnUnspecifiedTimestampAsUtcAndConvertsItToLocalTime() {

        // Arrange — a timestamp that reached us without a kind. The API has stamped a Z since its
        // UtcDateTimeJsonConverter shipped, so this no longer describes the live wire format, but
        // an unmarked value is still the one case that has to be *told* it is UTC
        var published = new DateTime(2026, 7, 19, 10, 0, 0, DateTimeKind.Unspecified);
        _webApi.GetLatestNewsAsync().Returns([Preview(publishedAt: published)]);

        // Act
        var result = await _service.GetLatestAsync(3);

        // Assert
        var expected = DateTime.SpecifyKind(published, DateTimeKind.Utc).ToLocalTime();
        using (Assert.EnterMultipleScope()) {
            Assert.That(result[0].PublishedAt, Is.EqualTo(expected), "The timestamp should be converted from UTC to local time");
            Assert.That(result[0].PublishedAt.Kind, Is.EqualTo(DateTimeKind.Local), "The timestamp should be local");
        }

    }

    [Test]
    public async Task GetLatestAsync_ConvertsAUtcTimestampToLocalTime() {

        // Arrange — what the API actually sends today: "…Z", which System.Text.Json reads as Utc
        var published = new DateTime(2026, 7, 19, 10, 0, 0, DateTimeKind.Utc);
        _webApi.GetLatestNewsAsync().Returns([Preview(publishedAt: published)]);

        // Act
        var result = await _service.GetLatestAsync(3);

        // Assert
        using (Assert.EnterMultipleScope()) {
            Assert.That(result[0].PublishedAt, Is.EqualTo(published.ToLocalTime()), "The timestamp should be converted from UTC to local time");
            Assert.That(result[0].PublishedAt.Kind, Is.EqualTo(DateTimeKind.Local), "The timestamp should be local");
        }

    }

    [Test]
    public async Task GetLatestAsync_DoesNotConvertAnAlreadyLocalTimestampASecondTime() {

        // Arrange — the shape an offset form ("…+00:00") produces: System.Text.Json takes its
        // offset branch and hands back a value that is *already* local. Converting again would
        // add this machine's offset twice, putting every article in the future
        var published = new DateTimeOffset(2026, 7, 19, 10, 0, 0, TimeSpan.Zero).LocalDateTime;
        _webApi.GetLatestNewsAsync().Returns([Preview(publishedAt: published)]);

        // Act
        var result = await _service.GetLatestAsync(3);

        // Assert
        using (Assert.EnterMultipleScope()) {
            Assert.That(result[0].PublishedAt, Is.EqualTo(published), "An already-local timestamp should pass through unchanged");
            Assert.That(result[0].PublishedAt, Is.LessThanOrEqualTo(DateTime.Now), "A double conversion would place it in the future");
        }

    }

    [Test]
    public async Task GetLatestAsync_WhenPublishedAtIsMissing_FallsBackToCreatedAt() {

        // Arrange
        var created = new DateTime(2026, 7, 18, 9, 0, 0);
        _webApi.GetLatestNewsAsync().Returns([Preview(publishedAt: null, createdAt: created)]);

        // Act
        var result = await _service.GetLatestAsync(3);

        // Assert
        Assert.That(result[0].PublishedAt, Is.EqualTo(DateTime.SpecifyKind(created, DateTimeKind.Utc).ToLocalTime()),
            "An unpublished entry should fall back to its creation date");

    }

    [Test]
    public async Task GetLatestAsync_ReordersByPublicationDate() {

        // Arrange — the API orders by created_at, the website by published_at
        _webApi.GetLatestNewsAsync().Returns([
            Preview(slug: "older", publishedAt: new DateTime(2026, 7, 1, 12, 0, 0), createdAt: new DateTime(2026, 7, 20, 12, 0, 0)),
            Preview(slug: "newer", publishedAt: new DateTime(2026, 7, 25, 12, 0, 0), createdAt: new DateTime(2026, 7, 10, 12, 0, 0))
        ]);

        // Act
        var result = await _service.GetLatestAsync(3);

        // Assert
        Assert.That(result.Select(x => x.Slug), Is.EqualTo(new[] { "newer", "older" }),
            "Entries should be ordered by publication date, not creation date");

    }

    [Test]
    public async Task GetLatestAsync_TakesNoMoreThanTheRequestedCount() {

        // Arrange
        _webApi.GetLatestNewsAsync().Returns([Preview(slug: "a"), Preview(slug: "b"), Preview(slug: "c")]);

        // Act
        var result = await _service.GetLatestAsync(2);

        // Assert
        Assert.That(result, Has.Count.EqualTo(2), "No more than the requested number of entries should be returned");

    }

    [Test]
    public async Task GetLatestAsync_WithinTheCacheWindow_DoesNotRefetch() {

        // Arrange
        _webApi.GetLatestNewsAsync().Returns([Preview()]);
        await _service.GetLatestAsync(3);

        // Act
        _timeProvider.Advance(TimeSpan.FromMinutes(4));
        await _service.GetLatestAsync(3);

        // Assert
        await _webApi.Received(1).GetLatestNewsAsync();

    }

    [Test]
    public async Task GetLatestAsync_OnceTheCacheHasExpired_Refetches() {

        // Arrange
        _webApi.GetLatestNewsAsync().Returns([Preview()]);
        await _service.GetLatestAsync(3);

        // Act
        _timeProvider.Advance(TimeSpan.FromMinutes(6));
        await _service.GetLatestAsync(3);

        // Assert
        await _webApi.Received(2).GetLatestNewsAsync();

    }

    [Test]
    public async Task GetLatestAsync_WhenForced_RefetchesWithinTheCacheWindow() {

        // Arrange
        _webApi.GetLatestNewsAsync().Returns([Preview()]);
        await _service.GetLatestAsync(3);

        // Act
        await _service.GetLatestAsync(3, forceRefresh: true);

        // Assert
        await _webApi.Received(2).GetLatestNewsAsync();

    }

    [Test]
    public async Task GetLatestAsync_WhenAFetchFails_KeepsTheEntriesItAlreadyHad() {

        // Arrange — a transient outage must not blank the dashboard
        _webApi.GetLatestNewsAsync().Returns([Preview(slug: "cached")]);
        await _service.GetLatestAsync(3);
        _webApi.GetLatestNewsAsync().Returns([]);

        // Act
        var result = await _service.GetLatestAsync(3, forceRefresh: true);

        // Assert
        Assert.That(result.Select(x => x.Slug), Is.EqualTo(new[] { "cached" }), "The previously fetched entries should be kept");

    }

    [Test]
    public async Task GetLatestAsync_WhenTheFirstFetchFails_ReturnsEmpty() {

        // Arrange
        _webApi.GetLatestNewsAsync().Returns([]);

        // Act
        var result = await _service.GetLatestAsync(3);

        // Assert
        Assert.That(result, Is.Empty, "With nothing cached a failed fetch should yield the empty state");

    }

    [Test]
    public async Task GetPageAsync_ReturnsThePageWithItsPagingState() {

        // Arrange
        _webApi.GetNewsPageAsync(2, 9).Returns(new PagedNewsResponse([Preview()], Page: 2, PageSize: 9, Total: 27, HasMore: true));

        // Act
        var result = await _service.GetPageAsync(2, 9);

        // Assert
        using (Assert.EnterMultipleScope()) {
            Assert.That(result.Page, Is.EqualTo(2), "Page should be carried through");
            Assert.That(result.Total, Is.EqualTo(27), "Total should be carried through");
            Assert.That(result.TotalPages, Is.EqualTo(3), "27 entries at 9 per page is three pages");
            Assert.That(result.HasMore, Is.True, "HasMore should be carried through");
            Assert.That(result.Items, Has.Count.EqualTo(1), "Items should be mapped");
        }

    }

    [Test]
    public async Task GetPageAsync_WhenTheRequestFails_ReturnsAnEmptyPage() {

        // Arrange
        _webApi.GetNewsPageAsync(Arg.Any<int>(), Arg.Any<int>()).Returns((PagedNewsResponse?)null);

        // Act
        var result = await _service.GetPageAsync(3, 9);

        // Assert
        using (Assert.EnterMultipleScope()) {
            Assert.That(result.Items, Is.Empty, "A failed request should yield no items");
            Assert.That(result.Page, Is.EqualTo(3), "The requested page should be reported back");
            Assert.That(result.TotalPages, Is.EqualTo(1), "An empty page still counts as one page");
            Assert.That(result.HasMore, Is.False, "There is nothing more to fetch");
        }

    }

    [Test]
    public async Task GetPageAsync_IsNotCached() {

        // Arrange
        _webApi.GetNewsPageAsync(1, 9).Returns(new PagedNewsResponse([Preview()], 1, 9, 1, false));

        // Act
        await _service.GetPageAsync(1, 9);
        await _service.GetPageAsync(1, 9);

        // Assert — the caller asked for this page, so it gets a fresh one
        await _webApi.Received(2).GetNewsPageAsync(1, 9);

    }

}
