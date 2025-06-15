namespace Battlegrounds.Modding.Battlegrounds;

/// <summary>
/// Sealed class representing a wincondition specifically tied to the Battlegrounds mod.
/// </summary>
public sealed class BattlegroundsWincondition : IWinconditionMod {

    /// <inheritdoc/>
    public ModGuid Guid { get; }

    /// <inheritdoc/>
    public string Name { get; }

    /// <inheritdoc/>
    public IGamemode[] Gamemodes { get; }

    /// <inheritdoc/>
    public IModPackage Package { get; }

    /// <inheritdoc/>
    public ModType GameModeType => ModType.Gamemode;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="package"></param>
    /// <param name="gamemodes"></param>
    public BattlegroundsWincondition(IModPackage package, IGamemode[] gamemodes) {

        // Set basic properties
        this.Guid = package.GamemodeGUID;
        this.Name = package.PackageName;

        // Set package
        this.Package = package;

        // Loop over gamemodes
        this.Gamemodes = gamemodes;

    }

}
