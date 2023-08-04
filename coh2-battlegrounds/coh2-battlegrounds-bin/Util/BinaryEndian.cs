using System;

namespace Battlegrounds.Util;

/// <summary>
/// Provides extension methods for converting between little-endian and big-endian byte order.
/// </summary>
public static class BinaryEndian {

    /// <summary>
    /// Converts a byte array to big-endian byte order and creates an object of type T.
    /// </summary>
    /// <typeparam name="T">The type of object to create from the byte array.</typeparam>
    /// <param name="raw">The byte array to convert.</param>
    /// <param name="func">A delegate that creates an object of type T from a byte array and an offset.</param>
    /// <returns>An object of type T in big-endian byte order.</returns>
    public static T ConvertBigEndian<T>(this byte[] raw, Func<byte[], int, T> func) {
        if (BitConverter.IsLittleEndian) {
            Array.Reverse(raw);
        }
        return func(raw, 0);
    }

    /// <summary>
    /// Converts a byte array to big-endian byte order.
    /// </summary>
    /// <param name="raw">The byte array to convert.</param>
    /// <returns>A byte array in big-endian byte order.</returns>
    public static byte[] ConvertBigEndian(this byte[] raw) {
        if (BitConverter.IsLittleEndian) {
            Array.Reverse(raw);
        }
        return raw;
    }

}
