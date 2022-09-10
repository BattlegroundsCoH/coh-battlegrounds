using System;
using System.Collections.Generic;
using System.Globalization;

using Battlegrounds.ErrorHandling.CommonExceptions;

namespace Battlegrounds.Game.Gameplay;

/// <summary>
/// Represents a faction in Company of Heroes 2. This class cannot be inherited. This class cannot be instantiated.
/// </summary>
public sealed class Faction {

    /// <summary>
    /// The unique ID assigned to this <see cref="Faction"/>.
    /// </summary>
    public byte UID { get; }

    /// <summary>
    /// The SCAR name for this <see cref="Faction"/>.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Get the racebps path.
    /// </summary>
    public string RbpPath { get; }

    /// <summary>
    /// Is this <see cref="Faction"/> an allied faction.
    /// </summary>
    public bool IsAllied { get; }

    /// <summary>
    /// Is this <see cref="Faction"/> an axis faction.
    /// </summary>
    public bool IsAxis => !this.IsAllied;

    /// <summary>
    /// The required <see cref="DLCPack"/> to be able to play with this <see cref="Faction"/>.
    /// </summary>
    public DLCPack RequiredDLC { get; }

    private Faction(byte id, string name, string rbppath, bool isAllied, DLCPack requiredDLC) {
        // Private constructor, we can't have custom armies, doesn't make sense to allow them then.
        this.UID = id;
        this.Name = name;
        this.RbpPath = rbppath;
        this.IsAllied = isAllied;
        this.RequiredDLC = requiredDLC;
    }

    /// <summary>
    /// Returns a string that represents the current object.
    /// </summary>
    /// <returns>A string that represents the current object.</returns>
    public override string ToString() => this.Name;

    public override bool Equals(object? obj) => obj is Faction f && f.UID == this.UID;

    public override int GetHashCode() => this.UID;

    #region Static Region

    public static implicit operator string(Faction fac) => fac.Name;

    public static explicit operator Faction(string name) => FromName(name);

    public static bool operator ==(Faction? a, Faction? b) => a is not null && a.Equals(b);
    public static bool operator !=(Faction? a, Faction? b) => !(a == b);


    public const string FactionStrSoviet = "soviet";
    public const string FactionStrAmerican = "aef";
    public const string FactionStrBritish = "british";
    public const string FactionStrGerman = "german";
    public const string FactionStrOKW = "west_german";

    /// <summary>
    /// The Soviet faction.
    /// </summary>
    public static readonly Faction Soviet = new(0, FactionStrSoviet, "racebps\\soviet", true, DLCPack.Base);

    /// <summary>
    /// The Wehrmacht (Ostheer) faction.
    /// </summary>
    public static readonly Faction Wehrmacht = new(1, FactionStrGerman, "racebps\\german", false, DLCPack.Base);

    /// <summary>
    /// The US Forces faction.
    /// </summary>
    public static readonly Faction America = new(2, FactionStrAmerican, "racebps\\aef", true, DLCPack.WesternFrontArmiesUSA);

    /// <summary>
    /// The Oberkommando West faction.
    /// </summary>
    public static readonly Faction OberkommandoWest = new(3, FactionStrOKW, "racebps\\west_german", false, DLCPack.WesternFrontArmiesOKW);

    /// <summary>
    /// The United Kingdom faction.
    /// </summary>
    public static readonly Faction British = new(4, FactionStrBritish, "racebps\\british", true, DLCPack.UKF);

    /// <summary>
    /// Get if some faction is an allied faction.
    /// </summary>
    /// <param name="factionName">The name of the faction to check.</param>
    /// <returns>Will return <see langword="true"/> if <paramref name="factionName"/> is not "german" and not "west_german". Otherwise <see langword="false"/.></returns>
    public static bool IsAlliedFaction(string factionName)
        => factionName is not "german" and not "west_german";

    /// <summary>
    /// Get a <see cref="Faction"/> by its <see cref="string"/> name identifier.
    /// </summary>
    /// <param name="name">The <see cref="string"/> faction name to find.</param>
    /// <returns>One of the five factions.</returns>
    /// <exception cref="ObjectNotFoundException"/>
    public static Faction FromName(string name) {
        var f = TryGetFromName(name);
        if (f is null) {
            throw new ObjectNotFoundException($"Failed to find faction with name '{name}'.");
        }
        return f;
    }

    /// <summary>
    /// Get a <see cref="Faction"/> by its <see cref="string"/> name identifier.
    /// </summary>
    /// <param name="name">The <see cref="string"/> faction name to find.</param>
    /// <returns>One of the five factions or null.</returns>
    public static Faction? TryGetFromName(string name) => name.ToLower(CultureInfo.InvariantCulture) switch {
        "sov" or FactionStrSoviet => Soviet,
        "ger" or FactionStrGerman => Wehrmacht,
        "usa" or FactionStrAmerican => America,
        "okw" or FactionStrOKW => OberkommandoWest,
        "ukf" or FactionStrBritish => British,
        _ => null,
    };

    /// <summary>
    /// Returns the complementary faction of the given faction (eg. Wehrmacht - Soviet).
    /// </summary>
    /// <param name="faction">The faction to find the complement of.</param>
    /// <returns>The comlementary faction.</returns>
    /// <exception cref="ArgumentException"/>
    public static Faction GetComplementaryFaction(Faction faction) {
        if (faction == Soviet) {
            return Wehrmacht;
        } else if (faction == America) {
            return OberkommandoWest;
        } else if (faction == British) {
            return OberkommandoWest;
        } else if (faction == OberkommandoWest) {
            return America;
        } else if (faction == Wehrmacht) {
            return Soviet;
        }
        throw new ArgumentException("Unknown or custom faction - Not allowed!");
    }

    /// <summary>
    /// Check if two factions are on the same team (Both are allied or both are axis)
    /// </summary>
    /// <param name="left">The first faction.</param>
    /// <param name="right">The second faction.</param>
    /// <returns><see langword="true"/> if both are on the same team. Otherwise <see langword="false"/>.</returns>
    public static bool AreSameTeam(Faction left, Faction right) => left.IsAllied == right.IsAllied || left.IsAxis == right.IsAxis;

    /// <summary>
    /// Get a list of all factions.
    /// </summary>
    public static List<Faction> Factions => new() { Soviet, /*British, America, OberkommandoWest,*/ Wehrmacht };

    #endregion

}
