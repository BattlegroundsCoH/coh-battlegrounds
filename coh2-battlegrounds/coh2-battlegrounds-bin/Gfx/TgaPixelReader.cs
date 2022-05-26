using System;
using System.IO;
using System.Runtime.CompilerServices;

namespace Battlegrounds.Gfx;

public static class TgaPixelReader {

    public static TgaImage ReadTarga(string targaSourcePath) {

        // Make sure the file exists
        if (!File.Exists(targaSourcePath)) {
            throw new FileNotFoundException("Failed to find targa file from argument.", targaSourcePath);
        }

        // Open the binary reader
        using BinaryReader binaryReader = new BinaryReader(File.OpenRead(targaSourcePath));

        // Define local variables
        TgaImageDatatype DataType;
        byte IDLength, ColourMapType, ColourMapD, BitsPerPixel, Flags;
        short ColourMapO, ColourMapL, XO, YO, Width, Height;

        // Try and read header
        try {

            IDLength = binaryReader.ReadByte();
            ColourMapType = binaryReader.ReadByte();
            DataType = (TgaImageDatatype)binaryReader.ReadByte();

            ColourMapO = binaryReader.ReadInt16();
            ColourMapL = binaryReader.ReadInt16();
            ColourMapD = binaryReader.ReadByte();

            XO = binaryReader.ReadInt16();
            YO = binaryReader.ReadInt16();

            Width = binaryReader.ReadInt16();
            Height = binaryReader.ReadInt16();

            BitsPerPixel = binaryReader.ReadByte();
            Flags = binaryReader.ReadByte();

        } catch (Exception e) {

            throw new BadImageFormatException("Failed to read tga header.", e);

        }

        // Make sure it's a valid datatype
        bool IsValidDataType = DataType is TgaImageDatatype.UNCOMPRESSED_RGB or TgaImageDatatype.RUN_LENGTH_ENCODED_RGB;

        // Make sure it's a valid BPP
        bool IsValidBPP = BitsPerPixel is 24 or 32;

        // Make sure the flag is valid
        bool IsValidFlag = (Flags & 0xC0) == 0;

        // Verify header details
        if (!IsValidDataType) {
            throw new BadImageFormatException($"Unexpected tga file format '{DataType}'. Please save the .tga file in a different format!");
        }

        // Verify header details
        if (!IsValidBPP) {
            throw new BadImageFormatException($"Unexpected tga file format '{BitsPerPixel}' is not a valid BPP. Please save the .tga file with either 24 or 32 bits per pixel!");
        }

        // Verify header details
        if (!IsValidFlag) {
            throw new BadImageFormatException($"Unexpected tga file flag '{Flags}'. Please save the .tga file in a different format!");
        }

        // 32 bits per pixel?
        bool is32bit = BitsPerPixel is 32;

        // Declare image buffer
        byte[] imageBuffer = new byte[(BitsPerPixel >> 3) * Width * Height];

        // Goto colour map and skip it
        binaryReader.BaseStream.Seek(IDLength, SeekOrigin.Current);
        if (ColourMapType == 1) {
            binaryReader.BaseStream.Seek(ColourMapL * (ColourMapD >> 3), SeekOrigin.Current);
        }

        // If uncrompressed, it's a simple copy/paste
        if (DataType is TgaImageDatatype.UNCOMPRESSED_RGB) {

            // Read raw rgb(a) data
            binaryReader.BaseStream.Read(imageBuffer, 0, (Width * Height) * (BitsPerPixel / 8));

        } else if (DataType is TgaImageDatatype.RUN_LENGTH_ENCODED_RGB) {

            // Pixel Index
            int n = 0;
            int bpp = BitsPerPixel / 8;
            int pixels = Width * Height;

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

        return new TgaImage(Width, Height, 96, 96, imageBuffer, DataType, BitsPerPixel);

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
