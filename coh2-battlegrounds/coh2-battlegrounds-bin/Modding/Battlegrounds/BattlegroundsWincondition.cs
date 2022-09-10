namespace Battlegrounds.Modding.Battlegrounds;

/// <summary>
/// Sealed class representing a wincondition specifically tied to the Battlegrounds mod.
/// </summary>
public sealed class BattlegroundsWincondition : IWinconditionMod {

    public ModGuid Guid { get; }

    public string Name { get; }

    public IGamemode[] Gamemodes { get; }

    public ModPackage Package { get; }

    public ModType GameModeType => ModType.Gamemode;

    public BattlegroundsWincondition(ModPackage package, IGamemode[] gamemodes) {

        // Set basic properties
        this.Guid = package.GamemodeGUID;
        this.Name = package.PackageName;

        // Set package
        this.Package = package;

        // Loop over gamemodes
        this.Gamemodes = gamemodes;

    }

}
