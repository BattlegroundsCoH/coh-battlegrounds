using System.Text;

namespace Battlegrounds.Util;

/// <summary>
/// 
/// </summary>
public static class StringUtil {

    /// <summary>
    /// 
    /// </summary>
    /// <param name="s"></param>
    /// <returns></returns>
    public static byte[] Encode(this string s) => s.Encode(Encoding.ASCII);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="s"></param>
    /// <param name="encoding"></param>
    /// <returns></returns>
    public static byte[] Encode(this string s, Encoding encoding) => encoding.GetBytes(s);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="s"></param>
    /// <param name="patterns"></param>
    /// <returns></returns>
    public static bool EndsWithAny(this string s, params string[] patterns) {
        for (int i = 0; i < patterns.Length; i++) {
            if (s.EndsWith(patterns[i])) {
                return true;
            }
        }
        return false;
    }

}
