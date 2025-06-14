using NUnit.Framework.Interfaces;

namespace Battlegrounds.Test;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class NoGithubActionsAttribute(string reason) : Attribute, IApplyToTest {
    public string Reason { get; } = reason;

    public void ApplyToTest(NUnit.Framework.Internal.Test test) {
        if (Github.IsGithubActions) {
            test.RunState = RunState.NotRunnable;
            test.Properties.Set("Reason", $"Test skipped in GitHub Actions: {Reason}");
        }
    }
}
