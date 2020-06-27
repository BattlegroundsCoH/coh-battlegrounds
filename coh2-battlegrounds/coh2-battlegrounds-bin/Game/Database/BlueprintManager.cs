using System;
using System.Collections.Generic;
using System.Linq;
using Battlegrounds.Game.Database.json;

namespace Battlegrounds.Game.Database {

    /// <summary>
    /// The type a <see cref="Blueprint"/> may represent in the <see cref="BlueprintManager"/>.
    /// </summary>
    public enum BlueprintType {
        
        /// <summary>
        /// Ability Blueprint
        /// </summary>
        ABP,

        /// <summary>
        /// Upgrade Blueprint
        /// </summary>
        UBP,
        
        /// <summary>
        /// Commander Blueprint
        /// </summary>
        CBP,

        /// <summary>
        /// Slot Item Blueprint
        /// </summary>
        IBP,

        /// <summary>
        /// Entity Blueprint
        /// </summary>
        EBP = 16,

        /// <summary>
        /// Squad Blueprint
        /// </summary>
        SBP = 32,

    }

    /// <summary>
    /// Database over all relevant blueprints in use
    /// </summary>
    public static class BlueprintManager {

        private static Dictionary<ulong, Blueprint> __entities;
        private static Dictionary<ulong, Blueprint> __squads;
        private static Dictionary<ulong, Blueprint> __abilities;
        private static Dictionary<ulong, Blueprint> __upgrades;
        private static Dictionary<ulong, Blueprint> __commanders;
        private static Dictionary<ulong, Blueprint> __slotitems;

        /// <summary>
        /// 
        /// </summary>
        public static void CreateDatabase() {

            // Create Dictionaries
            __entities = new Dictionary<ulong, Blueprint>();
            __squads = new Dictionary<ulong, Blueprint>();
            __abilities = new Dictionary<ulong, Blueprint>();
            __upgrades = new Dictionary<ulong, Blueprint>();
            __commanders = new Dictionary<ulong, Blueprint>();
            __slotitems = new Dictionary<ulong, Blueprint>();

            // Load data here
            bool sbpdb = LoadJsonDatabase("data\\json-db\\vcoh-sbp-db.json", BlueprintType.SBP); // load the squad blueprint database
            bool ubpdb = LoadJsonDatabase("data\\json-db\\vcoh-ubp-db.json", BlueprintType.UBP); // load the upgrade blueprint database

            // TODO: Load more here...

            if (!sbpdb || !ubpdb) {
                throw new Exception();
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="jsonfile"></param>
        /// <param name="bType"></param>
        /// <returns></returns>
        public static bool LoadJsonDatabase(string jsonfile, BlueprintType bType) {

            // Parse the file
            var ls = JsonParser.Parse(jsonfile);

            // If Empty
            if (ls.Count == 0) {
                return false;
            }

            // Make sure we got a json array
            if (ls.First() is JsonArray content) {

                // Get the target dictionary
                var targetDictionary = bType switch
                {
                    BlueprintType.ABP => __abilities,
                    BlueprintType.CBP => __commanders,
                    BlueprintType.EBP => __entities,
                    BlueprintType.SBP => __squads,
                    BlueprintType.UBP => __upgrades,
                    BlueprintType.IBP => __slotitems,
                    _ => throw new ArgumentException(),
                };

                // Add all the found elements
                foreach (IJsonElement element in content) {
                    if (element is Blueprint bp) {
                        targetDictionary.Add(bp.PBGID, bp);
                    } else {
                        return false;
                    }
                }

            } else {
                return false;
            }

            return true;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="bType"></param>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="KeyNotFoundException"/>
        /// <returns></returns>
        public static Blueprint FromPbgId(ushort id, BlueprintType bType) {
            return bType switch
            {
                BlueprintType.ABP => __abilities[id],
                BlueprintType.CBP => __commanders[id],
                BlueprintType.EBP => __entities[id],
                BlueprintType.SBP => __squads[id],
                BlueprintType.UBP => __upgrades[id],
                BlueprintType.IBP => __slotitems[id],
                _ => throw new ArgumentException(),
            };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="bType"></param>
        /// <returns></returns>
        public static Blueprint FromBlueprintName(string id, BlueprintType bType) {
            return bType switch
            {
                BlueprintType.ABP => __abilities.FirstOrDefault(x => (x.Value.Name ?? "") == id).Value,
                BlueprintType.CBP => __commanders.FirstOrDefault(x => (x.Value.Name ?? "") == id).Value,
                BlueprintType.EBP => __entities.FirstOrDefault(x => (x.Value.Name ?? "") == id).Value,
                BlueprintType.SBP => __squads.FirstOrDefault(x => (x.Value.Name ?? "") == id).Value,
                BlueprintType.UBP => __upgrades.FirstOrDefault(x => (x.Value.Name ?? "") == id).Value,
                BlueprintType.IBP => __slotitems.FirstOrDefault(x => (x.Value.Name ?? "") == id).Value,
                _ => throw new ArgumentException(),
            };
        }

    }

}
