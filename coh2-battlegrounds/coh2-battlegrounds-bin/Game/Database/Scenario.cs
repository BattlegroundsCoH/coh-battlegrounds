using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using System.IO;

using Battlegrounds.Game.Gameplay;
using Battlegrounds.Json;
using System;

namespace Battlegrounds.Game.Database {

    /// <summary>
    /// The theatre of war a scenario is taking place in.
    /// </summary>
    public enum ScenarioTheatre {
        
        /// <summary>
        /// Axis vs Soviets
        /// </summary>
        EasternFront,
        
        /// <summary>
        /// Axis vs UKF & USF
        /// </summary>
        WesternFront,
        
        /// <summary>
        /// Axis vs Allies (Germany)
        /// </summary>
        SharedFront,

    }

    /// <summary>
    /// Represents a scenario. Implements <see cref="IJsonObject"/>. This class cannot be inherited.
    /// </summary>
    public sealed class Scenario : IJsonObject {

        public const string INVALID_SGA = "INVALID SGA";

        /// <summary>
        /// The text-name for the <see cref="Scenario"/>.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The description text for the <see cref="Scenario"/>.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// The relative filename to the "mp" scenarios folder.
        /// </summary>
        public string RelativeFilename { get; set; }

        /// <summary>
        /// Name of the sga file containing this <see cref="Scenario"/>.
        /// </summary>
        public string SgaName { get; set; }

        /// <summary>
        /// The max amount of players who can play on this map.
        /// </summary>
        public byte MaxPlayers { get; set; }

        /// <summary>
        /// The <see cref="ScenarioTheatre"/> this map takes place in.
        /// </summary>
        [JsonEnum(typeof(ScenarioTheatre))]
        public ScenarioTheatre Theatre { get; set; }

        /// <summary>
        /// Can this <see cref="Scenario"/> be considered a winter map.
        /// </summary>
        public bool IsWintermap { get; set; }

        /// <summary>
        /// The <see cref="Wincondition"/> instances designed for this <see cref="Scenario"/>. Empty list means all <see cref="Wincondition"/> instances can be used.
        /// </summary>
        [JsonReference] 
        public List<Wincondition> Gamemodes { get; set; }

        public string ToJsonReference() => this.RelativeFilename;

        public Scenario() {
            this.SgaName = INVALID_SGA;
            this.Gamemodes = new List<Wincondition>();
        }

        /// <summary>
        /// New <see cref="Scenario"/> instance with data from either an infor or options file.
        /// </summary>
        /// <param name="infofile">The path to the info file.</param>
        /// <param name="optionsfile">The path to the options file</param>
        /// <exception cref="ArgumentNullException"/>
        public Scenario(string infofile, string optionsfile) {

            // Make sure infofile is not null
            if (infofile is null) {
                throw new ArgumentNullException(nameof(infofile), "Info filepath cannot be null");
            }

            // Make sure optionsfile is not null
            if (optionsfile is null) {
                throw new ArgumentNullException(nameof(optionsfile), "Options filepath cannot be null");
            }

            this.RelativeFilename = Path.GetFileNameWithoutExtension(infofile);
            this.Gamemodes = new List<Wincondition>();
            this.SgaName = string.Empty;

            string infolua = File.ReadAllText(infofile);
            string optionslua = File.ReadAllText(optionsfile);

            string matchpattern = @"(?<key>\w+)\s+=\s+(?<value>(\w+|\d+|(\"".*\"")))";
            var infomatches = Regex.Matches(infolua, matchpattern);
            var optionsmatches = Regex.Matches(infolua, matchpattern);

            this.Name = infomatches.FirstOrDefault(x => x.Groups["key"].Value.CompareTo("scenarioname") == 0)?.Groups["value"].Value.Trim(' ', '\"') ?? "???";
            this.Description = infomatches.FirstOrDefault(x => x.Groups["key"].Value.CompareTo("scenariodescription") == 0)?.Groups["value"].Value.Trim(' ', '\"') ?? "???";
            this.Description = this.Description.Replace(@"\""", "'").Replace('[', '(').Replace(']', ')');

            if (byte.TryParse(infomatches.FirstOrDefault(x => x.Groups["key"].Value.CompareTo("maxplayers") == 0)?.Groups["value"].Value ?? "2", out byte maxplayers)) {
                this.MaxPlayers = maxplayers;
            }

            if (int.TryParse(infomatches.FirstOrDefault(x => x.Groups["key"].Value.CompareTo("scenario_battlefront") == 0)?.Groups["value"].Value ?? "2", out int battlefront)) {
                this.Theatre = battlefront == 2 ? ScenarioTheatre.EasternFront : battlefront == 5 ? ScenarioTheatre.WesternFront : ScenarioTheatre.SharedFront;
            }

            this.IsWintermap = infolua.Contains("\"winter\"");

        }

        public override string ToString() => this.Name;

    }

}
