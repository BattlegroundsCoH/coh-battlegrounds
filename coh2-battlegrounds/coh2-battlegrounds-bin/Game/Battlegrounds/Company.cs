using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;

using Battlegrounds.Functional;
using Battlegrounds.Game.Database;
using Battlegrounds.Game.Gameplay;
using Battlegrounds.Json;
using Battlegrounds.Steam;
using Battlegrounds.Verification;

namespace Battlegrounds.Game.Battlegrounds {

    /// <summary>
    /// Represents a Company. Implements <see cref="IJsonObject"/> and <see cref="IChecksumItem"/>.
    /// </summary>
    public class Company : IJsonObject, IChecksumItem {

        /// <summary>
        /// The maximum size of a company.
        /// </summary>
        public const int MAX_SIZE = 40;

        private string m_checksum;
        private ushort m_nextSquadId;
        [JsonIgnoreIfEmpty] private List<Squad> m_squads;
        [JsonIgnoreIfEmpty] private List<Blueprint> m_inventory;

        [JsonEnum(typeof(CompanyType))] private CompanyType m_companyType;
        [JsonReference(typeof(Faction))] private Faction m_companyArmy;

        /// <summary>
        /// The name of the company.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The <see cref="CompanyType"/> that can be used to describe the <see cref="Company"/> characteristics.
        /// </summary>
        [JsonIgnore]
        public CompanyType Type => m_companyType;

        /// <summary>
        /// The GUID of the tuning mod used to create this company.
        /// </summary>
        public string TuningGUID { get; set; }

        /// <summary>
        /// The <see cref="Faction"/> this company is associated with.
        /// </summary>
        [JsonIgnore]
        public Faction Army => m_companyArmy;

        /// <summary>
        /// The display name of who owns the <see cref="Company"/>. This property is used by the compilers. This will be overriden.
        /// </summary>
        [JsonIgnore]
        public string Owner { get; set; }

        /// <summary>
        /// <see cref="ImmutableArray{T}"/> representation of the units in the <see cref="Company"/>.
        /// </summary>
        [JsonIgnore]
        public ImmutableArray<Squad> Units => m_squads.ToImmutableArray();

        /// <summary>
        /// <see cref="ImmutableArray{T}"/> representation of a <see cref="Company"/> inventory of stored <see cref="Blueprint"/> objects.
        /// </summary>
        [JsonIgnore]
        public ImmutableArray<Blueprint> Inventory => m_inventory.ToImmutableArray();

        /// <summary>
        /// 
        /// </summary>
        [JsonIgnore]
        public string Checksum => m_checksum;

        /// <summary>
        /// New empty <see cref="Company"/> instance.
        /// </summary>
        [Obsolete("Please use the CompanyBuilder to create a company")]
        public Company() {
            this.m_squads = new List<Squad>();
            this.m_inventory = new List<Blueprint>();
            this.m_companyType = CompanyType.Unspecified;
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
            if (army == null) {
                throw new ArgumentNullException("Army was null");
            }

            // Assign base values
            this.Name = name;
            this.Owner = user?.Name ?? "Unknown Player";
            this.m_companyArmy = army;
            this.TuningGUID = tuningGUID.Replace("-", "");
            this.m_companyType = CompanyType.Unspecified;

            // Prepare squad list
            this.m_squads = new List<Squad>();
            this.m_nextSquadId = 0;

            // Misc stuff
            this.m_inventory = new List<Blueprint>();

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
        /// Get the company <see cref="Squad"/> by its company index.
        /// </summary>
        /// <param name="squadID">The index of the squad to get</param>
        /// <returns>The squad with squad id matching requested squad ID or null.</returns>
        public Squad GetSquadByIndex(ushort squadID)
            => m_squads.FirstOrDefault(x => x.SquadID == squadID);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="squadId"></param>
        public bool RemoveSquad(ushort squadId)
            => m_squads.RemoveAll(x => x.SquadID == squadId) == 1;

        /// <summary>
        /// Reset the squads of the company.
        /// </summary>
        public void ResetSquads() {
            this.m_squads.Clear();
            this.m_nextSquadId = 0;
        }

        /// <summary>
        /// Reset most of the company data.
        /// </summary>
        public void ResetCompany() {
            this.ResetSquads();
            this.Name = string.Empty;
        }

        /// <summary>
        /// Get the complete checksum in string format.
        /// </summary>
        /// <returns>The string representation of the checksum.</returns>
        public string GetChecksum() {
            long aggr = Encoding.UTF8.GetBytes((this as IJsonObject).Serialize()).Aggregate<byte, long>(0, (a, b) => a += b + (a % b));
            aggr &= 0xffff;
            return aggr.ToString("X8");
        }

        /// <summary>
        /// Verify the checksum of the object.
        /// </summary>
        /// <returns>true if the checksum is valid. False if the checksum does not match.</returns>
        public bool VerifyChecksum() {
            
            // Backup and reset checksum
            string checksum = this.m_checksum;
            this.m_checksum = string.Empty;

            // Calculate checksum
            bool result = (this.GetChecksum().CompareTo(checksum) == 0);

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
            File.WriteAllText(jsonfile, (this as IJsonObject).Serialize());
        }

        internal void SetType(CompanyType type) => this.m_companyType = type;

        internal void SetArmy(Faction faction) => this.m_companyArmy = faction;

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public string ToJsonReference() => this.Name;

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString() => this.Name;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="jsonfilepath"></param>
        /// <returns></returns>
        public static Company ReadCompanyFromFile(string jsonfilepath) {

            List<IJsonElement> elements = JsonParser.Parse(jsonfilepath);
            if (elements.FirstOrDefault() is Company c) {
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
                return null;
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

    }

}
