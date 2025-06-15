using System;
using System.IO;
using System.Runtime.CompilerServices;

using Battlegrounds.Errors.IO;
using Battlegrounds.Functional;

namespace Battlegrounds.Gfx;

/// <summary>
/// Static utility class for reading the pixel data for <see cref="TgaImage"/> files.
/// </summary>
public static class TgaPixelReader {

    /// <summary>
    /// Reads a Targa image header from a binary reader.
    /// </summary>
    /// <param name="reader">The binary reader to read from.</param>
    /// <returns>An <see cref="Either{T1, T2}"/> containing the <see cref="TgaImageHeader"/> if the header was successfully read, or an <see cref="Exception"/> if an error occurred.</returns>
    public static Either<TgaImageHeader, IOException> ReadTargaImageHeader(BinaryReader reader) {

        // Define local variables
        TgaImageDatatype DataType;
        byte IDLength, ColourMapType, ColourMapD, BitsPerPixel, Flags;
        short ColourMapO, ColourMapL, XO, YO, Width, Height;

        // Try and read header
        try {

            IDLength = reader.ReadByte();
            ColourMapType = reader.ReadByte();
            DataType = (TgaImageDatatype)reader.ReadByte();

            ColourMapO = reader.ReadInt16();
            ColourMapL = reader.ReadInt16();
            ColourMapD = reader.ReadByte();

            XO = reader.ReadInt16();
            YO = reader.ReadInt16();

            Width = reader.ReadInt16();
            Height = reader.ReadInt16();

            BitsPerPixel = reader.ReadByte();
            Flags = reader.ReadByte();

        } catch (IOException e) {
            return e;
        }

        // Return header
        return new TgaImageHeader(DataType, IDLength, ColourMapType, ColourMapD, BitsPerPixel, Flags, ColourMapO, ColourMapL, XO, YO, Width, Height);

    }

    /// <summary>
    /// Reads a <see cref="TgaImage"/> file from the specified <paramref name="targaSourcePath"/>.
    /// </summary>
    /// <param name="targaSourcePath">The relative path of the targa file to read.</param>
    /// <returns>A <see cref="TgaImage"/> instance.</returns>
    /// <exception cref="FileNotFoundException"></exception>
    /// <exception cref="InvalidFileContentsException"></exception>
    public static TgaImage ReadTarga(string targaSourcePath) {


        // Make sure the file exists
        if (!File.Exists(targaSourcePath)) {
            throw new FileNotFoundException("Failed to find targa file from argument.", targaSourcePath);
        }

        // Open the binary reader
        using BinaryReader binaryReader = new BinaryReader(File.OpenRead(targaSourcePath));

        // Return
        return ReadTarga(binaryReader);

    }

    /// <summary>
    /// Reads a <see cref="TgaImage"/> file from the specified <paramref name="binaryReader"/>.
    /// </summary>
    /// <param name="binaryReader">The already open <see cref="BinaryReader"/> for the targa file to read.</param>
    /// <returns>A <see cref="TgaImage"/> instance.</returns>
    /// <exception cref="FileNotFoundException"></exception>
    /// <exception cref="InvalidFileContentsException"></exception>
    public static TgaImage ReadTarga(BinaryReader binaryReader) {

        // Read header
        var headerOrExc = ReadTargaImageHeader(binaryReader);
        if (headerOrExc.IfSecondOption(out IOException? ioex)) {
            throw new InvalidFileContentsException("Failed to read tga header", ioex);
        }

        // Get header
        var header = headerOrExc.First;

        // Make sure it's a valid datatype
        bool IsValidDataType = header.DataType is TgaImageDatatype.UNCOMPRESSED_RGB or TgaImageDatatype.RUN_LENGTH_ENCODED_RGB;

        // Make sure it's a valid BPP
        bool IsValidBPP = header.BitsPerPixel is 24 or 32;

        // Make sure the flag is valid
        bool IsValidFlag = (header.Flags & 0xC0) == 0;

        // Get if v/h flipped (5th bit == 0 then flipped verticallly according to: https://gamedev.net/forums/topic/63834-why-does-my-tga-appear-upside-down/ )
        int origoFlag = (header.Flags & 0x30) >> 0x04;
        bool IsHorizontallyFlipped = origoFlag is 0x01 or 0x03;
        bool IsVerticallyFlipped = origoFlag is 0x00 or 0x01;

        // Verify header details
        if (!IsValidDataType) {
            throw new InvalidFileContentsException($"Unexpected tga file format '{header.DataType}'. Please save the .tga file in a different format!");
        }

        // Verify header details
        if (!IsValidBPP) {
            throw new InvalidFileContentsException($"Unexpected tga file format '{header.BitsPerPixel}' is not a valid BPP. Please save the .tga file with either 24 or 32 bits per pixel!");
        }

        // Verify header details
        if (!IsValidFlag) {
            throw new InvalidFileContentsException($"Unexpected tga file flag '{header.Flags}'. Please save the .tga file in a different format!");
        }

        // 32 bits per pixel?
        bool is32bit = header.BitsPerPixel is 32;

        // Declare image buffer
        byte[] imageBuffer = new byte[(header.BitsPerPixel >> 3) * header.Width * header.Height];

        // Goto colour map and skip it
        binaryReader.BaseStream.Seek(header.IDLength, SeekOrigin.Current);
        if (header.ColourMapType == 1) {
            binaryReader.BaseStream.Seek(header.ColourMapL * (header.ColourMapD >> 3), SeekOrigin.Current);
        }

        // If uncrompressed, it's a simple copy/paste
        if (header.DataType is TgaImageDatatype.UNCOMPRESSED_RGB) {

            // Read raw rgb(a) data
            binaryReader.BaseStream.Read(imageBuffer, 0, (header.Width * header.Height) * (header.BitsPerPixel / 8));

        } else if (header.DataType is TgaImageDatatype.RUN_LENGTH_ENCODED_RGB) {

            // Pixel Index
            int n = 0;
            int bpp = header.BitsPerPixel / 8;
            int pixels = header.Width * header.Height;

            unsafe {
                fixed (byte* pBuffer = imageBuffer) {

                    // While packets to read
                    while (n < pixels) {

                        // Read packet header
                        byte packetHeader = binaryReader.ReadByte();
                        int packetPixelCount = packetHeader & 0x7f;

                        // Read initial pixel
                        ReadPixel(pBuffer + (n * bpp), binaryReader, bpp);
                        n++;

                        if ((packetHeader & 0x80) != 0) { // Run-Length chunk (So copy the same colour by packetPixelCount amount)
                            for (int i = 0; i < packetPixelCount; i++) {
                                Unsafe.CopyBlock(pBuffer + (n * bpp), pBuffer, (uint)bpp);
                                n++;
                            }
                        } else { // Raw chunk, each packetPixelCount is a unique colour)
                            for (int i = 0; i < packetPixelCount; i++) {
                                ReadPixel(pBuffer + (n * bpp), binaryReader, bpp);
                                n++;
                            }
                        }

                    }

                }
            }

        }

        // If flipped horizontally, undo
        if (IsHorizontallyFlipped) {
            /*byte[] tmp = new byte[imageBuffer.Length];
            unsafe {
                fixed (byte* hflip = tmp) {
                    fixed (byte* pBuffer = imageBuffer) {
                        for (int i = 0; i < Width; i++) {

                        }
                    }
                }
            }*/
        }

        // If flipped vertically, undo
        if (IsVerticallyFlipped) {
            byte[] tmp = new byte[imageBuffer.Length];
            uint stride = (uint)((header.Width * header.BitsPerPixel) / 8);
            unsafe {
                fixed (byte* hflip = tmp) {
                    fixed (byte* pBuffer = imageBuffer) {
                        for (int y = 0; y < header.Height; y++) {
                            Unsafe.CopyBlock(hflip + (y * stride), pBuffer + ((header.Height - 1 - y) * stride), stride);
                        }
                    }
                }
            }
            imageBuffer = tmp;
        }

        // Return image
        return new TgaImage(header, imageBuffer);

    }

    private static unsafe void ReadPixel(byte* buffer, BinaryReader binaryReader, int size) {
        byte[] colour = binaryReader.ReadBytes(size);
        if (size is 3) {
            buffer[0] = colour[2];
            buffer[1] = colour[1];
            buffer[2] = colour[0];
        } else {
            buffer[0] = colour[2];
            buffer[1] = colour[1];
            buffer[2] = colour[0];
            buffer[3] = colour[3];
        }
    }

}
