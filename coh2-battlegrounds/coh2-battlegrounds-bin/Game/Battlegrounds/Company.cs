using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;

using Battlegrounds.Game.Database;
using Battlegrounds.Json;
using Battlegrounds.Game.Gameplay;
using Battlegrounds.Steam;
using Battlegrounds.Verification;
using Battlegrounds.Functional;

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
        private List<Squad> m_squads;

        /// <summary>
        /// The name of the company.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The GUID of the tuning mod used to create this company.
        /// </summary>
        public string TuningGUID { get; set; }

        /// <summary>
        /// The <see cref="Faction"/> this company is associated with.
        /// </summary>
        [JsonReference(typeof(Faction))]
        public Faction Army { get; }

        /// <summary>
        /// The <see cref="SteamUser"/> who owns the <see cref="Company"/>.
        /// </summary>
        [JsonReference(typeof(SteamUser))]
        public SteamUser Owner { get; }

        /// <summary>
        /// <see cref="ImmutableArray{T}"/> representation of the units in the <see cref="Company"/>.
        /// </summary>
        [JsonIgnore]
        public ImmutableArray<Squad> Units => m_squads.ToImmutableArray();

        /// <summary>
        /// 
        /// </summary>
        [JsonIgnore]
        public string Checksum => m_checksum;

        /// <summary>
        /// New empty <see cref="Company"/> instance. (Do not use).
        /// </summary>
        [Obsolete("Please use the constructor requiring a SteamUser, name, and faction.")]
        public Company() {
            this.m_squads = new List<Squad>();
        }

        /// <summary>
        /// New <see cref="Company"/> instance with a <see cref="SteamUser"/> assigned.
        /// </summary>
        /// <param name="user">The <see cref="SteamUser"/> who can use the <see cref="Company"/>.</param>
        /// <param name="name">The name of the company.</param>
        /// <param name="army">The <see cref="Faction"/> that can use the <see cref="Company"/>.</param>
        /// <exception cref="ArgumentNullException"/>
        public Company(SteamUser user, string name, Faction army) {

            // Make sure the user is 'valid'
            if (user == null) {
                throw new ArgumentNullException();
            }

            // Make sure it's a valid army
            if (army == null) {
                throw new ArgumentNullException();
            }

            // Assign
            this.Name = name;
            this.Owner = user;
            this.Army = army;
            this.TuningGUID = string.Empty; // TODO: Take this as a parameter as well.

            // Prepare squad list
            this.m_squads = new List<Squad>();
            this.m_nextSquadId = 0;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bp"></param>
        /// <param name="vet"></param>
        /// <param name="vetprog"></param>
        /// <returns></returns>
        public int AddSquad(string bp, byte vet, float vetprog)
            => this.AddSquad(bp, string.Empty, false, vet, vetprog, null, null, null);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bp"></param>
        /// <param name="vet"></param>
        /// <param name="vetprog"></param>
        /// <param name="upgrades"></param>
        /// <returns></returns>
        public int AddSquad(string bp, byte vet, float vetprog, string[] upgrades)
            => this.AddSquad(bp, string.Empty, false, vet, vetprog, upgrades, null, null);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bp"></param>
        /// <param name="vet"></param>
        /// <param name="vetprog"></param>
        /// <param name="upgrades"></param>
        /// <param name="slotiems"></param>
        /// <returns></returns>
        public int AddSquad(string bp, byte vet, float vetprog, string[] upgrades, string[] slotiems)
            => this.AddSquad(bp, string.Empty, false, vet, vetprog, upgrades, slotiems, null);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mainBP"></param>
        /// <param name="supportBP"></param>
        /// <param name="vet"></param>
        /// <param name="vetprog"></param>
        /// <returns></returns>
        public int AddSquad(string mainBP, string supportBP, byte vet, float vetprog)
            => this.AddSquad(mainBP, supportBP, false, vet, vetprog, null, null, null);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mainBP"></param>
        /// <param name="supportBP"></param>
        /// <param name="vet"></param>
        /// <param name="vetprog"></param>
        /// <param name="upgrades"></param>
        /// <returns></returns>
        public int AddSquad(string mainBP, string supportBP, byte vet, float vetprog, string[] upgrades)
            => this.AddSquad(mainBP, supportBP, false, vet, vetprog, upgrades, null, null);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mainBP"></param>
        /// <param name="supportBP"></param>
        /// <param name="vet"></param>
        /// <param name="vetprog"></param>
        /// <param name="upgrades"></param>
        /// <param name="slotiems"></param>
        /// <returns></returns>
        public int AddSquad(string mainBP, string supportBP, byte vet, float vetprog, string[] upgrades, string[] slotiems)
            => this.AddSquad(mainBP, supportBP, false, vet, vetprog, upgrades, slotiems, null);

        /// <summary>
        /// Add a new <see cref="Squad"/> instance to the company with veterancy, upgrades, and slot items pre-applied.
        /// </summary>
        /// <param name="bp">The name of the <see cref="SquadBlueprint"/> to give the squad.</param>
        /// <param name="vet">The amount of veterancy the squad will have.</param>
        /// <param name="vetprog">The progress towards the next veterancy level.</param>
        /// <param name="upgrades">The pre-set upgrades.</param>
        /// <param name="slotitems">The pre-set slot items.</param>
        /// <returns>The squad index given to the squad.</returns>
        public int AddSquad(string bp, string supportBP, bool deployExit, byte vet, float vetprog, string[] upgrades, string[] slotitems, Modifier[] modifiers) {

            // Make sure there's something to work with
            if (upgrades == null) {
                upgrades = new string[0];
            }

            // Make sure there is a workable slotitem array
            if (slotitems == null) {
                slotitems = new string[0];
            }

            // Make sure there's a workable modifier array
            if (modifiers == null) {
                modifiers = new Modifier[0];
            }

            // Cast arrays
            UpgradeBlueprint[] ubps = upgrades.Convert(x => BlueprintManager.FromBlueprintName(x, BlueprintType.UBP) as UpgradeBlueprint);
            SlotItemBlueprint[] ibps = slotitems.Convert(x => BlueprintManager.FromBlueprintName(x, BlueprintType.IBP) as SlotItemBlueprint);

            // Get the 
            SquadBlueprint sbp = BlueprintManager.FromBlueprintName(bp, BlueprintType.SBP) as SquadBlueprint;
            Blueprint supportBlueprint = null;

            // Get support blueprint if any
            if (supportBP.CompareTo(string.Empty) != 0) {
                if (BlueprintManager.FromBlueprintName(supportBP, BlueprintType.EBP) is Blueprint b1) { // always try with ebp first
                    supportBlueprint = b1;
                } else if (BlueprintManager.FromBlueprintName(supportBP, BlueprintType.SBP) is Blueprint b2) {
                    supportBlueprint = b2;
                }
            }

            // Return squad index
            return this.AddSquad(sbp, supportBlueprint, deployExit, vet, vetprog, ubps, ibps, modifiers);

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="main"></param>
        /// <param name="supprt"></param>
        /// <param name="deployAndExit"></param>
        /// <param name="vet"></param>
        /// <param name="vetprog"></param>
        /// <param name="upgrades"></param>
        /// <param name="slotitems"></param>
        /// <param name="modifiers"></param>
        /// <returns></returns>
        public int AddSquad(
            SquadBlueprint main, Blueprint supprt, bool deployAndExit, byte vet, float vetprog, 
            UpgradeBlueprint[] upgrades, SlotItemBlueprint[] slotitems, Modifier[] modifiers) {

            Squad squad = new Squad(m_nextSquadId++, null, main);
            squad.SetVeterancy(vet, vetprog);
            squad.SetDeploymentMethod(supprt, deployAndExit);

            upgrades.ForEach(x => squad.AddUpgrade(x));
            slotitems.ForEach(x => squad.AddSlotItem(x));
            modifiers.ForEach(x => squad.AddModifier(x));

            m_squads.Add(squad);

            return squad.SquadID;

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
            long aggr = System.Text.Encoding.UTF8.GetBytes((this as IJsonObject).Serialize()).Aggregate<byte, long>(0, (a, b) => a += b + (a % b));
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
            Company company = JsonParser.ParseString<Company>(Encoding.ASCII.GetString(jsonbytes));
            if (company.VerifyChecksum()) {
                return company;
            } else {
#if RELEASE
                throw new ChecksumViolationException();
#else
                return null;
#endif
            }
        }

    }

}
