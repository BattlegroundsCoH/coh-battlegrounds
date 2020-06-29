using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using Battlegrounds.Game.Database;
using Battlegrounds.Game.Database.json;
using Battlegrounds.Game.Gameplay;
using Battlegrounds.Steam;
using Battlegrounds.Verification;

namespace Battlegrounds.Game.Battlegrounds {
    
    /// <summary>
    /// Represents a Company. Implements <see cref="IJsonObject"/>.
    /// </summary>
    public class Company : IJsonObject, IChecksumItem {

        private ushort m_nextSquadId;
        private List<Squad> m_squads;

        /// <summary>
        /// The name of the company.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The <see cref="Faction"/> this company is associated with.
        /// </summary>
        public Faction Army { get; }

        /// <summary>
        /// The <see cref="SteamUser"/> who owns the <see cref="Company"/>.
        /// </summary>
        [JsonIgnore]
        public SteamUser Owner { get; }

        /// <summary>
        /// <see cref="ImmutableArray{T}"/> representation of the units in the <see cref="Company"/>.
        /// </summary>
        [JsonIgnore]
        public ImmutableArray<Squad> Units => m_squads.ToImmutableArray();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="user"></param>
        /// <param name="name"></param>
        /// <param name="army"></param>
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
        /// <param name="upgrades"></param>
        /// <param name="slotitems"></param>
        /// <param name="verify"></param>
        /// <returns></returns>
        public bool AddSquad(string bp, byte vet, float vetprog, string[] upgrades = null, string[] slotitems = null, bool verify = true) {

            // If we're to verify, we do it here
            if (verify) {
                // Verify we can do this
            }

            // Create squad
            Squad squad = new Squad(m_nextSquadId++, null, BlueprintManager.FromBlueprintName(bp, BlueprintType.SBP));
            squad.SetVeterancy(vet, vetprog);
            
            // Add upgrades
            if (upgrades != null) {
                foreach(string upg in upgrades) {
                    squad.AddUpgrade(BlueprintManager.FromBlueprintName(upg, BlueprintType.UBP));
                }
            }

            // Add items
            if (slotitems != null) {
                foreach (string item in slotitems) {
                    squad.AddUpgrade(BlueprintManager.FromBlueprintName(item, BlueprintType.IBP));
                }
            }

            // Add to list of available squads
            m_squads.Add(squad);

            // Return true
            return true;

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
        /// Get the complete checksum in string format.
        /// </summary>
        /// <returns>The string representation of the checksum.</returns>
        public string GetChecksum() => throw new NotImplementedException();

        /// <summary>
        /// Save all <see cref="Company"/> data to a Json file.
        /// </summary>
        /// <param name="jsonfile">The path of the file to save <see cref="Company"/> data into.</param>
        public void SaveToFile(string jsonfile) 
            => File.WriteAllText(jsonfile, (this as IJsonObject).Serialize());

    }

}
