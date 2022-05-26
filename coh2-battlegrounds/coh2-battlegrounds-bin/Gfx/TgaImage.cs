namespace Battlegrounds.Gfx;

/// <summary>
/// Enum representing the embedding of the image pixel data.
/// </summary>
public enum TgaImageDatatype : byte {

    /// <summary>
    /// No image data.
    /// </summary>
    NO_IMAGE_DATA = 0,

    /// <summary>
    /// Uncompressed colour map.
    /// </summary>
    UNCOMPRESSED_COLOUR_MAP = 1,

    /// <summary>
    /// Uncompressed RGB(A) map.
    /// </summary>
    UNCOMPRESSED_RGB = 2,

    /// <summary>
    /// Uncompressed black and white map.
    /// </summary>
    UNCOMPRESSED_BLACK_WHITE = 3,

    /// <summary>
    /// Run length encoded colour map.
    /// </summary>
    RUN_LENGTH_ENCODED_COLOUR_MAP = 9,

    /// <summary>
    /// Run length encoded RGB(A) map.
    /// </summary>
    RUN_LENGTH_ENCODED_RGB = 10,

    /// <summary>
    /// Compressed black and white.
    /// </summary>
    COMPRESSED_BLACK_WHITE = 11,

    /// <summary>
    /// Compressed colour data.
    /// </summary>
    COMPRESSED_COLOUR_MAP_HUFFMAN = 32,

    /// <summary>
    /// Compressed Huffman 4PASS representation.
    /// </summary>
    COMPRESSED_COLOUR_MAP_HUFFMAN_4PASS = 33

}

/// <summary>
/// Class representing a <see cref="TgaImage"/>.
/// </summary>
public sealed class TgaImage {

    private readonly byte[] m_pixelData;

    /// <summary>
    /// Get the width of the image.
    /// </summary>
    public int Width { get; }

    /// <summary>
    /// Get the height of the image.
    /// </summary>
    public int Height { get; }

    /// <summary>
    /// Get the pixel resolution along the X-axis.
    /// </summary>
    public int DPIX { get; }

    /// <summary>
    /// Get the pixel resolution along the Y-axis.
    /// </summary>
    public int DPIY { get; }

    /// <summary>
    /// Get the image data type.
    /// </summary>
    public TgaImageDatatype ImageDatatype { get; }

    /// <summary>
    /// Get the pits per pixel.
    /// </summary>
    public int BIP { get; }

    /// <summary>
    /// Get if the image is 32-bit (RGBA).
    /// </summary>
    public bool Is32Bit => this.BIP is 32;

    /// <summary>
    /// Get the stride of the image.
    /// </summary>
    public int Stride => (this.Width * this.BIP) / 8;

    internal TgaImage(int width, int height, int dpix, int dpiy, byte[] data, TgaImageDatatype imageDatatype, int bip) {
        this.Width = width;
        this.Height = height;
        this.DPIX = dpix;
        this.DPIY = dpiy;
        this.ImageDatatype = imageDatatype;
        this.BIP = bip;
        this.m_pixelData = data;
    }

    /// <summary>
    /// Get the pixel data for the image.
    /// </summary>
    /// <returns>A byte array containing the parsed RGB(A) data.</returns>
    public byte[] GetPixelData() => this.m_pixelData;

}
