using System;
using System.Text.Json.Serialization;

using Battlegrounds.ErrorHandling.CommonExceptions;
using Battlegrounds.Functional;
using Battlegrounds.Game.Database;
using Battlegrounds.Game.Database.Extensions;
using Battlegrounds.Game.Database.Management;
using Battlegrounds.Modding.Content.Companies;

namespace Battlegrounds.Modding.Content;

/// <summary>
/// Class representing faction data for a Battlegrounds mod package.
/// </summary>
public class FactionData {

    public readonly struct UnitAbility {

        public string Blueprint { get; init; }

        public FactionAbility[] Abilities { get; init; }

        [JsonConstructor]
        public UnitAbility(string Blueprint, FactionAbility[] Abilities) {
            this.Blueprint = Blueprint;
            this.Abilities = Abilities;
        }

    }

    public readonly struct Driver {

        public string Blueprint { get; init; }
        public string WhenType { get; init; }

        [JsonConstructor]
        public Driver(string Blueprint, string WhenType) {
            this.Blueprint = Blueprint;
            this.WhenType = WhenType;
        }

    }

    public readonly struct CompanySettings {

        /// <summary>
        /// Get or initialise units to exclude from unit list. (May be used in other contexts, but cannot be added directly to the company).
        /// </summary>
        public string[] ExcludeUnits { get; init; }

        public FactionCompanyType[] Types { get; init; }

        [JsonConstructor]
        public CompanySettings(string[]? ExcludeUnits, FactionCompanyType[]? Types) {
            this.ExcludeUnits = ExcludeUnits ?? Array.Empty<string>();
            this.Types = Types ?? Array.Empty<FactionCompanyType>();
        }

    }

    public string Faction { get; init; }

    public Driver[] Drivers { get; init; }

    public FactionAbility[] Abilities { get; init; }

    public UnitAbility[] UnitAbilities { get; init; }

    public string[] Transports { get; init; }

    public string[] TowTransports { get; init; }

    public bool CanHaveParadropInCompanies { get; init; }

    public bool CanHaveGliderInCompanies { get; init; }

    public CompanySettings Companies { get; set; }

    /// <summary>
    /// Get or initialise the blueprint to use when recrewing team weapons
    /// </summary>
    public string TeamWeaponCrew { get; init; }

    [JsonIgnore]
    public ModPackage? Package { get; set; }

    [JsonConstructor]
    public FactionData(string Faction, 
        Driver[] Drivers, 
        FactionAbility[] Abilities, 
        UnitAbility[] UnitAbilities, string[] Transports, string[] TowTransports, bool CanHaveParadropInCompanies, bool CanHaveGliderInCompanies,
        CompanySettings Companies, string TeamWeaponCrew) {
        
        // Set fields
        this.Faction = Faction;
        this.Drivers = Drivers;
        this.Abilities = Abilities;
        this.UnitAbilities = UnitAbilities;
        this.Transports = Transports;
        this.TowTransports = TowTransports;
        this.CanHaveGliderInCompanies = CanHaveGliderInCompanies;
        this.CanHaveParadropInCompanies = CanHaveParadropInCompanies;
        this.TeamWeaponCrew = TeamWeaponCrew;
        this.Companies = Companies with { 
            Types = Companies.Types.ForEach(x => x.FactionData = this) 
        };

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
                return BlueprintManager.FromBlueprintName<SquadBlueprint>(this.Drivers[i].Blueprint);
        }
        throw new ObjectNotFoundException("Failed to find driver blueprint.");
    }

}
