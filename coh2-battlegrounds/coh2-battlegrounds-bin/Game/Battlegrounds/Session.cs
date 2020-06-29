using System;
using System.Collections.Generic;
using Battlegrounds.Game.Database.json;
using Battlegrounds.Game.Gameplay;

namespace Battlegrounds.Game.Battlegrounds {
    
    /// <summary>
    /// Represents a game session where a match will take place between players with a pre-selected <see cref="Company"/> and using a set of preset settings. Implements <see cref="IJsonObject"/>.
    /// </summary>
    public class Session : IJsonObject {

        /// <summary>
        /// The <see cref="Company"/> objects representing the companies within this session.
        /// </summary>
        public Company[] Companies { get; }

        /// <summary>
        /// The name of the scenario file to play.
        /// </summary>
        public string Scenario { get; }

        /// <summary>
        /// The <see cref="Wincondition"/> to use when playing.
        /// </summary>
        public Wincondition Gamemode { get; }

        /// <summary>
        /// Allow AI to take part in the <see cref="Session"/>.
        /// </summary>
        public bool AllowAI { get; }

        /// <summary>
        /// A list of all settings to apply for the <see cref="Session"/>.
        /// </summary>
        public Dictionary<string, object> Settings { get; }

        /// <summary>
        /// The unique GUID to use to verify match data.
        /// </summary>
        public Guid SessionID { get; }

        private Session(string scenario, Company[] companies, Wincondition gamemode, bool enableAI) {
            this.Settings = new Dictionary<string, object>();
            this.SessionID = Guid.NewGuid();
            this.Scenario = scenario;
            this.Companies = companies;
            this.Gamemode = gamemode;
            this.AllowAI = enableAI;
        }

        /// <summary>
        /// Add a custom setting to the <see cref="Session"/>.
        /// </summary>
        /// <param name="setting">The name of the setting to add.</param>
        /// <param name="value">The value of the setting (Automatically converted to a Lua code equivalent).</param>
        public void AddSetting(string setting, object value) {
            if (this.Settings.ContainsKey(setting)) {
                this.Settings[setting] = value;
            } else {
                this.Settings.Add(setting, value);
            }
        }

        /// <summary>
        /// Create a new <see cref="Session"/> instance with a unique <see cref="Guid"/>.
        /// </summary>
        /// <param name="scenario">The map name of the scenario to play on.</param>
        /// <param name="companies">All the companies who will take part in the <see cref="Session"/>.</param>
        /// <param name="gamemode">The gamemode to play with.</param>
        /// <param name="enableAI">Allow AI players in this <see cref="Session"/>.</param>
        /// <returns>New <see cref="Session"/> with the given data.</returns>
        public static Session CreateSession(string scenario, Company[] companies, Wincondition gamemode, bool enableAI) {

            // Create the session
            Session session = new Session(scenario, companies, gamemode, enableAI);

            // Return the new session
            return session;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public string ToJsonReference() => this.SessionID.ToString();

    }

}
