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
    /// Get the locale key for the faction name.
    /// </summary>
    public uint NameKey { get; }

    /// <summary>
    /// Get the locale key for the faction description.
    /// </summary>
    public uint DescKey { get; }

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

    private Faction(byte id, uint namekey, uint descKey, string name, string rbppath, bool isAllied, DLCPack requiredDLC) {
        this.UID = id;
        this.Name = name;
        this.NameKey = namekey;
        this.DescKey = descKey;
        this.RbpPath = rbppath;
        this.IsAllied = isAllied;
        this.RequiredDLC = requiredDLC;
    }

    /// <summary>
    /// Returns a string that represents the current object.
    /// </summary>
    /// <returns>A string that represents the current object.</returns>
    public override string ToString() => this.Name;

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is Faction f && f.UID == this.UID;

    /// <inheritdoc/>
    public override int GetHashCode() => this.UID;

    #region Static Region

    /// <summary>
    /// 
    /// </summary>
    /// <param name="fac"></param>
    public static implicit operator string(Faction fac) => fac.Name;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    public static bool operator ==(Faction? a, Faction? b) => a is not null && a.Equals(b);
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    public static bool operator !=(Faction? a, Faction? b) => !(a == b);

    /// <summary>
    /// 
    /// </summary>
    public const string FactionStrSoviet = "soviet";

    /// <summary>
    /// 
    /// </summary>
    public const string FactionStrAmerican = "aef";
    
    /// <summary>
    /// 
    /// </summary>
    public const string FactionStrBritish = "british";
    
    /// <summary>
    /// 
    /// </summary>
    public const string FactionStrGerman = "german";

    /// <summary>
    /// 
    /// </summary>
    public const string FactionStrGermans = "germans";

    /// <summary>
    /// 
    /// </summary>
    public const string FactionStrOKW = "west_german";

    /// <summary>
    /// 
    /// </summary>
    public const string FactionStrAfrikaKorps = "afrika_korps";

    /// <summary>
    /// 
    /// </summary>
    public const string FactionStrAmericans = "americans";

    /// <summary>
    /// 
    /// </summary>
    public const string FactionStrBritishAfrica = "british_africa";

    /// <summary>
    /// The Soviet faction (CoH2).
    /// </summary>
    public static readonly Faction Soviet = new(0, 2099401, 2099400, FactionStrSoviet, "racebps\\soviet", true, DLCPack.CoH2Base);

    /// <summary>
    /// The Wehrmacht (Ostheer) faction (CoH2).
    /// </summary>
    public static readonly Faction Wehrmacht = new(1, 2099451, 2099450, FactionStrGerman, "racebps\\german", false, DLCPack.CoH2Base);

    /// <summary>
    /// The US Forces faction (CoH2).
    /// </summary>
    public static readonly Faction America = new(2, 11073202, 11073200, FactionStrAmerican, "racebps\\aef", true, DLCPack.WesternFrontArmiesUSA);

    /// <summary>
    /// The Oberkommando West faction (CoH2).
    /// </summary>
    public static readonly Faction OberkommandoWest = new(3, 11073205, 11073203, FactionStrOKW, "racebps\\west_german", false, DLCPack.WesternFrontArmiesOKW);

    /// <summary>
    /// The United Kingdom faction (CoH2).
    /// </summary>
    public static readonly Faction British = new(4, 11078364, 11078365, FactionStrBritish, "racebps\\british", true, DLCPack.UKF);

    /// <summary>
    /// The Wehrmacht (Germans) faction (CoH3).
    /// </summary>
    public static readonly Faction Germans = new(5, 11154248, 11234530, FactionStrGermans, "racebps\\germans", false, DLCPack.CoH3Base);

    /// <summary>
    /// The Afrika Korps faction (CoH3).
    /// </summary>
    public static readonly Faction AfrikaKorps = new(6, 11181964, 11220490, FactionStrAfrikaKorps, "racebps\\afrika_korps", false, DLCPack.CoH3Base);

    /// <summary>
    /// The British Forces (CoH3).
    /// </summary>
    public static readonly Faction BritishAfrica = new(7, 11186599, 11234531, FactionStrBritishAfrica, "racebps\\british_africa", true, DLCPack.CoH3Base);

    /// <summary>
    /// The US Forces faction (CoH3).
    /// </summary>
    public static readonly Faction Americans = new(7, 11154247, 11234529, FactionStrAmericans, "racebps\\americans", true, DLCPack.CoH3Base);

    /// <summary>
    /// Get if some faction is an allied faction.
    /// </summary>
    /// <param name="factionName">The name of the faction to check.</param>
    /// <returns>Will return <see langword="true"/> if <paramref name="factionName"/> is an allied faction. Otherwise <see langword="false"/> when an axis faction.</returns>
    public static bool IsAlliedFaction(string factionName)
        => factionName is not FactionStrAfrikaKorps and not FactionStrGerman and not FactionStrGermans and not FactionStrOKW;

    /// <summary>
    /// Get a <see cref="Faction"/> by its <see cref="string"/> name identifier.
    /// </summary>
    /// <param name="name">The <see cref="string"/> faction name to find.</param>
    /// <returns>One of the five factions.</returns>
    /// <exception cref="ObjectNotFoundException"/>
    [Obsolete("Specify the targetted game")]
    public static Faction FromName(string name) 
        => TryGetFromName(name) ?? throw new ObjectNotFoundException($"Failed to find faction with name '{name}'.");

    /// <summary>
    /// Get a <see cref="Faction"/> by its <see cref="string"/> name identifier.
    /// </summary>
    /// <param name="name">The <see cref="string"/> faction name to find.</param>
    /// <param name="game">The game that the faction can be played in. If <see cref="GameCase.All"/> is specified, CoH2 factions are looked up before CoH3 factions.</param>
    /// <returns>One of the five factions.</returns>
    /// <exception cref="ObjectNotFoundException"/>
    public static Faction FromName(string name, GameCase game) 
        => TryGetFromName(name, game) ?? throw new ObjectNotFoundException($"Game '{game}' does not have a faction with the name '{name}'.");

    /// <summary>
    /// Get a <see cref="Faction"/> by its <see cref="string"/> name identifier.
    /// </summary>
    /// <param name="name">The <see cref="string"/> faction name to find.</param>
    /// <returns>One of the five factions or null.</returns>
    [Obsolete("Specify the targetted game")]
    public static Faction? TryGetFromName(string name) => name.ToLower(CultureInfo.InvariantCulture) switch {
        "sov" or FactionStrSoviet => Soviet,
        "ger" or FactionStrGerman => Wehrmacht,
        "usa" or FactionStrAmerican => America,
        "okw" or FactionStrOKW => OberkommandoWest,
        "ukf" or FactionStrBritish => British,
        "dak" or FactionStrAfrikaKorps => AfrikaKorps,
        FactionStrGermans => Germans,
        FactionStrBritishAfrica => BritishAfrica,
        FactionStrAmericans => Americans,
        _ => null,
    };

    /// <summary>
    /// Get a <see cref="Faction"/> by its <see cref="string"/> name identifier.
    /// </summary>
    /// <param name="name">The <see cref="string"/> faction name to find.</param>
    /// <param name="game">The game that the faction can be played in. If <see cref="GameCase.All"/> is specified, CoH2 factions are looked up before CoH3 factions.</param>
    /// <returns>One of the factions or null.</returns>
    public static Faction? TryGetFromName(string name, GameCase game) => game switch {
        GameCase.CompanyOfHeroes2 => name switch {
            "sov" or FactionStrSoviet => Soviet,
            "ger" or FactionStrGerman => Wehrmacht,
            "usa" or FactionStrAmerican => America,
            "okw" or FactionStrOKW => OberkommandoWest,
            "ukf" or FactionStrBritish => British,
            _ => null,
        },
        GameCase.CompanyOfHeroes3 => name switch {
            "ger" or FactionStrGermans => Germans,
            "dak" or FactionStrAfrikaKorps => AfrikaKorps,
            "ukf" or FactionStrBritishAfrica => BritishAfrica,
            "usf" or FactionStrAmericans => Americans,
            _ => null
        },
        GameCase.All => TryGetFromName(name, GameCase.CompanyOfHeroes2) ?? TryGetFromName(name, GameCase.CompanyOfHeroes3),
        _ => null
    };

    /// <summary>
    /// Returns the complementary faction of the given faction (eg. Wehrmacht - Soviet).
    /// </summary>
    /// <param name="faction">The faction to find the complement of.</param>
    /// <returns>The comlementary faction.</returns>
    /// <exception cref="ArgumentException"/>
    [Obsolete("Specify the targetted game")]
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
        } else if (faction == AfrikaKorps) {
            return BritishAfrica;
        } else if (faction == Germans) {
            return Americans;
        } else if (faction == BritishAfrica) {
            return AfrikaKorps;
        } else if (faction == Americans) {
            return Germans;
        }
        throw new ArgumentException("Unknown or custom faction - Not allowed!");
    }

    /// <summary>
    /// Check if two factions are on the same team (Both are allied or both are axis)
    /// </summary>
    /// <param name="left">The first faction.</param>
    /// <param name="right">The second faction.</param>
    /// <returns><see langword="true"/> if both are on the same team. Otherwise <see langword="false"/>.</returns>
    public static bool AreSameTeam(Faction left, Faction right) 
        => (left.IsAllied == right.IsAllied || left.IsAxis == right.IsAxis);

    /// <summary>
    /// Determines if the two input factions are for the same game.
    /// </summary>
    /// <param name="left">The first faction.</param>
    /// <param name="right">The second faction.</param>
    /// <returns><see langword="true"/> if both are in the same game. Otherwise <see langword="false"/>.</returns>
    public static bool AreSameGame(Faction left, Faction right)
        => left.RequiredDLC.Game == right.RequiredDLC.Game;

    /// <summary>
    /// Get a list of all factions.
    /// </summary>
    [Obsolete("Use the game specific property")]
    public static List<Faction> Factions => new() { Soviet, British, America, OberkommandoWest, Wehrmacht };

    /// <summary>
    /// 
    /// </summary>
    public static IList<Faction> CoH2Factions => new[] { Soviet, British, America, OberkommandoWest, Wehrmacht };

    /// <summary>
    /// 
    /// </summary>
    public static IList<Faction> CoH3Factions => new[] { Germans, AfrikaKorps, BritishAfrica, Americans };

    /// <summary>
    /// 
    /// </summary>
    /// <param name="game"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static IList<Faction> GetFactions(GameCase game) => game switch {
        GameCase.CompanyOfHeroes2 => CoH2Factions,
        GameCase.CompanyOfHeroes3 => CoH3Factions,
        _ => throw new ArgumentException($"Invalid game case {game}", nameof(game))
    };

    #endregion

}
