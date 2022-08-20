using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Battlegrounds.Modding.Content.Companies;

public class FactionCompanyType {

    public readonly struct CostModifier {
        public float Manpower { get; }
        public float Munition { get; }
        public float Fuel { get; }
        [JsonConstructor]
        public CostModifier(float Manpower, float Munition, float Fuel) {
            this.Manpower = Manpower;
            this.Munition = Munition;
            this.Fuel = Fuel;
        }
    }

    public readonly struct TransportOption {
        public float CostModifier { get; }
        [JsonConstructor]
        public TransportOption(float CostModifier) {
            this.CostModifier = CostModifier;
        }
    }

    public class Phase {

        public int ActivationTime { get; init; }

        /// <summary>
        /// Get the additional deployment delay applied to unlocked units
        /// </summary>
        public float DeployDelay { get; init; }

        public CostModifier ResourceIncomeModifier { get; init; }

        /// <summary>
        /// Get the cost modifier applied to units that are unlocked in the previous phase
        /// </summary>
        public CostModifier UnitCostModifier { get; init; }

        public string[] Unlocks { get; init; }

        [JsonConstructor]
        public Phase(int ActivationTime, float DeployDelay, CostModifier ResourceIncomeModifier, CostModifier UnitCostModifier, string[] Unlocks) {
            this.ActivationTime = ActivationTime;
            this.DeployDelay = DeployDelay;
            this.ResourceIncomeModifier = ResourceIncomeModifier;
            this.UnitCostModifier = UnitCostModifier;
            this.Unlocks = Unlocks;
        }

    }

    public string Id { get; init; }

    public string Icon { get; init; }

    public int MaxInfantry { get; init; }

    public int MaxTeamWeapons { get; init; }
    
    public int MaxVehicles { get; init; }
    
    public int MaxLeaders { get; init; }

    public int MaxAbilities { get; init; }

    public string[] Exclude { get; init; }

    public string[] DeployTypes { get; init; }

    public Dictionary<string, TransportOption> DeployBlueprints { get; init; }

    public Dictionary<string, Phase> Phases { get; init; }

    [JsonConstructor]
    public FactionCompanyType(string Id, string Icon, 
        int MaxInfantry, int MaxTeamWeapons, int MaxVehicles, int MaxLeaders, int MaxAbilities, 
        string[] Exclude, string[] DeployTypes, Dictionary<string, TransportOption> DeployBlueprints, Dictionary<string, Phase> Phases) {

        // Set properties
        this.Id = Id;
        this.Icon = Icon;
        this.MaxLeaders = MaxLeaders;
        this.MaxInfantry = MaxInfantry;
        this.MaxVehicles = MaxVehicles;
        this.MaxTeamWeapons = MaxTeamWeapons;
        this.Exclude = Exclude;
        this.DeployTypes = DeployTypes;
        this.DeployBlueprints = DeployBlueprints;
        this.Phases = Phases;
        this.MaxAbilities = MaxAbilities;
    }

}
