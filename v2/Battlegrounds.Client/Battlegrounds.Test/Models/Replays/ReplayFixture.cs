namespace Battlegrounds.Test.Models.Replays;

public static class ReplayFixture {

    /// <summary>
    /// Gets the location of a sample replay file for testing.
    /// </summary>
    /// <remarks>
    /// Validates that the file exists at the specified path and throws an assertion error if it does not.
    /// </remarks>
    public static readonly string TEMP_23_05_2025__23_51_FILE = GetReplayLocation("temp_23-05-2025__23_51.rec");

    /// <summary>
    /// Gets the location of a sample replay file for testing.
    /// </summary>
    /// <remarks>
    /// Validates that the file exists at the specified path and throws an assertion error if it does not.<br/>
    /// <br/>Bad replay file since it lacks actual bg_events, but it does have broadcast messages (that are ignored)
    /// </remarks>
    public static readonly string TEMP_10_05_2025__21_16_FILE = GetReplayLocation("temp_10-05-2025__21_16.rec");

    /// <summary>
    /// Gets the location of a sample replay file for testing.
    /// </summary>
    /// <remarks>
    /// Validates that the file exists at the specified path and throws an assertion error if it does not.
    /// </remarks>
    public static readonly string TEMP_10_05_2025__20_55_FILE = GetReplayLocation("temp_10-05-2025__20_55.rec");

    private static string GetReplayLocation(string replayName) {
        var path = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestData", "Playbacks", replayName);
        Assert.That(File.Exists(path), Is.True, $"Replay file not found: {path}");
        return path;
    }

}
