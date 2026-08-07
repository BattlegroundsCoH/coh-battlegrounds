namespace Battlegrounds.Test.Models.Replays;

/// <summary>
/// Provides static members for accessing sample replay file locations used in testing scenarios.
/// </summary>
/// <remarks>Each member represents the path to a specific replay file located in the test data directory. The
/// class ensures that the referenced files exist at the expected locations, throwing an assertion error if a file is
/// missing. Intended for use in automated tests that require replay data.</remarks>
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

    /// <summary>
    /// Gets the location of a sample replay file for testing.
    /// </summary>
    /// <remarks>
    /// Validates that the file exists at the specified path and throws an assertion error if it does not.
    /// </remarks>
    public static readonly string TEMP_19_06_2025__18_31_FILE = GetReplayLocation("temp_19-06-2025__18_31.rec");

    /// <summary>
    /// Gets the location of a sample replay file for testing.
    /// </summary>
    /// <remarks>
    /// Validates that the file exists at the specified path and throws an assertion error if it does not.
    /// </remarks>
    public static readonly string TEMP_26_02_2026__17_40_FILE = GetReplayLocation("temp_26-02-2026__17_40.rec");

    /// <summary>
    /// Gets the location of a sample replay file for testing.
    /// </summary>
    /// <remarks>
    /// Validates that the file exists at the specified path and throws an assertion error if it does not.
    /// </remarks>
    public static readonly string TEMP_21_04_2026__17_14_FILE = GetReplayLocation("temp_21-04-2026__17_14.rec");

    /// <summary>
    /// Gets the location of a sample replay file for testing.
    /// </summary>
    /// <remarks>
    /// Validates that the file exists at the specified path and throws an assertion error if it does not.
    /// </remarks>
    public static readonly string TEMP_23_04_2026__17_29_FILE = GetReplayLocation("temp_23-04-2026__17_29.rec");

    /// <summary>
    /// Gets the location of a sample replay file for testing.
    /// </summary>
    /// <remarks>
    /// Validates that the file exists at the specified path and throws an assertion error if it does not.<br/>
    /// <br/>Contains a replay file where the player team positions are not as set up in the lobby, but otherwise valid.
    /// </remarks>
    public static readonly string TEMP_24_04_2026__19_25_FILE = GetReplayLocation("temp_24-04-2026__19_25.rec");

    /// <summary>
    /// Gets the location of a sample replay file for testing.
    /// </summary>
    /// <remarks>
    /// Contains no bg_events, but is a valid replay file otherwise. Used to test compatibility with Company of Heroes 3 version 2.5.0.
    /// </remarks>
    public static readonly string BG_COMPATIBILITY_TEST_2_5_0_FILE = GetReplayLocation("bg-compatibility-test-2_5_0.rec");

    private static string GetReplayLocation(string replayName) {
        var path = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestData", "Playbacks", replayName);
        Assert.That(File.Exists(path), Is.True, $"Replay file not found: {path}");
        return path;
    }

}
