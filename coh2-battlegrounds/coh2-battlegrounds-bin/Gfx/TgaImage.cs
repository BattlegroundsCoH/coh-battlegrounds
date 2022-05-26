namespace Battlegrounds.Gfx;

public enum TgaImageDatatype : byte {
    NO_IMAGE_DATA = 0,
    UNCOMPRESSED_COLOUR_MAP = 1,
    UNCOMPRESSED_RGB = 2,
    UNCOMPRESSED_BLACK_WHITE = 3,
    RUN_LENGTH_ENCODED_COLOUR_MAP = 9,
    RUN_LENGTH_ENCODED_RGB = 10,
    COMPRESSED_BLACK_WHITE = 11,
    COMPRESSED_COLOUR_MAP_HUFFMAN = 32,
    COMPRESSED_COLOUR_MAP_HUFFMAN_4PASS = 33
}

/// <summary>
/// 
/// </summary>
public class TgaImage {

    private readonly byte[] m_pixelData;

    public int Width { get; }

    public int Height { get; }

    public int DPIX { get; }

    public int DPIY { get; }

    public TgaImageDatatype ImageDatatype { get; }

    public int BIP { get; }

    public bool Is32Bit => this.BIP is 32;

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

    public byte[] GetPixelData() => this.m_pixelData;

}
