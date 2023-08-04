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
/// 
/// </summary>
/// <param name="DataType"></param>
/// <param name="IDLength"></param>
/// <param name="ColourMapType"></param>
/// <param name="ColourMapD"></param>
/// <param name="BitsPerPixel"></param>
/// <param name="Flags"></param>
/// <param name="ColourMapO"></param>
/// <param name="ColourMapL"></param>
/// <param name="XO"></param>
/// <param name="YO"></param>
/// <param name="Width"></param>
/// <param name="Height"></param>
public record TgaImageHeader(TgaImageDatatype DataType,
byte IDLength, byte ColourMapType, byte ColourMapD, byte BitsPerPixel, byte Flags,
short ColourMapO, short ColourMapL, short XO, short YO, short Width, short Height);
