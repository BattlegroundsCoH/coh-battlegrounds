using System.Windows.Media;

namespace Battlegrounds.Services;

/// <summary>
/// Loads images from remote URLs, caching them in memory and on disk.
/// </summary>
/// <remarks>Remote images cannot go through a converter at all — a converter cannot await — so consumers hold
/// the resulting <see cref="ImageSource"/> on their view-model and raise a change notification when
/// it arrives.</remarks>
public interface IImageCacheService {

    /// <summary>
    /// Gets the image at the specified URL, from the cache if it is there and over the network if not.
    /// </summary>
    /// <param name="url">The absolute URL of the image.</param>
    /// <param name="ct">A token to cancel the download.</param>
    /// <returns>The decoded, frozen image, or <see langword="null"/> if it could not be obtained.</returns>
    Task<ImageSource?> GetImageAsync(string url, CancellationToken ct = default);

}
