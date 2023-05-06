using System;

using Battlegrounds.Modding;

namespace Battlegrounds.Game.Blueprints;

/// <summary>
/// Readonly struct that epresents a unique identifier for a blueprint, by mod.
/// </summary>
public readonly struct BlueprintUID {

    /// <summary>
    /// The <see cref="ModGuid"/> associated with this blueprint UID
    /// </summary>
    public readonly ModGuid Mod;

    /// <summary>
    /// The unique identifier associated with the blueprint.
    /// </summary>
    public readonly ulong UniqueIdentifier;

    /// <summary>
    /// The game associated with this blueprint UID
    /// </summary>
    public readonly GameCase Game;

    /// <summary>
    /// Initialise a new <see cref="BlueprintUID"/> instance with <paramref name="uid"/> for a <see cref="ModGuid.BaseGame"/> blueprint.
    /// </summary>
    /// <param name="uid">The unique ID.</param>
    public BlueprintUID(ulong uid) : this(uid, ModGuid.BaseGame) { }

    /// <summary>
    /// Initialise a new <see cref="BlueprintUID"/> instance with <paramref name="uid"/> for a <paramref name="mod"/> blueprint.
    /// </summary>
    /// <param name="uid">The unique ID.</param>
    /// <param name="mod">The mod to associate <paramref name="uid"/> with.</param>
    public BlueprintUID(ulong uid, ModGuid mod) {
        UniqueIdentifier = uid;
        Mod = mod;
    }

    /// <inheritdoc/>
    public override string ToString() => (Mod == ModGuid.BaseGame || string.IsNullOrEmpty(Mod.GUID)) ? UniqueIdentifier.ToString() : $"{Mod.GUID}:{UniqueIdentifier}";

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is BlueprintUID bip && bip.UniqueIdentifier == UniqueIdentifier && bip.Mod.Equals(Mod);

    /// <inheritdoc/>
    public override int GetHashCode() {
        HashCode code = new();
        code.Add(Mod);
        code.Add(UniqueIdentifier);
        return code.ToHashCode();
    }

    /// <inheritdoc/>
    public static bool operator ==(BlueprintUID left, BlueprintUID right) => left.Equals(right);

    /// <inheritdoc/>
    public static bool operator !=(BlueprintUID left, BlueprintUID right) => !(left == right);

}
