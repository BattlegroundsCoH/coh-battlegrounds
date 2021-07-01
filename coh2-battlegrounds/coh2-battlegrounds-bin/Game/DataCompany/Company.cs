using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

using Battlegrounds.Game.Database;
using Battlegrounds.Game.Gameplay;
using Battlegrounds.Json;
using Battlegrounds.Steam;
using Battlegrounds.Verification;

namespace Battlegrounds.Game.DataCompany {

    /// <summary>
    /// Represents a Company. Implements <see cref="IJsonObject"/> and <see cref="IChecksumItem"/>.
    /// </summary>
    public class Company : IJsonObject, IChecksumItem {

        /// <summary>
        /// The maximum size of a company.
        /// </summary>
        public const int MAX_SIZE = 40;

        #region Private Fields
        private string m_checksum;
        private string m_lastEditVersion;
        private ushort m_nextSquadId;
        private List<Squad> m_squads;
        private List<Blueprint> m_inventory;
        private List<Modifier> m_modifiers;
        private List<UpgradeBlueprint> m_upgrades;
        private List<SpecialAbility> m_abilities;
        private CompanyType m_companyType;
        private CompanyAvailabilityType m_availabilityType;
        private Faction m_companyArmy;
        private CompanyStatistics m_companyStatistics;
        #endregion

        /// <summary>
        /// The name of the company.
        /// </summary>
        public string Name { get; set; }

        [JsonIgnore]
        public double Strength => this.GetStrength();

        /// <summary>
        /// The <see cref="CompanyType"/> that can be used to describe the <see cref="Company"/> characteristics.
        /// </summary>
        [JsonBackingField(nameof(m_companyType))]
        [JsonEnum(typeof(CompanyType))]
        public CompanyType Type => this.m_companyType;

        /// <summary>
        /// The <see cref="CompanyAvailabilityType"/> that will determine when the company is available.
        /// </summary>
        [JsonBackingField(nameof(m_availabilityType))]
        [JsonEnum(typeof(CompanyAvailabilityType))]
        public CompanyAvailabilityType AvailabilityType => this.m_availabilityType;

        /// <summary>
        /// The version that was used to generate this company.
        /// </summary>
        [JsonBackingField(nameof(m_lastEditVersion))]
        public string AppVersion => this.m_lastEditVersion;

        /// <summary>
        /// The GUID of the tuning mod used to create this company.
        /// </summary>
        public string TuningGUID { get; set; }

        /// <summary>
        /// The <see cref="Faction"/> this company is associated with.
        /// </summary>
        [JsonBackingField(nameof(m_companyArmy))]
        [JsonReference(typeof(Faction))]
        public Faction Army => this.m_companyArmy;

        /// <summary>
        /// The display name of who owns the <see cref="Company"/>. This property is used by the compilers. This will be overriden.
        /// </summary>
        [JsonIgnore]
        public string Owner { get; set; } = string.Empty;

        /// <summary>
        /// <see cref="ImmutableArray{T}"/> representation of the units in the <see cref="Company"/>.
        /// </summary>
        [JsonBackingField(nameof(m_squads))]
        [JsonIgnoreIfEmpty]
        public ImmutableArray<Squad> Units => this.m_squads.ToImmutableArray();

        /// <summary>
        /// <see cref="ImmutableArray{T}"/> representation of a <see cref="Company"/> inventory of stored <see cref="Blueprint"/> objects.
        /// </summary>
        [JsonBackingField(nameof(m_inventory))]
        [JsonIgnoreIfEmpty]
        public ImmutableArray<Blueprint> Inventory => this.m_inventory.ToImmutableArray();

        /// <summary>
        /// <see cref="ImmutableArray{T}"/> representation of a <see cref="Company"/>'s upgrade list.
        /// </summary>
        [JsonBackingField(nameof(m_upgrades))]
        [JsonIgnoreIfEmpty]
        public ImmutableArray<UpgradeBlueprint> Upgrades => this.m_upgrades.ToImmutableArray();

        /// <summary>
        /// <see cref="ImmutableArray{T}"/> representation of a <see cref="Company"/>'s modifier list.
        /// </summary>
        [JsonBackingField(nameof(m_modifiers))]
        [JsonIgnoreIfEmpty]
        public ImmutableArray<Modifier> Modifiers => this.m_modifiers.ToImmutableArray();

        /// <summary>
        /// <see cref="ImmutableArray{T}"/> representation of a <see cref="Company"/>'s special ability list.
        /// </summary>
        [JsonBackingField(nameof(m_abilities))]
        [JsonIgnoreIfEmpty]
        public ImmutableArray<SpecialAbility> Abilities => this.m_abilities.ToImmutableArray();

        /// <summary>
        /// Statistics tied to the <see cref="Company"/>.
        /// </summary>
        [JsonBackingField(nameof(m_companyStatistics))]
        public CompanyStatistics Statistics => this.m_companyStatistics;

        /// <summary>
        /// The calculated company checksum.
        /// </summary>
        [JsonBackingField(nameof(m_checksum))]
        public string Checksum => this.m_checksum;


        [JsonBackingField(nameof(m_nextSquadId))]
        protected ushort NextSquadID => this.m_nextSquadId;

        /// <summary>
        /// New empty <see cref="Company"/> instance.
        /// </summary>
        [Obsolete("Please use the CompanyBuilder to create a company")]
        public Company() {
            this.m_squads = new List<Squad>();
            this.m_inventory = new List<Blueprint>();
            this.m_modifiers = new List<Modifier>();
            this.m_upgrades = new List<UpgradeBlueprint>();
            this.m_abilities = new List<SpecialAbility>();
            this.m_companyType = CompanyType.Unspecified;
            this.m_companyStatistics = new CompanyStatistics();
            this.m_lastEditVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }

        /// <summary>
        /// New <see cref="Company"/> instance with a <see cref="SteamUser"/> assigned.
        /// </summary>
        /// <param name="user">The <see cref="SteamUser"/> who can use the <see cref="Company"/>. (Can be null).</param>
        /// <param name="name">The name of the company.</param>
        /// <param name="army">The <see cref="Faction"/> that can use the <see cref="Company"/>.</param>
        /// <param name="tuningGUID">The GUID of the tuning mod the <see cref="Company"/> is using blueprints from.</param>
        /// <exception cref="ArgumentNullException"/>
        [Obsolete("Please use the CompanyBuilder to create a company")]
        public Company(SteamUser user, string name, Faction army, string tuningGUID) {

            // Make sure it's a valid army
            if (army is null) {
                throw new ArgumentNullException(nameof(army), "Army cannot be null");
            }

            // Assign base values
            this.Name = name;
            this.Owner = user?.Name ?? "Unknown Player";
            this.m_companyArmy = army;
            this.TuningGUID = tuningGUID.Replace("-", "");
            this.m_companyType = CompanyType.Unspecified;
            this.m_companyStatistics = new CompanyStatistics();

            // Prepare squad list
            this.m_squads = new List<Squad>();
            this.m_nextSquadId = 0;

            // Misc stuff
            this.m_inventory = new List<Blueprint>();

            // Set last edit version
            this.m_lastEditVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public ushort AddSquad(UnitBuilder builder) {
            ushort id = this.m_nextSquadId++;
            Squad squad = builder.Build(id);
            if (squad.Crew != null) {
                this.m_nextSquadId++;
            }
            this.m_squads.Add(squad);
            return id;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="squadID"></param>
        public void RemoveSquad(uint squadID) => this.m_squads.RemoveAll(x => x.SquadID == squadID);

        /// <summary>
        /// Get the company <see cref="Squad"/> by its company index.
        /// </summary>
        /// <param name="squadID">The index of the squad to get</param>
        /// <returns>The squad with squad id matching requested squad ID or null.</returns>
        public Squad GetSquadByIndex(ushort squadID) {
            Squad s = this.m_squads.FirstOrDefault(x => x.SquadID == squadID);
            if (s == null) { s = this.m_squads.FirstOrDefault(x => x.Crew.SquadID == squadID)?.Crew; }
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
            this.m_checksum = string.Empty;
        }

        /// <summary>
        /// Get the complete checksum in string format.
        /// </summary>
        /// <returns>The string representation of the checksum.</returns>
        public string GetChecksum() {
            long aggr = Encoding.UTF8.GetBytes(this.SerializeAsJson()).Aggregate<byte, long>(0, (a, b) => a + b + (a % b));
            aggr &= 0xffff;
            return aggr.ToString("X8");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool VerifyAppVersion() => this.m_lastEditVersion.CompareTo(this.m_lastEditVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString()) == 0;

        /// <summary>
        /// Verify the checksum of the object.
        /// </summary>
        /// <returns>true if the checksum is valid. False if the checksum does not match.</returns>
        public bool VerifyChecksum() {
           
            // Backup and reset checksum
            string checksum = this.m_checksum;
            this.m_checksum = string.Empty;

            // Calculate checksum
            string newChecksum = this.GetChecksum();
            bool result = newChecksum.CompareTo(checksum) == 0;

            // Restore checksum
            this.m_checksum = checksum;

            // Return result
            return result;

        }

        /// <summary>
        /// Save all <see cref="Company"/> data to a Json file.
        /// </summary>
        /// <param name="jsonfile">The path of the file to save <see cref="Company"/> data into.</param>
        public void SaveToFile(string jsonfile) {
            this.m_checksum = string.Empty;
            this.m_checksum = this.GetChecksum();
            File.WriteAllText(jsonfile, this.SerializeAsJson());
        }

        public void SetType(CompanyType type) => this.m_companyType = type;

        public void SetArmy(Faction faction) => this.m_companyArmy = faction;

        public void SetAvailability(CompanyAvailabilityType companyAvailability) => this.m_availabilityType = companyAvailability;

        /// <summary>
        /// Calculate the strength of the company
        /// </summary>
        /// <returns></returns>
        public double GetStrength() {
            double total = 1;
            // TODO: Add other factors here
            total *= 1.0 + this.m_companyStatistics.WinRate;
            return total;
        }

        public string ToJsonReference() => this.Name;

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString() => this.Name;

        /// <summary>
        /// Convert the <see cref="Company"/> into a <see cref="byte"/> array.
        /// </summary>
        /// <returns>The binary representation of the <see cref="Company"/>.</returns>
        public byte[] ToBytes() {
            this.m_checksum = string.Empty;
            this.m_checksum = this.GetChecksum();
            return Encoding.UTF8.GetBytes(this.SerializeAsJson());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="updateFunction"></param>
        public void UpdateStatistics(Func<CompanyStatistics, CompanyStatistics> updateFunction) => this.m_companyStatistics = updateFunction(this.m_companyStatistics);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="jsonfilepath"></param>
        /// <returns></returns>
        /// <exception cref="JsonTypeException"/>
        public static Company ReadCompanyFromFile(string jsonfilepath) {
            try {
                // Load from json
                Company c = JsonParser.ParseFile<Company>(jsonfilepath);
                if (c.VerifyAppVersion()) {
                    if (c.VerifyChecksum()) {
                        return c;
                    } else {
#if RELEASE
                        throw new ChecksumViolationException();
#else
                        return null;
#endif
                    }
                } else {
#if RELEASE
                        throw new Exception("Version stuff");
#else
                    return null;
#endif
                }
            } catch (JsonTypeException) {
                throw;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="jsonbytes"></param>
        /// <returns></returns>
        public static Company ReadCompanyFromBytes(byte[] jsonbytes) {
            Company company = JsonParser.ParseString<Company>(Encoding.UTF8.GetString(jsonbytes));
            if (company.VerifyChecksum()) {
                return company;
            } else {
#if RELEASE
                throw new ChecksumViolationException();
#else
                File.WriteAllBytes("errCompanyData.json", jsonbytes);
                return null;
#endif
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="jsonString"></param>
        /// <returns></returns>
        public static Company ReadCompanyFromString(string jsonString) {
            Company company = JsonParser.ParseString<Company>(jsonString);
            if (company.VerifyChecksum()) {
                return company;
            } else {
#if RELEASE
                throw new ChecksumViolationException();
#else
                File.WriteAllText("errCompanyData.json", jsonString);
                return null;
#endif
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="jsonfilepath"></param>
        /// <returns></returns>
        /// <exception cref="JsonTypeException"/>
        public static Company UpgradeCompanyToCurrentAppVersion(string jsonfilepath) {
            try {
                if (JsonParser.ParseFile<Company>(jsonfilepath) is Company c) {
                    c.m_lastEditVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();
                    return c;
                } else {
                    return null;
                }
            } catch (JsonTypeException) {
                throw;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="squadIndex"></param>
        /// <param name="squad"></param>
        public void ReplaceSquad(ushort squadIndex, Squad squad) {
            int arrIndex = this.m_squads.FindIndex(x => x.SquadID == squadIndex);
            if (arrIndex != -1) {
                this.m_squads[arrIndex] = squad;
            } else {
                throw new InvalidOperationException();
            }
        }

    }

}
