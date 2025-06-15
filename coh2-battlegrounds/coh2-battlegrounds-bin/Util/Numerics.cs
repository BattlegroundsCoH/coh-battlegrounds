using System;

namespace Battlegrounds.Util;

/// <summary>
/// Static utility class for working with numbers.
/// </summary>
public static class Numerics {

    /// <summary>
    /// Constant radian to degree convertor. (1 Rad = 57.29578°)
    /// </summary>
    public const double RAD2DEG = 57.29578;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="x"></param>
    /// <returns></returns>
    public static double Square(this double x) => x * x;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="x"></param>
    /// <returns></returns>
    public static float Square(this float x) => x * x;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="x"></param>
    /// <returns></returns>
    public static int Square(this int x) => x * x;

    /// <summary>
    /// Fold <paramref name="b"/> times, mutating the initial <paramref name="s"/>.
    /// </summary>
    /// <typeparam name="T">The type of the state to mutate.</typeparam>
    /// <param name="b">The byte defining how many times to fold.</param>
    /// <param name="s">The inital state.</param>
    /// <param name="fld">The folding function.</param>
    /// <returns>The final state of the folding operation.</returns>
    public static T Fold<T>(this byte b, T s, Func<byte, T, T> fld) {
        T v = s;
        for (byte i = 0; i < b; i++)
            v = fld(i, v);
        return v;
    }

}
