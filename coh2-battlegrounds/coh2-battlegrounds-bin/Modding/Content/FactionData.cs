using System.Collections.Generic;
using System.Text.Json.Serialization;

using Battlegrounds.Modding.Content.Companies;

namespace Battlegrounds.Modding.Content;

/// <summary>
/// Readonly struct representing faction data for a Battlegrounds mod package.
/// </summary>
public readonly struct FactionData {

    public readonly struct UnitAbility {

        public string Blueprint { get; }

        public FactionAbility[] Abilities { get; }

        [JsonConstructor]
        public UnitAbility(string Blueprint, FactionAbility[] Abilities) {
            this.Blueprint = Blueprint;
            this.Abilities = Abilities;
        }

    }

    public readonly struct Driver {

        public string Blueprint { get; }
        public string WhenType { get; }

        [JsonConstructor]
        public Driver(string Blueprint, string WhenType) {
            this.Blueprint = Blueprint;
            this.WhenType = WhenType;
        }

    }

    public readonly struct CompanySettings {

        public Dictionary<string, FactionCompanyType> Types { get; }

        [JsonConstructor]
        public CompanySettings(Dictionary<string, FactionCompanyType> Types) {
            this.Types = Types;
        }

    }

    public string Faction { get; }

    public Driver[] Drivers { get; }

    public FactionAbility[] Abilities { get; }

    public UnitAbility[] UnitAbilities { get; }

    public string[] Transports { get; }

    public string[] TowTransports { get; }

    public bool CanHaveParadropInCompanies { get; }

    public bool CanHaveGliderInCompanies { get; }

    public CompanySettings Companies { get; }

    [JsonConstructor]
    public FactionData(string Faction, 
        Driver[] Drivers, 
        FactionAbility[] Abilities, 
        UnitAbility[] UnitAbilities, string[] Transports, string[] TowTransports, bool CanHaveParadropInCompanies, bool CanHaveGliderInCompanies,
        CompanySettings Companies) {
        
        // Set fields
        this.Faction = Faction;
        this.Drivers = Drivers;
        this.Abilities = Abilities;
        this.UnitAbilities = UnitAbilities;
        this.Transports = Transports;
        this.TowTransports = TowTransports;
        this.CanHaveGliderInCompanies = CanHaveGliderInCompanies;
        this.CanHaveParadropInCompanies = CanHaveParadropInCompanies;
        this.Companies = Companies;

    }

}
