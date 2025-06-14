namespace Battlegrounds.Test;

public abstract class LocalTestOnly {

    [SetUp]
    public void GithubPreflightTest() {
        Github.SkipIfGitubActions();
    }

}
