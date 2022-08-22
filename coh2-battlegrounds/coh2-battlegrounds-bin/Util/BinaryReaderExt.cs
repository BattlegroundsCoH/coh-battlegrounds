using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Battlegrounds.Util;

/// <summary>
/// Extension class for extending the utilities of <see cref="BinaryReader"/>
/// </summary>
public static class BinaryReaderExt {

    /// <summary>
    /// Read an ASCII string from a length that's given before the string
    /// </summary>
    /// <param name="reader">The stream to read string from</param>
    /// <returns>The read string</returns>
    public static string ReadASCIIString(this BinaryReader reader)
        => reader.ReadASCIIString((int)reader.ReadUInt32());

    /// <summary>
    /// Read an ASCII string from a known length
    /// </summary>
    /// <param name="reader">The stream to read string from</param>
    /// <param name="length"></param>
    /// <returns>The read string</returns>
    public static string ReadASCIIString(this BinaryReader reader, int length)
        => Encoding.ASCII.GetString(reader.ReadBytes(length));

    /// <summary>
    /// Read a Unicode string from a length that's given before the string
    /// </summary>
    /// <param name="reader">The stream to read string from</param>
    /// <returns>The read string</returns>
    public static string ReadUnicodeString(this BinaryReader reader)
        => reader.ReadUnicodeString((int)reader.ReadUInt32());

    /// <summary>
    /// Read a Unicode string from a known length
    /// </summary>
    /// <param name="reader">The stream to read string from</param>
    /// <param name="length"></param>
    /// <returns>The read string</returns>
    public static string ReadUnicodeString(this BinaryReader reader, int length)
        => Encoding.Unicode.GetString(reader.ReadBytes(length));

    /// <summary>
    /// Read a UTF-8 string of unknown length
    /// </summary>
    /// <param name="reader">The stream to read string from</param>
    /// <returns></returns>
    public static string ReadUTF8String(this BinaryReader reader) {
        StringBuilder strBuilder = new StringBuilder();
        while (!reader.HasReachedEOS()) {
            ushort u = BitConverter.ToUInt16(reader.ReadBytes(2));
            if (u == 0) {
                break;
            } else {
                strBuilder.Append((char)u);
            }
        }
        return strBuilder.ToString();
    }

    /// <summary>
    /// Read a UTF-8 of known character count
    /// </summary>
    /// <param name="reader">The stream to read string from</param>
    /// <param name="characterCount">The amount of UTF-8 characters to read</param>
    /// <returns></returns>
    public static string ReadUTF8String(this BinaryReader reader, uint characterCount) {
        byte[] content = reader.ReadBytes((int)characterCount * 2);
        StringBuilder strBuilder = new StringBuilder();
        for (int i = 0; i < content.Length; i += 2) {
            ushort u = BitConverter.ToUInt16(content, i);
            strBuilder.Append((char)u);
        }
        return strBuilder.ToString();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="reader"></param>
    /// <param name="encoding"></param>
    /// <param name="characterCount"></param>
    /// <returns></returns>
    public static string ReadEncodedString(this BinaryReader reader, Encoding encoding, int characterCount = -1) {
        if (encoding == Encoding.ASCII) {
            return characterCount == -1 ? reader.ReadASCIIString() : reader.ReadASCIIString(characterCount);
        } else if (encoding == Encoding.UTF8) {
            return characterCount == -1 ? reader.ReadUTF8String() : reader.ReadUTF8String((uint)characterCount);
        } else if (encoding == Encoding.Unicode) {
            return characterCount == -1 ? reader.ReadUnicodeString() : reader.ReadUnicodeString(characterCount);
        } else {
            throw new NotSupportedException();
        }
    }

    /// <summary>
    /// Extension method: Skips the specified amount of bytes
    /// </summary>
    /// <param name="reader">The stream to skip bytes in</param>
    /// <param name="count">The amount of bytes to skip</param>
    public static void Skip(this BinaryReader reader, long count)
        => reader.BaseStream.Seek(count, SeekOrigin.Current);

    /// <summary>
    /// Has the stream reached the end of stream (EOS)
    /// </summary>
    /// <param name="reader">The stream to check</param>
    /// <returns>True if the stream position is equal or greater than the stream length</returns>
    public static bool HasReachedEOS(this BinaryReader reader)
        => reader.BaseStream.Position >= reader.BaseStream.Length;

    /// <summary>
    /// Skip all bytes until a specific byte array match is found
    /// </summary>
    /// <param name="reader">The stream to advance memory pointer in</param>
    /// <param name="barray">The byte array to find and read</param>
    public static void SkipUntil(this BinaryReader reader, byte[] barray) {

        while (!reader.HasReachedEOS()) {

            byte[] arr = reader.ReadBytes(barray.Length);

            if (ByteUtil.Match(arr, barray)) {
                break;
            } else {
                reader.BaseStream.Position -= (barray.Length - 1);
            }

        }

    }

    /// <summary>
    /// Read all bytes remaining in the <see cref="BinaryReader"/> object
    /// </summary>
    /// <param name="reader">The stream to read to end</param>
    /// <returns>Byte array containing all the bytes remaining in the stream</returns>
    public static byte[] ReadToEnd(this BinaryReader reader) {
        List<byte> content = new List<byte>();
        while (!reader.HasReachedEOS()) {
            if (reader.BaseStream.Position + 256 <= reader.BaseStream.Length) {
                content.AddRange(reader.ReadBytes(256));
            } else {
                content.AddRange(reader.ReadBytes((int)(reader.BaseStream.Length - reader.BaseStream.Position)));
            }
        }
        return content.ToArray();
    }

    /// <summary>
    /// Read <paramref name="amount"/> bytes into a <see cref="MemoryStream"/>.
    /// </summary>
    /// <param name="reader">The <see cref="BinaryReader"/> to use when filling stream with data.</param>
    /// <param name="amount">The amount of bytes to read from <paramref name="reader"/>.</param>
    /// <param name="memory">The final <see cref="MemoryStream"/> populated with read byte data.</param>
    public static void FillStream(this BinaryReader reader, int amount, out MemoryStream memory) {
        memory = new MemoryStream(reader.ReadBytes(amount)) {
            Position = 0 // Just to be safe...
        };
    }

    /// <summary>
    /// Read <paramref name="amount"/> bytes into a <see cref="MemoryStream"/>.
    /// </summary>
    /// <param name="reader">The <see cref="BinaryReader"/> to use when filling stream with data.</param>
    /// <param name="amount">The amount of bytes to read from <paramref name="reader"/>.</param>
    /// <returns>The populated <see cref="MemoryStream"/> instance filled with read byte data.</returns>
    public static MemoryStream FillStream(this BinaryReader reader, int amount) {
        FillStream(reader, amount, out MemoryStream mem);
        return mem;
    }

}

