using System.Linq;
using System.Text;

namespace Battlegrounds.Util;

/// <summary>
/// Static utilty class providing helper functions for byte data.
/// </summary>
public static class ByteUtil {

    /// <summary>
    /// Check if a byte array matches the contents of the string.
    /// </summary>
    /// <param name="barray">Byte array to check for matching content</param>
    /// <param name="content">The string content to compare against</param>
    /// <returns>True if the contents of the byte array matches the content of the string.</returns>
    public static bool Match(byte[] barray, string content)
        => Match(barray, 0, content);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    public static bool Match(byte[] a, byte[] b) {
        if (a.Length == b.Length) {
            return a.Zip(b).All((z) => z.First == z.Second);
        } else {
            return false;
        }
    }

    /// <summary>
    /// Check if a byte array matches the contents of the string.
    /// </summary>
    /// <param name="src">Byte array to check for matching content</param>
    /// <param name="offset">The offset in the source array to compare</param>
    /// <param name="content">The string content to compare against</param>
    /// <returns>True if the contents of the byte array matches the content of the string.</returns>
    public static bool Match(byte[] src, int offset, byte[] content) {
        if (content.Length <= src.Length) {
            for (int i = 0; i < content.Length && i + offset < src.Length; i++) {
                if (src[i + offset] != content[i]) {
                    return false;
                }
            }
            return offset + content.Length <= src.Length;
        } else {
            return false;
        }
    }

    /// <summary>
    /// Check if a byte array matches the contents of the string.
    /// </summary>
    /// <param name="src">Byte array to check for matching content</param>
    /// <param name="offset">The offset in the source array to compare</param>
    /// <param name="content">The string content to compare against</param>
    /// <returns>True if the contents of the byte array matches the content of the string.</returns>
    public static bool Match(byte[] src, int offset, string content)
        => Match(src, offset, Encoding.ASCII.GetBytes(content));

}
