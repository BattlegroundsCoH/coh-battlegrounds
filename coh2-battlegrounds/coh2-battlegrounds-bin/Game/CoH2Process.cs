namespace Battlegrounds.Game;

/// <summary>
/// Class for launching Company of Heroes 2 with the proper set of arguments.
/// </summary>
public sealed class CoH2Process : GameProcess {

    /// <summary>
    /// The steam app ID for Company of Heroes 2.
    /// </summary>
    public override string AppId => "231430";

    /// <summary>
    /// Get the name of the executable.
    /// </summary>
    public override string ApplicationExecutable => "RelicCoH2";

}
