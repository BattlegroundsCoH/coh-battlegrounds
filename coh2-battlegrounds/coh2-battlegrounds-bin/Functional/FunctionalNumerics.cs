namespace Battlegrounds.Functional;

public static class FunctionalNumerics {

    public static bool InRange(this int n, int min, int max) => min <= n && n <= max;

    public static bool InRange(this uint n, uint min, uint max) => min <= n && n <= max;

}
