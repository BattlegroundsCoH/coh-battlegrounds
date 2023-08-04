using System;
using System.Text.Json.Serialization;

using Battlegrounds.Errors.Common;
using Battlegrounds.Functional;
using Battlegrounds.Game;
using Battlegrounds.Game.Blueprints;
using Battlegrounds.Game.Blueprints.Extensions;
using Battlegrounds.Modding.Content.Companies;

namespace Battlegrounds.Modding.Content;

/// <summary>
/// Class representing faction data for a Battlegrounds mod package.
/// </summary>
public class FactionData {

    /// <summary>
    /// 
    /// </summary>
    public readonly struct UnitAbility {

        /// <summary>
        /// 
        /// </summary>
        public string Blueprint { get; init; }

        /// <summary>
        /// 
        /// </summary>
        public FactionAbility[] Abilities { get; init; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Blueprint"></param>
        /// <param name="Abilities"></param>
        [JsonConstructor]
        public UnitAbility(string Blueprint, FactionAbility[] Abilities) {
            this.Blueprint = Blueprint;
            this.Abilities = Abilities;
        }

    }

    /// <summary>
    /// 
    /// </summary>
    public readonly struct Driver {
        
        /// <summary>
        /// 
        /// </summary>
        public string Blueprint { get; init; }

        /// <summary>
        /// 
        /// </summary>
        public string WhenType { get; init; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Blueprint"></param>
        /// <param name="WhenType"></param>
        [JsonConstructor]
        public Driver(string Blueprint, string WhenType) {
            this.Blueprint = Blueprint;
            this.WhenType = WhenType;
        }

    }

    /// <summary>
    /// 
    /// </summary>
    public readonly struct CompanySettings {

        /// <summary>
        /// Get or initialise units to exclude from unit list. (May be used in other contexts, but cannot be added directly to the company).
        /// </summary>
        public string[] ExcludeUnits { get; init; }

        /// <summary>
        /// 
        /// </summary>
        public FactionCompanyType[] Types { get; init; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ExcludeUnits"></param>
        /// <param name="Types"></param>
        [JsonConstructor]
        public CompanySettings(string[]? ExcludeUnits, FactionCompanyType[]? Types) {
            this.ExcludeUnits = ExcludeUnits ?? Array.Empty<string>();
            this.Types = Types ?? Array.Empty<FactionCompanyType>();
        }

    }

    /// <summary>
    /// 
    /// </summary>
    public string Faction { get; init; }

    /// <summary>
    /// 
    /// </summary>
    public Driver[] Drivers { get; init; }

    /// <summary>
    /// 
    /// </summary>
    public FactionAbility[] Abilities { get; init; }

    /// <summary>
    /// 
    /// </summary>
    public UnitAbility[] UnitAbilities { get; init; }

    /// <summary>
    /// 
    /// </summary>
    public string[] Transports { get; init; }

    /// <summary>
    /// 
    /// </summary>
    public string[] TowTransports { get; init; }

    /// <summary>
    /// 
    /// </summary>
    public bool CanHaveParadropInCompanies { get; init; }

    /// <summary>
    /// 
    /// </summary>
    public bool CanHaveGliderInCompanies { get; init; }

    /// <summary>
    /// 
    /// </summary>
    public CompanySettings Companies { get; set; }

    /// <summary>
    /// Get or initialise the blueprint to use when recrewing team weapons
    /// </summary>
    public string TeamWeaponCrew { get; init; }

    /// <summary>
    /// Get or initialise the game case associated with this faction.
    /// </summary>
    public GameCase Game { get; init; }

    /// <summary>
    /// 
    /// </summary>
    [JsonIgnore]
    public IModPackage? Package { get; set; }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="Faction"></param>
    /// <param name="Drivers"></param>
    /// <param name="Abilities"></param>
    /// <param name="UnitAbilities"></param>
    /// <param name="Transports"></param>
    /// <param name="TowTransports"></param>
    /// <param name="CanHaveParadropInCompanies"></param>
    /// <param name="CanHaveGliderInCompanies"></param>
    /// <param name="Companies"></param>
    /// <param name="TeamWeaponCrew"></param>
    /// <param name="Game"></param>
    [JsonConstructor]
    public FactionData(string Faction, 
        Driver[]? Drivers, 
        FactionAbility[]? Abilities, 
        UnitAbility[]? UnitAbilities, string[]? Transports, string[]? TowTransports, bool CanHaveParadropInCompanies, bool CanHaveGliderInCompanies,
        CompanySettings Companies, string TeamWeaponCrew, GameCase Game) {
        
        // Set fields
        this.Faction = Faction;
        this.Drivers = Drivers ?? Array.Empty<Driver>();
        this.Abilities = Abilities ?? Array.Empty<FactionAbility>();
        this.UnitAbilities = UnitAbilities ?? Array.Empty<UnitAbility>();
        this.Transports = Transports ?? Array.Empty<string>();
        this.TowTransports = TowTransports ?? Array.Empty<string>();
        this.CanHaveGliderInCompanies = CanHaveGliderInCompanies;
        this.CanHaveParadropInCompanies = CanHaveParadropInCompanies;
        this.TeamWeaponCrew = TeamWeaponCrew;
        this.Companies = Companies with { 
            Types = Companies.Types.ForEach(x => x.FactionData = this) 
        };

        this.Game = Game is GameCase.Unspecified ? global::Battlegrounds.Game.Gameplay.Faction.TryGetGameFromFactionName(this.Faction) : Game;

    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public string[] GetHiddenSquads() => this.Companies.Types
        .MapNotNull(x => x.TeamWeaponCrew)
        .Concat(this.Companies.ExcludeUnits)
        .Append(this.TeamWeaponCrew);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="types"></param>
    /// <returns></returns>
    /// <exception cref="ObjectNotFoundException"></exception>
    public SquadBlueprint GetDriver(TypeList types) {
        for (int i = 0; i < this.Drivers.Length; i++) {
            if (string.IsNullOrEmpty(this.Drivers[i].WhenType) || types.IsType(this.Drivers[i].WhenType))
                return Package!.GetDataSource().GetBlueprints(Game).FromBlueprintName<SquadBlueprint>(this.Drivers[i].Blueprint);
        }
        throw new ObjectNotFoundException("Failed to find driver blueprint.");
    }

}
