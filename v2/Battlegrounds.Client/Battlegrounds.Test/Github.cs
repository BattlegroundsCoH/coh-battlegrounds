namespace Battlegrounds.Test;

public static class Github {

    private static bool _isGithubActions = false;

    public static bool IsGithubActions {
        get {
            if (_isGithubActions) 
                return true;
            _isGithubActions = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("TEST_LOCATION"));
            TestContext.Out.WriteLine($"IsGithubActions: {_isGithubActions}");
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
