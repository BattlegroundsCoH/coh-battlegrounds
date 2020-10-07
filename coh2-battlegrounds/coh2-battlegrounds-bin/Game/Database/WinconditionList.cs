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

        /// <summary>
        /// Const string identifier for the Victory Points gamemode.
        /// </summary>
        public const string VictoryPoints = "Victory Points";

        /// <summary>
        /// Const string identifier for the Breakthrough gamemode.
        /// </summary>
        public const string Breakthrough = "Breakthrough";

        /// <summary>
        /// Const string identifier for the Breakout gamemode.
        /// </summary>
        public const string Breakout = "Breakout";

        /// <summary>
        /// Const string identifier for the AirAssault gamemode.
        /// </summary>
        public const string AirAssault = "Air Assault";

        /// <summary>
        /// Const string identifier for the Attrition gamemode.
        /// </summary>
        public const string Attrition = "Attrition";

        static Dictionary<string, Wincondition> __winconditions;

        /// <summary>
        /// Create and load the <see cref="WinconditionList"/>.
        /// </summary>
        public static void CreateAndLoadDatabase() {

            Guid bg_wincondition = Guid.Parse("6a0a13b89555402ca75b85dc30f5cb04");

            // Create the wincondition dictionary
            __winconditions = new Dictionary<string, Wincondition> {
                [VictoryPoints] = new Wincondition(VictoryPoints, bg_wincondition) {
                    Options = new WinconditionOption[] {
                        new WinconditionOption("250 Victory Points", 250),
                        new WinconditionOption("500 Victory Points", 500),
                        new WinconditionOption("1000 Victory Points", 1000)
                    },
                    DefaultOptionIndex = 1
                },
                [Breakthrough] = new Wincondition(Breakthrough, bg_wincondition) {
                    Options = new WinconditionOption[] {
                        new WinconditionOption("250 Victory Points", 250),
                        new WinconditionOption("500 Victory Points", 500),
                        new WinconditionOption("1000 Victory Points", 1000)
                    },
                },
                [Breakout] = new Wincondition(Breakout, bg_wincondition) {
                    Options = new WinconditionOption[] {
                        new WinconditionOption("250 Victory Points", 250),
                        new WinconditionOption("500 Victory Points", 500),
                        new WinconditionOption("1000 Victory Points", 1000)
                    },
                },
                [AirAssault] = new Wincondition(AirAssault, bg_wincondition) {
                    Options = new WinconditionOption[] {
                        new WinconditionOption("75 Victory Points", 75),
                        new WinconditionOption("250 Victory Points", 250),
                        new WinconditionOption("500 Victory Points", 500)
                    },
                },
                [Attrition] = new Wincondition(Attrition, bg_wincondition) {
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
        /// <param name="guid"></param>
        /// <returns></returns>
        public static Wincondition GetWinconditionByName(string name) => __winconditions[name];

        public static List<Wincondition> GetDefaultList() {
            return new List<Wincondition> {
                GetWinconditionByName(Attrition)
            };
        }

    }

}
