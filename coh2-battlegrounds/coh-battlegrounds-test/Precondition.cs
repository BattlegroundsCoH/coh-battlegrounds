namespace Battlegrounds.Testing;

internal static class Precondition {
    
    public static bool IsGithub() => Environment.GetEnvironmentVariable("TEST_LOCATION") is "github";
    
    public static void RequiresNotGithubActions(string reason) {
        if (IsGithub()) {
            Assert.Inconclusive(reason);
        }
    }

    private static readonly string[] coh3 = {
        "C:\\Program Files (x86)\\Steam\\steamapps\\common\\Company of Heroes 3"
    };

    public static string RequiresEssenceEditor() {
        for (int i = 0; i < coh3.Length; i++) {
            string ee = Path.Combine(coh3[i], "EssenceEditor.exe");
            if (File.Exists(ee)) {
                return ee;
            }
        }
        Assert.Inconclusive("No Essence Editor found; Try adding your local CoH3 path in the Preconditions file");
        return string.Empty;
    }

}
