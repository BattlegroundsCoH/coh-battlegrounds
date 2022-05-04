using System;

namespace Battlegrounds.Util;

public static class BinaryEndian {

    public static T ConvertBigEndian<T>(this byte[] raw, Func<byte[], int, T> func) {
        if (BitConverter.IsLittleEndian) {
            Array.Reverse(raw);
        }
        return func(raw, 0);
    }

    public static byte[] ConvertBigEndian(this byte[] raw) {
        if (BitConverter.IsLittleEndian) {
            Array.Reverse(raw);
        }
        return raw;
    }

}
