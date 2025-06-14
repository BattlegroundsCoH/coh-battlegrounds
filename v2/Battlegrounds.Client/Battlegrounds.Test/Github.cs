namespace Battlegrounds.Test;

public static class Github {

    private static bool _isGithubActions = false;

    public static bool IsGithubActions {
        get {
            if (_isGithubActions) 
                return true;
            _isGithubActions = Environment.GetEnvironmentVariable("GITHUB_ACTIONS") == "true";
            return _isGithubActions;
        }
    }

    public static void SkipIfGitubActions() {
        if (!IsGithubActions) {
            return;
        }
        Assert.Inconclusive("This test is skipped in GitHub Actions environment.");
    }

}
