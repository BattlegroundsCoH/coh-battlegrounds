using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Immutable;

using Battlegrounds.Game.Database;
using Battlegrounds.Json;

namespace Battlegrounds.Game.Gameplay {

    /// <summary>
    /// The method in which to deploy a <see cref="Squad"/>.
    /// </summary>
    public enum DeploymentMethod {

        /// <summary>
        /// No special method is defined (units walks unto the battlefield)
        /// </summary>
        None,

        /// <summary>
        /// The unit is transported unto the battlefield using the support blueprint. The transport will then leave the battlefield.
        /// </summary>
        DeployAndExit,

        /// <summary>
        /// The unit will be transported unto the battlefield using the support blueprint.
        /// </summary>
        DeployAndStay,

        /// <summary>
        /// The unit is dropped from the skies using a parachute.
        /// </summary>
        Paradrop,

        /// <summary>
        /// The unit is deployed using a glider.
        /// </summary>
        Glider,

    }

    /// <summary>
    /// The phase in which a <see cref="Squad"/> can be deployed.
    /// </summary>
    public enum DeploymentPhase {

        /// <summary>
        /// No phase - <see cref="Squad"/> may not be deployed in any standard way.
        /// </summary>
        PhaseNone,

        /// <summary>
        /// Deployed in the initial starting phase of a match.
        /// </summary>
        PhaseInitial,

        /// <summary>
        /// Deployment is available in the first (early-game) phase.
        /// </summary>
        PhaseA,

        /// <summary>
        /// Deployment is available in the second (mid-game) phase.
        /// </summary>
        PhaseB,

        /// <summary>
        /// Deployment is available in the third and final (late-game) phase.
        /// </summary>
        PhaseC,

    }

    /// <summary>
    /// Representation of a Squad. Implements <see cref="IJsonObject"/>.
    /// </summary>
    public class Squad : IJsonObject {

        [JsonIgnoreIfValue((byte)0)]private byte m_veterancyRank;
        [JsonIgnoreIfValue(0.0f)] private float m_veterancyProgress;

        [JsonIgnoreIfValue(false)] private bool m_isCrewSquad;
        [JsonIgnoreIfValue(DeploymentMethod.None)][JsonEnum(typeof(DeploymentMethod))] private DeploymentMethod m_deployMode;
        [JsonIgnoreIfValue(DeploymentPhase.PhaseNone)][JsonEnum(typeof(DeploymentPhase))] private DeploymentPhase m_deployPhase;
        [JsonIgnoreIfNull] private Squad m_crewSquad;
        [JsonReference(typeof(BlueprintManager))][JsonIgnoreIfNull] private Blueprint m_deployBp;

        [JsonReference(typeof(BlueprintManager))][JsonIgnoreIfEmpty] private HashSet<Blueprint> m_upgrades;
        [JsonReference(typeof(BlueprintManager))][JsonIgnoreIfEmpty] private HashSet<Blueprint> m_slotItems;
        [JsonIgnoreIfEmpty] private HashSet<Modifier> m_modifiers;

        /// <summary>
        /// The unique squad ID used to identify the <see cref="Squad"/>.
        /// </summary>
        public ushort SquadID { get; }

        /// <summary>
        /// The player who (currently) owns the <see cref="Squad"/>.
        /// </summary>
        [JsonIgnore]
        public Player PlayerOwner { get; }

        /// <summary>
        /// The (crew if squad is a vehicle) <see cref="Database.Blueprint"/> the <see cref="Squad"/> is a type of.
        /// </summary>
        [JsonReference(typeof(BlueprintManager))]
        [JsonIgnoreIfNull]
        public Blueprint Blueprint { get; }

        /// <summary>
        /// The squad or entity <see cref="Database.Blueprint"/> to support this squad.
        /// </summary>
        [JsonIgnore]
        public Blueprint SupportBlueprint => this.m_deployBp;

        /// <summary>
        /// The method to use when deploying a <see cref="Squad"/>.
        /// </summary>
        [JsonIgnore]
        public DeploymentMethod DeploymentMethod => this.m_deployMode;

        /// <summary>
        /// The phase in which a squad can be deployed.
        /// </summary>
        [JsonIgnore]
        public DeploymentPhase DeploymentPhase => this.m_deployPhase;

        /// <summary>
        /// The squad data for the crew.
        /// </summary>
        [JsonIgnore]
        public Squad Crew => this.m_crewSquad;

        /// <summary>
        /// Is the <see cref="Squad"/> the crew for another <see cref="Squad"/> instance.
        /// </summary>
        [JsonIgnore]
        public bool IsCrew => this.m_isCrewSquad;

        /// <summary>
        /// The <see cref="Blueprint"/> in a <see cref="SquadBlueprint"/> form.
        /// </summary>
        /// <exception cref="InvalidCastException"/>
        [JsonIgnore]
        public SquadBlueprint SBP => this.Blueprint as SquadBlueprint;

        /// <summary>
        /// The achieved veterancy rank of a <see cref="Squad"/>.
        /// </summary>
        [JsonIgnore]
        public byte VeterancyRank => this.m_veterancyRank;

        /// <summary>
        /// The current veterancy progress of a <see cref="Squad"/>.
        /// </summary>
        [JsonIgnore]
        public float VeterancyProgress => this.m_veterancyProgress;

        /// <summary>
        /// The current upgrades applied to a <see cref="Squad"/>.
        /// </summary>
        [JsonIgnore]
        public ImmutableHashSet<Blueprint> Upgrades => m_upgrades.ToImmutableHashSet();

        /// <summary>
        /// The current slot items carried by the <see cref="Squad"/>.
        /// </summary>
        [JsonIgnore]
        public ImmutableHashSet<Blueprint> SlotItems => m_slotItems.ToImmutableHashSet();

        /// <summary>
        /// The current modifiers applied to the <see cref="Squad"/>.
        /// </summary>
        [JsonIgnore]
        public ImmutableHashSet<Modifier> Modifiers => m_modifiers.ToImmutableHashSet();

        /// <summary>
        /// Create a basic <see cref="Squad"/> instance without any identifying values.
        /// </summary>
        public Squad() {
            this.SquadID = 0;
            this.PlayerOwner = null;
            this.Blueprint = null;
            this.m_slotItems = new HashSet<Blueprint>();
            this.m_upgrades = new HashSet<Blueprint>();
            this.m_modifiers = new HashSet<Modifier>();
            this.m_deployMode = DeploymentMethod.None;
            this.m_deployPhase = DeploymentPhase.PhaseNone;
            this.m_crewSquad = null;
        }

        /// <summary>
        /// Create new <see cref="Squad"/> instance with a unique squad ID, a <see cref="Player"/> owner and a <see cref="Database.Blueprint"/>.
        /// </summary>
        /// <param name="squadID">The unique squad ID used to identify the squad</param>
        /// <param name="owner">The <see cref="Player"/> who owns the squad</param>
        /// <param name="squadBlueprint">The <see cref="Database.Blueprint"/> the squad is an instance of</param>
        public Squad(ushort squadID, Player owner, Blueprint squadBlueprint) {
            this.SquadID = squadID;
            this.PlayerOwner = owner;
            this.Blueprint = squadBlueprint;
            this.m_slotItems = new HashSet<Blueprint>();
            this.m_upgrades = new HashSet<Blueprint>();
            this.m_modifiers = new HashSet<Modifier>();
            this.m_deployMode = DeploymentMethod.None;
            this.m_deployPhase = DeploymentPhase.PhaseNone;
            this.m_crewSquad = null;
        }

        /// <summary>
        /// Set the veterancy of the <see cref="Squad"/>. The rank and progress is not checked with the blueprint - any veterancy level can be achieved here.
        /// </summary>
        /// <param name="rank">The rank (or level) the squad has achieved.</param>
        /// <param name="progress">The current progress towards the next veterancy level</param>
        public void SetVeterancy(byte rank, float progress = 0.0f) {
            this.m_veterancyRank = rank;
            this.m_veterancyProgress = progress;
        }

        /// <summary>
        /// Set the deployment method used by a <see cref="Squad"/>.
        /// </summary>
        /// <param name="transportBlueprint">The <see cref="Database.Blueprint"/> to use as transport unit.</param>
        /// <param name="deployMode">The mode used to deploy a <see cref="Squad"/>.</param>
        /// <param name="phase">The deployment phase</param>
        public void SetDeploymentMethod(Blueprint transportBlueprint, DeploymentMethod deployMode, DeploymentPhase phase) {
            this.m_deployMode = deployMode;
            this.m_deployBp = transportBlueprint;
            this.m_deployPhase = phase;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="crew"></param>
        public void SetCrew(Squad crew) {
            this.m_crewSquad = crew;
            this.m_crewSquad.SetIsCrew(true);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="isCrew"></param>
        public void SetIsCrew(bool isCrew) => m_isCrewSquad = isCrew;

        /// <summary>
        /// Add an upgrade to the squad
        /// </summary>
        /// <param name="upgradeBP">The upgrade blueprint to add</param>
        public void AddUpgrade(Blueprint upgradeBP) => this.m_upgrades.Add(upgradeBP);

        /// <summary>
        /// Add a slot item to the squad
        /// </summary>
        /// <param name="slotItemBP">The slot item blueprint to add</param>
        public void AddSlotItem(Blueprint slotItemBP) => this.m_slotItems.Add(slotItemBP);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="modifier"></param>
        public void AddModifier(Modifier modifier) => this.m_modifiers.Add(modifier);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="modifierName"></param>
        public void RemoveModifier(string modifierName) => this.m_modifiers.RemoveWhere(x => x.Name.CompareTo(modifierName) == 0);

        /// <summary>
        /// Calculate the actual cost of a <see cref="Squad"/>.
        /// </summary>
        /// <returns>The cost of the squad.</returns>
        /// <exception cref="NullReferenceException"/>
        public Cost GetCost() {

            Cost c = new Cost(SBP.Cost.Manpower, SBP.Cost.Munitions, SBP.Cost.Fuel, SBP.Cost.FieldTime);
            c = this.m_upgrades.Select(x => (x as UpgradeBlueprint).Cost).Aggregate(c, (a, b) => a + b);

            if (this.m_deployBp is SquadBlueprint sbp) {
                c += sbp.Cost * (this.DeploymentMethod == DeploymentMethod.DeployAndExit ? 0.25f : 0.65f);
            }

            // TODO: More here

            return c;

        }

        /// <summary>
        /// Get the category the squad belongs to. This may change depending on certain parameters.
        /// </summary>
        /// <param name="simplified">Use a simplified (3) category system (team_weapon, vehicle, or infantry).</param>
        /// <returns>The matching category in string format. If no matching category is found, 'infantry' or 'main_infantry' is returned depending on the simplified parameter.</returns>
        public string GetCategory(bool simplified) {

            if (simplified) {

                if (this.SBP.IsAntiTank || this.SBP.IsHeavyArtillery || this.SBP.Types.Contains("mortar") || this.SBP.Types.Contains("hmg")) {
                    return "team_weapon";
                } else if (this.SBP.Types.Contains("vehicle")) {
                    return "vehicle";
                } else {
                    return "infantry";
                }

            } else {
                return "main_infantry";
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public string ToJsonReference() => this.SquadID.ToString();

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString() => $"{this.SBP.Name}${this.SquadID}";
        
    }

}
