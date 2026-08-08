using System.Collections.Concurrent;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using Battlegrounds.Facades.API;
using Battlegrounds.Models;

using Microsoft.Extensions.Logging;

namespace Battlegrounds.Services.Infrastructure;

/// <summary>
/// Two-tier image cache: frozen bitmaps in memory over a directory of downloaded bytes on disk.
/// </summary>
/// <remarks>The disk tier is what lets the dashboard paint its news covers immediately on launch
/// instead of waiting on the network every session. It lives under <c>%AppData%</c> because
/// everything in it is disposable and re-downloadable.</remarks>
public sealed class ImageCacheService(
    ILogger<ImageCacheService> logger,
    IAsyncHttpClient asyncHttpClient,
    Configuration configuration) : IImageCacheService {

    private static readonly TimeSpan MaxCacheAge = TimeSpan.FromDays(30);

    /// <summary>The size the cache directory is trimmed back to, oldest file first.</summary>
    private const long MaxCacheBytes = 64L * 1024 * 1024;

    /// <summary>
    /// Keyed by URL, holding the in-flight <i>task</i> rather than its result: nine tiles binding at
    /// once must produce one download, not nine.
    /// </summary>
    private readonly ConcurrentDictionary<string, Task<ImageSource?>> _cache = new();

    private readonly ILogger<ImageCacheService> _logger = logger;
    private readonly IAsyncHttpClient _httpClient = asyncHttpClient;
    private readonly Configuration _configuration = configuration;

    private int _sweepStarted;

    public async Task<ImageSource?> GetImageAsync(string url, CancellationToken ct = default) {

        if (string.IsNullOrWhiteSpace(url)) {
            return null;
        }

        EnsureSweepStarted();

        // Task.Run rather than a bare call: callers are on the UI thread, so without it the disk
        // probe and — more expensively — the bitmap decode of every visible tile would run there.
        // Freezing the result is what makes handing it back across the boundary safe.
        Task<ImageSource?> load = _cache.GetOrAdd(url, key => Task.Run(() => LoadAsync(key, ct), CancellationToken.None));

        ImageSource? image;
        try {
            image = await load;
        } catch {
            Forget(url, load);
            throw;
        }

        if (image is null) {
            Forget(url, load);
        }

        return image;

    }

    /// <summary>
    /// Drops a cache entry, but only if it is still the exact task that was observed.
    /// </summary>
    private void Forget(string url, Task<ImageSource?> load)
        => ((ICollection<KeyValuePair<string, Task<ImageSource?>>>)_cache).Remove(new(url, load));

    private async Task<ImageSource?> LoadAsync(string url, CancellationToken ct)
        => await LoadFromDiskAsync(url, ct) ?? await DownloadAsync(url, ct);

    private async Task<ImageSource?> LoadFromDiskAsync(string url, CancellationToken ct) {

        string path = GetCacheFilePath(url);
        try {
            if (!File.Exists(path)) {
                return null;
            }
            byte[] content = await File.ReadAllBytesAsync(path, ct);
            ImageSource? image = Decode(content, url);
            if (image is null) {
                File.Delete(path);
                return null;
            }
            File.SetLastAccessTimeUtc(path, DateTime.UtcNow);
            return image;
        } catch (OperationCanceledException) {
            throw;
        } catch (Exception ex) {
            _logger.LogWarning(ex, "Failed to read cached image for {Url} from {Path}.", url, path);
            return null;
        }

    }

    private async Task<ImageSource?> DownloadAsync(string url, CancellationToken ct) {

        HttpRequestMessage request = new(HttpMethod.Get, url);
        HttpResponseMessage response = await _httpClient.SendRequestAsync(request);
        if (!response.IsSuccessStatusCode) {
            _logger.LogError("Failed to download image {Url}. Status code: {StatusCode}, Reason: {ReasonPhrase}", url, response.StatusCode, response.ReasonPhrase);
            return null;
        }

        byte[] content;
        try {
            content = await response.Content.ReadAsByteArrayAsync(ct);
        } catch (OperationCanceledException) {
            throw;
        } catch (Exception ex) {
            _logger.LogError(ex, "Failed to read the content of image {Url}.", url);
            return null;
        }

        ImageSource? image = Decode(content, url);
        if (image is null) {
            return null;
        }

        await WriteToDiskAsync(url, content, ct);
        return image;

    }

    private async Task WriteToDiskAsync(string url, byte[] content, CancellationToken ct) {

        string path = GetCacheFilePath(url);
        string temporaryPath = $"{path}.tmp";
        try {
            Directory.CreateDirectory(_configuration.ImageCachePath);
            await File.WriteAllBytesAsync(temporaryPath, content, ct);
            File.Move(temporaryPath, path, overwrite: true);
        } catch (Exception ex) {
            _logger.LogWarning(ex, "Failed to cache image {Url} at {Path}.", url, path);
            try {
                if (File.Exists(temporaryPath)) {
                    File.Delete(temporaryPath);
                }
            } catch (Exception cleanupException) {
                _logger.LogWarning(cleanupException, "Failed to clean up the temporary image cache file {Path}.", temporaryPath);
            }
        }

    }

    /// <summary>
    /// Decodes image bytes into a frozen <see cref="BitmapImage"/>.
    /// </summary>
    private ImageSource? Decode(byte[] content, string url) {
        try {
            using MemoryStream stream = new(content);
            BitmapImage bitmap = new();
            bitmap.BeginInit();
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.StreamSource = stream;
            bitmap.EndInit();
            bitmap.Freeze();
            return bitmap;
        } catch (Exception ex) {
            _logger.LogWarning(ex, "Failed to decode the image at {Url}.", url);
            return null;
        }
    }

    private string GetCacheFilePath(string url) {
        byte[] hash = SHA256.HashData(Encoding.UTF8.GetBytes(url));
        return Path.Combine(_configuration.ImageCachePath, Convert.ToHexStringLower(hash));
    }

    private void EnsureSweepStarted() {
        if (Interlocked.Exchange(ref _sweepStarted, 1) == 0) {
            _ = Task.Run(Sweep);
        }
    }

    /// <summary>
    /// Discards stale cache files and trims the directory back to <see cref="MaxCacheBytes"/>.
    /// </summary>
    private void Sweep() {
        try {

            DirectoryInfo directory = new(_configuration.ImageCachePath);
            if (!directory.Exists) {
                return;
            }

            DateTime cutoff = DateTime.UtcNow - MaxCacheAge;
            List<FileInfo> files = [];
            foreach (FileInfo file in directory.EnumerateFiles()) {
                if (file.LastAccessTimeUtc < cutoff) {
                    TryDelete(file);
                } else {
                    files.Add(file);
                }
            }

            long total = files.Sum(x => x.Length);
            if (total <= MaxCacheBytes) {
                return;
            }

            foreach (FileInfo file in files.OrderBy(x => x.LastAccessTimeUtc)) {
                if (total <= MaxCacheBytes) {
                    break;
                }
                long length = file.Length;
                if (TryDelete(file)) {
                    total -= length;
                }
            }

        } catch (Exception ex) {
            _logger.LogWarning(ex, "Failed to sweep the image cache at {Path}.", _configuration.ImageCachePath);
        }
    }

    private bool TryDelete(FileInfo file) {
        try {
            file.Delete();
            return true;
        } catch (Exception ex) {
            _logger.LogWarning(ex, "Failed to delete the cached image {Path}.", file.FullName);
            return false;
        }
    }

}
