using System;
using System.Runtime.CompilerServices;

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

    /// <summary>
    /// Get the pixel at (<paramref name="x"/>,<paramref name="y"/>).
    /// </summary>
    /// <param name="x">The x-position of the pixel.</param>
    /// <param name="y">The y-position of the pixel.</param>
    /// <returns>The pixel found at (<paramref name="x"/>,<paramref name="y"/>).</returns>
    /// <exception cref="ArgumentOutOfRangeException"/>
    /// <exception cref="IndexOutOfRangeException"/>
    public TgaPixel GetPixel(int x, int y) {

        // Verify X-coordinate
        if (x < 0 || x > this.Width)
            throw new ArgumentOutOfRangeException(nameof(x), $"Invalid X-position {x}. Must be greater than 0 and less than {this.Width}.");

        // Verify Y-coordinate
        if (y < 0 || y > this.Height)
            throw new ArgumentOutOfRangeException(nameof(y), $"Invalid Y-position {y}. Must be greater than 0 and less than {this.Height}.");

        // Compute byte position in bytemap
        int _x = x * (this.Is32Bit ? 4 : 3);
        int _y = y * this.Stride;
        
        // Compute RGB positions
        int blue = _y + _x;
        int green = _y + _x + 1;
        int red = _y + _x + 2;
        
        // Return pixel (if 32-bit also include the alpha channel, which follows the red channel)
        if (this.Is32Bit) {
            return new TgaPixel(this.m_pixelData[red], this.m_pixelData[green], this.m_pixelData[blue], this.m_pixelData[red+1]);
        } else {
            return new TgaPixel(this.m_pixelData[red], this.m_pixelData[green], this.m_pixelData[blue]);
        }

    }

    /// <summary>
    /// Get a <see cref="TgaPixel"/>[,] map from the internal byte representation.
    /// </summary>
    /// <returns>A <see cref="TgaPixel"/>[,] map representing the <see cref="TgaImage"/>.</returns>
    public unsafe TgaPixel[,] ToPixelMap() {

        // Create pixel map
        var pixelMap = new TgaPixel[this.Width, this.Height];

        // Get bytes per pixel
        int bpp = this.BIP / 8;

        // Loop over
        fixed (byte* pData = this.m_pixelData) {
            for (int y = 0; y < this.Height; y++) {
                int _y = y * this.Stride;
                for (int x = 0; x < this.Width; x++) {
                    int _x = x * bpp;
                    if (this.Is32Bit) {
                        pixelMap[x, y] = Unsafe.ReadUnaligned<TgaPixel>(pData + _y + _x);
                    } else {
                        pixelMap[x, y] = Unsafe.ReadUnaligned<TgaPixelRGB>(pData + _y + _x);
                    }
                }
            }
        }

        // Return
        return pixelMap;

    }

}
