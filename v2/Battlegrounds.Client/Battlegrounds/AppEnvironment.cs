namespace Battlegrounds;

/// <summary>
/// Build-time facts about the running app.
/// </summary>
/// <remarks>
/// A composition concern rather than a domain model, so it sits beside
/// <see cref="BattlegroundsApp"/> rather than in <c>Models/</c>.
/// </remarks>
public static class AppEnvironment {

    /// <summary>
    /// Whether this is a developer build.
    /// </summary>
    /// <remarks>
    /// Compiled out rather than configured, so no <c>config.json</c> edit can turn these on in
    /// a shipped build.
    /// </remarks>
    public static bool IsDeveloperMode =>
#if DEBUG
        true;
#else
        false;
#endif

}
