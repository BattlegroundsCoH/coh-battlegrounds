using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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

        private static Dictionary<ushort, Blueprint> __entities;
        private static Dictionary<ushort, Blueprint> __squads;
        private static Dictionary<ushort, Blueprint> __abilities;
        private static Dictionary<ushort, Blueprint> __upgrades;
        private static Dictionary<ushort, Blueprint> __commanders;

        /// <summary>
        /// 
        /// </summary>
        public static void CreateDatabase() {

            // Create Dictionaries
            __entities = new Dictionary<ushort, Blueprint>();
            __squads = new Dictionary<ushort, Blueprint>();
            __abilities = new Dictionary<ushort, Blueprint>();
            __upgrades = new Dictionary<ushort, Blueprint>();
            __commanders = new Dictionary<ushort, Blueprint>();

            // Load data here
                // TODO: Load it...

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="jsonfile"></param>
        /// <param name="bType"></param>
        /// <returns></returns>
        public static bool LoadJsonDatabase(string jsonfile, BlueprintType bType) {

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
                _ => throw new ArgumentException(),
            };
        }

    }

}
