namespace Battlegrounds.Modding;

/// <summary>
/// Enum representing the different types of mods.
/// </summary>
public enum ModType {

    /// <summary>
    /// Wincondition Pack
    /// </summary>
    Gamemode,

    /// <summary>
    /// Propertybaggroup Pack
    /// </summary>
    Tuning,

    /// <summary>
    /// Asset Pack
    /// </summary>
    Asset,

    /// <summary>
    /// Skin Pack
    /// </summary>
    Skin

}

/// <summary>
/// Basic interface for mods used by the game.
/// </summary>
public interface IGameMod {

    /// <summary>
    /// Get the associated mod package.
    /// </summary>
    IModPackage Package { get; }

    /// <summary>
    /// Get the <see cref="ModGuid"/> used to identify the mod.
    /// </summary>
    ModGuid Guid { get; }

    /// <summary>
    /// Get the display name of the mod.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Get the <see cref="ModType"/> of the <see cref="IGameMod"/>.
    /// </summary>
    ModType GameModeType { get; }

}
