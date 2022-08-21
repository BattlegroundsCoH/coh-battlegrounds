using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text.Json.Serialization;

using Battlegrounds.Functional;
using Battlegrounds.Game.Database;
using Battlegrounds.Game.Database.Management;
using Battlegrounds.Game.Gameplay;
using Battlegrounds.Modding;
using Battlegrounds.Modding.Content.Companies;
using Battlegrounds.Verification;

namespace Battlegrounds.Game.DataCompany;

/// <summary>
/// Enum specifying the availability context of a company.
/// </summary>
public enum CompanyAvailabilityType {

    /// <summary>
    /// May only be used in multiplayer (skirmish)
    /// </summary>
    MultiplayerOnly,

    /// <summary>
    /// May only be used in campaigns.
    /// </summary>
    CampaignOnly,

    /// <summary>
    /// May be used in either campaigns or multiplayer.
    /// </summary>
    AnyMode,

}

/// <summary>
/// Represents a Company. Implements <see cref="IChecksumItem"/>.
/// </summary>
[JsonConverter(typeof(CompanySerializer))]
public class Company : IChecksumItem {

    /// <summary>
    /// The maximum size of a company.
    /// </summary>
    public const int MAX_SIZE = 40;

    /// <summary>
    /// The max amount of initially deployed units.
    /// </summary>
    public const int DEFAULT_INITIAL = 5;

    /// <summary>
    /// The max amount of abilities available to a company.
    /// </summary>
    public const int MAX_ABILITY = 6;

    /// <summary>
    /// The maximum rating value achievable
    /// </summary>
    public const double MAX_RATING = MAX_SIZE * 5.0;

    private ulong m_checksum;
    private string m_lastEditVersion;
    private ushort m_nextSquadId;
    private CompanyAvailabilityType m_availabilityType;
    private readonly List<Squad> m_squads;
    private readonly List<Blueprint> m_inventory;
    private readonly List<Modifier> m_modifiers;
    private readonly List<UpgradeBlueprint> m_upgrades;
    private readonly List<Ability> m_abilities;
    private CompanyStatistics m_companyStatistics;
    private bool m_autoReplenish;

    /// <summary>
    /// Get the name of the company.
    /// </summary>
    [ChecksumProperty]
    public string Name { get; set; }

    /// <summary>
    /// Get the calculated strength of the company.
    /// </summary>
    /// <remarks>
    /// Valid values within the closed interval [0, <see cref="MAX_RATING"/>]
    /// </remarks>
    public double Strength => Math.Round(this.GetStrength(), 2);

    /// <summary>
    /// Get the rating of the company.
    /// </summary>
    [JsonIgnore]
    public double Rating => Math.Round(this.GetStrength() / MAX_RATING, 2);

    /// <summary>
    /// Get the <see cref="CompanyType"/> that can be used to describe the <see cref="Company"/> characteristics.
    /// </summary>
    [ChecksumProperty]
    public FactionCompanyType Type { get; init; }

    /// <summary>
    /// Get the <see cref="CompanyAvailabilityType"/> that will determine when the company is available.
    /// </summary>
    [ChecksumProperty]
    public CompanyAvailabilityType AvailabilityType => this.m_availabilityType;

    /// <summary>
    /// Get the version that was used to generate this company.
    /// </summary>
    [ChecksumProperty]
    public string AppVersion => this.m_lastEditVersion;

    /// <summary>
    /// Get tthe GUID of the tuning mod used to create this company.
    /// </summary>
    [ChecksumProperty]
    public ModGuid TuningGUID { get; set; }

    /// <summary>
    /// Get the <see cref="Faction"/> this company is associated with.
    /// </summary>
    [ChecksumProperty]
    public Faction Army { get; }

    /// <summary>
    /// Get the display name of who owns the <see cref="Company"/>. This property is used by the compilers. This will be overriden.
    /// </summary>
    public string Owner { get; set; } = string.Empty;

    /// <summary>
    /// Get the <see cref="ImmutableArray{T}"/> representation of the units in the <see cref="Company"/>.
    /// </summary>
    [ChecksumProperty(IsCollection = true)]
    public Squad[] Units => this.m_squads.ToArray();

    /// <summary>
    /// Get the <see cref="ImmutableArray{T}"/> representation of a <see cref="Company"/> inventory of stored <see cref="Blueprint"/> objects.
    /// </summary>
    [ChecksumProperty(IsCollection = true)]
    public ImmutableArray<Blueprint> Inventory => this.m_inventory.ToImmutableArray();

    /// <summary>
    /// Get the <see cref="ImmutableArray{T}"/> representation of a <see cref="Company"/>'s upgrade list.
    /// </summary>
    [ChecksumProperty(IsCollection = true)]
    public ImmutableArray<UpgradeBlueprint> Upgrades => this.m_upgrades.ToImmutableArray();

    /// <summary>
    /// Get the <see cref="ImmutableArray{T}"/> representation of a <see cref="Company"/>'s modifier list.
    /// </summary>
    [ChecksumProperty(IsCollection = true)]
    public ImmutableArray<Modifier> Modifiers => this.m_modifiers.ToImmutableArray();

    /// <summary>
    /// Get the <see cref="ImmutableArray{T}"/> representation of a <see cref="Company"/>'s special ability list.
    /// </summary>
    [ChecksumProperty(IsCollection = true)]
    public ImmutableArray<Ability> Abilities => this.m_abilities.ToImmutableArray();

    /// <summary>
    /// Get the statistics tied to the <see cref="Company"/>.
    /// </summary>
    [ChecksumProperty]
    public CompanyStatistics Statistics => this.m_companyStatistics;

    /// <summary>
    /// Get the calculated company checksum.
    /// </summary>
    public ulong Checksum => this.m_checksum;

    /// <summary>
    /// Get if the company should automatically replace lost units
    /// </summary>
    [ChecksumProperty]
    public bool AutoReplenish => this.m_autoReplenish;

    /// <summary>
    /// New empty <see cref="Company"/> instance.
    /// </summary>
    internal Company(Faction faction, FactionCompanyType companyType) {
        this.Army = faction;
        this.Type = companyType;
        this.Name = "Untitled Company";
        this.m_squads = new List<Squad>();
        this.m_inventory = new List<Blueprint>();
        this.m_modifiers = new List<Modifier>();
        this.m_upgrades = new List<UpgradeBlueprint>();
        this.m_abilities = new List<Ability>();
        this.m_companyStatistics = new CompanyStatistics();
        this.m_lastEditVersion = BattlegroundsInstance.BG_VERSION;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    public void AddSquad(UnitBuilder builder) {
        if (builder.HasIndex) {
            this.m_squads.Add(builder.Commit((ushort)0).Result);
        } else {
            this.m_squads.Add(builder.Commit(this.m_nextSquadId++).Result);
        }
    }

    /// <summary>
    /// Get the company <see cref="Squad"/> by its company index.
    /// </summary>
    /// <param name="squadID">The index of the squad to get</param>
    /// <returns>The squad with squad id matching requested squad ID or null.</returns>
    public Squad? GetSquadByIndex(ushort squadID) {
        Squad? s = this.m_squads.FirstOrDefault(x => x.SquadID == squadID);
        if (s is null) { 
            return this.m_squads.FirstOrDefault(x => x.Crew is not null ? x.Crew.SquadID == squadID : false)?.Crew; 
        }
        return s;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="squadId"></param>
    public bool RemoveSquad(ushort squadId)
        => this.m_squads.RemoveAll(x => x.SquadID == squadId) == 1;

    /// <summary>
    /// Reset the squads of the company.
    /// </summary>
    public void ResetSquads() {
        this.m_squads.Clear();
        this.m_nextSquadId = 0;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="blueprint"></param>
    public void AddInventoryItem(Blueprint blueprint) => this.m_inventory.Add(blueprint);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="blueprint"></param>
    public void RemoveInventoryItem(Blueprint blueprint) {
        if (this.m_inventory.FirstOrDefault(x => x.Equals(blueprint)) is Blueprint bp) {
            this.m_inventory.Remove(bp);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="ability"></param>
    public void AddAbility(Ability ability) => this.m_abilities.Add(ability);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="ability"></param>
    /// <returns></returns>
    public bool RemoveAbility(Ability ability) => this.m_abilities.Remove(ability);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="ability"></param>
    /// <returns></returns>
    public bool RemoveAbility(string ability)
        => this.m_abilities.FirstOrDefault(x => x.ABP.Name == ability) is Ability abp && this.m_abilities.Remove(abp);

    /// <summary>
    /// Reset most of the company data, including the company inventory.
    /// </summary>
    public void ResetCompany() => this.ResetCompany(true);

    /// <summary>
    /// Reset most of the company data.
    /// </summary>
    /// <param name="resetInventory"></param>
    public void ResetCompany(bool resetInventory) {
        this.ResetSquads();
        this.m_abilities.Clear();
        if (resetInventory) {
            this.m_inventory.Clear();
        }
        this.Name = string.Empty;
        this.m_checksum = 0;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public bool VerifyAppVersion() => this.m_lastEditVersion is BattlegroundsInstance.BG_VERSION;

    public bool VerifyChecksum(ulong checksum)
        => this.m_checksum == checksum;

    public void CalculateChecksum()
        => this.m_checksum = new Checksum(this).GetCheckksum();

    /// <summary>
    /// 
    /// </summary>
    /// <param name="companyAvailability"></param>
    public void SetAvailability(CompanyAvailabilityType companyAvailability) => this.m_availabilityType = companyAvailability;

    /// <summary>
    /// Calculate the strength of the company
    /// </summary>
    /// <returns>The calculated strength of the company based on squad veterancy levels relative to size of company and win rate</returns>
    public double GetStrength() {

        // Get unit weight
        var weight = this.m_squads.Count / (double)MAX_SIZE;

        // Return total
        return this.m_squads.Aggregate(0.0, (a, b) => a + (b.VeterancyRank * weight)) * this.m_companyStatistics.WinRate;

    }

    /// <summary>
    /// Returns a string that represents the current object.
    /// </summary>
    /// <returns>A string that represents the current object.</returns>
    public override string ToString() => this.Name;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="updateFunction"></param>
    public void UpdateStatistics(Func<CompanyStatistics, CompanyStatistics> updateFunction)
        => this.m_companyStatistics = updateFunction(this.m_companyStatistics);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="squadIndex"></param>
    /// <param name="squad"></param>
    public bool ReplaceSquad(ushort squadIndex, Squad squad) {
        int arrIndex = this.m_squads.FindIndex(x => x.SquadID == squadIndex);
        if (arrIndex is -1) {
            return false;
        }
        this.m_squads[arrIndex] = squad;
        return true;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="version"></param>
    public void SetAppVersion(string version) => this.m_lastEditVersion = version;

    /// <summary>
    /// Set if the company should automatically replenish lost units.
    /// </summary>
    /// <param name="autoReinforce">Boolean flag setting if this behaviour is enabled or not.</param>
    public void SetReinforcementsEnabled(bool autoReinforce) => this.m_autoReplenish = autoReinforce;

    /// <summary>
    /// Get all special unit abilities available in the company.
    /// </summary>
    /// <returns>An array of <see cref="Ability"/> instances that are made available by units in the company.</returns>
    /// <exception cref="InvalidOperationException"/>
    public Ability[] GetSpecialUnitAbilities() {

        // Get the relevant mod package
        var package = ModManager.GetPackageFromGuid(this.TuningGUID);
        if (package is null) {
            throw new InvalidOperationException();
        }

        // Return from this
        return GetSpecialUnitAbilities(this.Army, package, this.m_squads.Select(x => x.SBP).Distinct().ToArray());

    }

    /// <summary>
    /// Get all special unit abilities based on blueprint collection.
    /// </summary>
    /// <param name="faction">The faction to collect unit abilities from.</param>
    /// <param name="mod">The mod to collect unit abilities from.</param>
    /// <param name="squadBlueprints">The blueprints of units who grant unit abilities.</param>
    /// <returns>Array of special abilities that <paramref name="squadBlueprints"/> bring.</returns>
    public static Ability[] GetSpecialUnitAbilities(Faction faction, ModPackage mod, IEnumerable<SquadBlueprint> squadBlueprints) {

        // Get unit abilities for faction
        var uabps = mod.FactionSettings[faction].UnitAbilities;

        // Select all relevant abilities
        var abps = uabps.Filter(x => squadBlueprints.Any(y => y.Name == x.Blueprint))
            .MapAndFlatten(x => x.Abilities)
            .Map(x => (data: x, bp: BlueprintManager.FromBlueprintName<AbilityBlueprint>(x.Blueprint)))
            .Filter(x => x.bp is not null);

        // Return array of ability instances
        return abps.Map(x => {
            var category = x.data.AbilityCategory;
            int use = x.data.MaxUsePerMatch;
            string[] units = uabps.Filter(y => y.Abilities.Any(z => z.Blueprint == x.bp.Name)).Map(y => y.Blueprint);
            return new Ability(x.bp, x.data.LockoutBlueprint, units, category, use, 0);
        });

    }

}
