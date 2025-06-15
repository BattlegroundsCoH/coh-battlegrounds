namespace Battlegrounds.Game;

/// <summary>
/// Class for launching Company of Heroes 3 with the proper set of arguments.
/// </summary>
public sealed class CoH3Process : GameProcess {

    /// <summary>
    /// The steam app ID for Company of Heroes 3.
    /// </summary>
    public override string AppId => "1677280";

    /// <summary>
    /// Get the name of the executable.
    /// </summary>
    public override string ApplicationExecutable => "Anvil";

}
