namespace Battlegrounds.Core;

public record struct Version(int Major, int Minor, int Patch) {

    public static readonly Version CoreVersion = new Version(2, 0, 0); // Remember to bump when releasing a new version!

    public override readonly string ToString() => $"v{Major}.{Minor}.{Patch}";

    public readonly bool IsGreaterThan(Version other) {
        if (this.Major > other.Major)
            return true;
        if (this.Minor > other.Minor)
            return true;
        if (this.Patch > other.Patch)
            return true;
        return false;
    }

    public static bool operator <(Version lhs, Version rhs) => rhs.IsGreaterThan(lhs);

    public static bool operator >(Version lhs, Version rhs) => lhs.IsGreaterThan(rhs);

}
