namespace Battlegrounds.Testing;

internal static class Github {
    public static bool IsGithub() => Environment.GetEnvironmentVariable("TEST_LOCATION") is "github";
    public static void BailIfGithub(string reason) {
        if (IsGithub()) {
            Assert.Inconclusive(reason);
        }
    }
}
