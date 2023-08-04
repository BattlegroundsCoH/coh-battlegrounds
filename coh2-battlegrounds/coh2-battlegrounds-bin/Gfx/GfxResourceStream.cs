using System.IO;

namespace Battlegrounds.Gfx;

/// <summary>
/// Provides a set of methods for reading a GFX resource.
/// </summary>
public class GfxResourceStream : MemoryStream {

    /// <summary>
    /// Get the width of the resource image.
    /// </summary>
    public int ResourceImageWidth { get; }

    /// <summary>
    /// Get the height of the resource image.
    /// </summary>
    public int ResourceImageHeight { get; }

    /// <summary>
    /// Get the identifier of the resource.
    /// </summary>
    public string ResourceIdentifier { get; }

    /// <summary>
    /// Initialize a new <see cref="GfxResourceStream"/> class with stream data.
    /// </summary>
    /// <param name="identifier">The identifier of the contained <see cref="GfxResource"/>.</param>
    /// <param name="rawResourceBytes">The raw bytes to stream.</param>
    /// <param name="width">The width of the resource.</param>
    /// <param name="height">The height of the resource.</param>
    public GfxResourceStream(string identifier, byte[] rawResourceBytes, int width, int height) : base(rawResourceBytes) {
        this.ResourceIdentifier = identifier;
        this.ResourceImageHeight = height;
        this.ResourceImageWidth = width;
    }

}
