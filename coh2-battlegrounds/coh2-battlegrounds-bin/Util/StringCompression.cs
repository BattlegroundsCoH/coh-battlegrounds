﻿using System;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace Battlegrounds.Util;

/// <summary>
/// Static utility class for compressing and decompressing strings using GZip and Base64
/// </summary>
public static class StringCompression {

    /// <summary>
    /// Compress the string into base 64.
    /// </summary>
    /// <param name="text">The text to compress.</param>
    /// <returns>The compressed string.</returns>
    public static string CompressString(this string text) {
        byte[] buffer = Encoding.UTF8.GetBytes(text);
        var memoryStream = new MemoryStream();
        using (var gZipStream = new GZipStream(memoryStream, CompressionMode.Compress, true)) {
            gZipStream.Write(buffer, 0, buffer.Length);
        }

        memoryStream.Position = 0;

        var compressedData = new byte[memoryStream.Length];
        memoryStream.Read(compressedData, 0, compressedData.Length);

        var gZipBuffer = new byte[compressedData.Length + 4];
        Buffer.BlockCopy(compressedData, 0, gZipBuffer, 4, compressedData.Length);
        Buffer.BlockCopy(BitConverter.GetBytes(buffer.Length), 0, gZipBuffer, 0, 4);
        return Convert.ToBase64String(gZipBuffer);
    }

    /// <summary>
    /// Decompress a string from base 64.
    /// </summary>
    /// <param name="compressedText">The text to decompress</param>
    /// <returns>The original string</returns>
    public static string DecompressString(this string compressedText) {
        byte[] gZipBuffer = Convert.FromBase64String(compressedText);
        using (var memoryStream = new MemoryStream()) {
            int dataLength = BitConverter.ToInt32(gZipBuffer, 0);
            memoryStream.Write(gZipBuffer, 4, gZipBuffer.Length - 4);

            var buffer = new byte[dataLength];

            memoryStream.Position = 0;
            using (var gZipStream = new GZipStream(memoryStream, CompressionMode.Decompress)) {
                gZipStream.Read(buffer, 0, buffer.Length);
            }

            return Encoding.UTF8.GetString(buffer);
        }
    }

}
