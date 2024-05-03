namespace Battlegrounds.Core;

public record struct Version(int Major, int Minor, int Build) {

    public static readonly Version CoreVersion = new Version(2, 0, 0); // Remember to bump when releasing a new version!

    public override readonly string ToString() => $"v{Major}.{Minor}.{Build}";

    public readonly bool IsGraterThan(Version other) {
        if (this.Major > other.Major)
            return true;
        if (this.Minor > other.Minor)
            return true;
        if (this.Build > other.Build)
            return true;
        return false;
    }

    public static bool operator <(Version lhs, Version rhs) => rhs.IsGraterThan(lhs);

    public static bool operator >(Version lhs, Version rhs) => lhs.IsGraterThan(rhs);

}
