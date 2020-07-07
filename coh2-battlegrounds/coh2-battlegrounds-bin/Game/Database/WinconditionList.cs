using System;
using System.Collections.Generic;
using System.Linq;
using Battlegrounds.Game.Gameplay;
using Battlegrounds.Modding;

namespace Battlegrounds.Game.Database {
    
    /// <summary>
    /// A small static class-database over all relevant <see cref="Wincondition"/> instances.
    /// </summary>
    public static class WinconditionList {

        static Dictionary<string, Wincondition> __winconditions;

        /// <summary>
        /// Create and load the <see cref="WinconditionList"/>.
        /// </summary>
        public static void CreateAndLoadDatabase() {

            Guid vp_guid = Guid.NewGuid();

            // Create the wincondition dictionary
            __winconditions = new Dictionary<string, Wincondition> {
                [vp_guid.ToString()] = new Wincondition("Victory Points", vp_guid) {
                    Options = new WinconditionOption[] {
                        new WinconditionOption("250 Victory Points", 250),
                        new WinconditionOption("500 Victory Points", 500),
                        new WinconditionOption("1000 Victory Points", 1000)
                    },
                    DefaultOptionIndex = 1
                },
            };

        }

        /// <summary>
        /// Register a new <see cref="Wincondition"/> in the database.
        /// </summary>
        /// <param name="wincondition"></param>
        public static void AddWincondition(Wincondition wincondition) => __winconditions.Add(wincondition.Guid.ToString(), wincondition);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static Wincondition GetWinconditionByName(string name) => __winconditions.FirstOrDefault(x => x.Value.Name.CompareTo(name) == 0).Value;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        public static Wincondition GetWinconditionBuGUID(string guid) => __winconditions[guid];

        /// <summary>
        /// 
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        public static Wincondition GetWinconditionBuGUID(Guid guid) => __winconditions[guid.ToString()];

    }

}
