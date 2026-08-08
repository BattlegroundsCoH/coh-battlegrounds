using System.Net;

using Battlegrounds.Facades.API;
using Battlegrounds.Models;
using Battlegrounds.Services.Infrastructure;

using NSubstitute;

namespace Battlegrounds.Test.Services;

[TestOf(typeof(ImageCacheService))]
public class ImageCacheServiceTests {

    private const string Url = "https://api.example.com/api/news/resources/cover-id";
    
    private static readonly byte[] OnePixelPng =
        Convert.FromBase64String("iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mP8z8BQDwAEhQGAhKmMIQAAAABJRU5ErkJggg==");

    private IAsyncHttpClient _httpClient;
    private TestLogger<ImageCacheService> _logger;
    private Configuration _configuration;
    private string _cachePath;

    [SetUp]
    public void SetUp() {
        _httpClient = Substitute.For<IAsyncHttpClient>();
        _logger = new TestLogger<ImageCacheService>();
        _cachePath = Path.Combine(Path.GetTempPath(), $"bg-image-cache-tests-{Guid.NewGuid():N}");
        _configuration = new Configuration { ImageCachePath = _cachePath };
    }

    [TearDown]
    public void TearDown() {
        _httpClient.ClearReceivedCalls();
        _logger.Dispose();
        if (Directory.Exists(_cachePath)) {
            Directory.Delete(_cachePath, recursive: true);
        }
    }

    private ImageCacheService CreateService() => new(_logger, _httpClient, _configuration);

    private void GivenTheServerReturns(byte[] content)
        => _httpClient.SendRequestAsync(Arg.Any<HttpRequestMessage>())
            .Returns(new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent(content) });

    [Test]
    public async Task GetImageAsync_DownloadsAndDecodesTheImage() {

        // Arrange
        GivenTheServerReturns(OnePixelPng);
        var service = CreateService();

        // Act
        var result = await service.GetImageAsync(Url);

        // Assert
        using (Assert.EnterMultipleScope()) {
            Assert.That(result, Is.Not.Null, "The image should be decoded");
            Assert.That(result!.IsFrozen, Is.True, "The image must be frozen so it can cross to the UI thread");
        }

    }

    [Test]
    public async Task GetImageAsync_WhenAlreadyInMemory_DoesNotDownloadAgain() {

        // Arrange
        GivenTheServerReturns(OnePixelPng);
        var service = CreateService();

        // Act
        await service.GetImageAsync(Url);
        await service.GetImageAsync(Url);

        // Assert
        await _httpClient.Received(1).SendRequestAsync(Arg.Any<HttpRequestMessage>());

    }

    [Test]
    public async Task GetImageAsync_WhenAskedConcurrently_DownloadsOnce() {

        // Arrange — nine tiles binding at once must produce one download, not nine
        GivenTheServerReturns(OnePixelPng);
        var service = CreateService();

        // Act
        await Task.WhenAll(Enumerable.Range(0, 9).Select(_ => service.GetImageAsync(Url)));

        // Assert
        await _httpClient.Received(1).SendRequestAsync(Arg.Any<HttpRequestMessage>());

    }

    [Test]
    public async Task GetImageAsync_ServesFromDiskAcrossInstances() {

        // Arrange — this is the point of the disk tier: covers survive a restart
        GivenTheServerReturns(OnePixelPng);
        await CreateService().GetImageAsync(Url);
        _httpClient.ClearReceivedCalls();

        // Act
        var result = await CreateService().GetImageAsync(Url);

        // Assert
        Assert.That(result, Is.Not.Null, "The cached image should be served from disk");
        await _httpClient.DidNotReceive().SendRequestAsync(Arg.Any<HttpRequestMessage>());

    }

    [Test]
    public async Task GetImageAsync_WhenTheCachedFileIsCorrupt_RedownloadsIt() {

        // Arrange — a half-written file from a previous crash must not blank the tile forever
        GivenTheServerReturns(OnePixelPng);
        await CreateService().GetImageAsync(Url);
        _httpClient.ClearReceivedCalls();
        foreach (string file in Directory.GetFiles(_cachePath)) {
            File.WriteAllBytes(file, [0x00, 0x01, 0x02]);
        }

        // Act
        var result = await CreateService().GetImageAsync(Url);

        // Assert
        Assert.That(result, Is.Not.Null, "The image should be re-downloaded");
        await _httpClient.Received(1).SendRequestAsync(Arg.Any<HttpRequestMessage>());

    }

    [Test]
    public async Task GetImageAsync_WhenTheDownloadFails_ReturnsNull() {

        // Arrange
        _httpClient.SendRequestAsync(Arg.Any<HttpRequestMessage>())
            .Returns(new HttpResponseMessage(HttpStatusCode.NotFound));
        var service = CreateService();

        // Act
        var result = await service.GetImageAsync(Url);

        // Assert
        Assert.That(result, Is.Null, "A failed download should yield null rather than throwing");

    }

    [Test]
    public async Task GetImageAsync_WhenTheContentIsNotAnImage_ReturnsNullAndCachesNothing() {

        // Arrange
        GivenTheServerReturns([0x00, 0x01, 0x02]);
        var service = CreateService();

        // Act
        var result = await service.GetImageAsync(Url);

        // Assert
        using (Assert.EnterMultipleScope()) {
            Assert.That(result, Is.Null, "Undecodable content should yield null");
            Assert.That(Directory.Exists(_cachePath) && Directory.GetFiles(_cachePath).Length > 0, Is.False,
                "Undecodable content should not be written to the cache");
        }

    }

    [Test]
    public async Task GetImageAsync_AfterAFailure_TriesAgainOnTheNextCall() {

        // Arrange — a transient outage must not blank the tile until the app restarts
        _httpClient.SendRequestAsync(Arg.Any<HttpRequestMessage>())
            .Returns(new HttpResponseMessage(HttpStatusCode.ServiceUnavailable));
        var service = CreateService();
        await service.GetImageAsync(Url);

        // Act
        GivenTheServerReturns(OnePixelPng);
        var result = await service.GetImageAsync(Url);

        // Assert
        Assert.That(result, Is.Not.Null, "The retry should succeed once the server recovers");

    }

    [Test]
    public async Task GetImageAsync_WhenTheUrlIsEmpty_ReturnsNullWithoutRequesting() {

        // Act
        var result = await CreateService().GetImageAsync(string.Empty);

        // Assert
        Assert.That(result, Is.Null, "An empty URL should yield null");
        await _httpClient.DidNotReceive().SendRequestAsync(Arg.Any<HttpRequestMessage>());

    }

}
