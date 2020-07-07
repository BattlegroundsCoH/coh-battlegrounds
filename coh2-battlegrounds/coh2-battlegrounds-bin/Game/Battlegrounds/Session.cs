using System;
using System.Collections.Generic;

using Battlegrounds.Json;
using Battlegrounds.Game.Gameplay;
using Battlegrounds.Modding;
using Battlegrounds.Game.Database;

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
        public Scenario Scenario { get; }

        /// <summary>
        /// The <see cref="Wincondition"/> to use when playing.
        /// </summary>
        public IWinconditionMod Gamemode { get; }

        /// <summary>
        /// A list of all settings to apply for the <see cref="Session"/>.
        /// </summary>
        public Dictionary<string, object> Settings { get; }

        /// <summary>
        /// The unique GUID to use to verify match data.
        /// </summary>
        public Guid SessionID { get; }

        /// <summary>
        /// 
        /// </summary>
        [JsonIgnore] public ITuningMod TuningMod { get; private set; }

        private Session(Scenario scenario, Company[] companies, IWinconditionMod gamemode) {
            this.Settings = new Dictionary<string, object>();
            this.SessionID = Guid.NewGuid();
            this.Scenario = scenario;
            this.Companies = companies;
            this.Gamemode = gamemode;
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
        /// <param name="sessionInfo">The map name of the scenario to play on.</param>
        /// <param name="companies">All the companies who will take part in the <see cref="Session"/>.</param>
        /// <returns>New <see cref="Session"/> with the given data.</returns>
        public static Session CreateSession(SessionInfo sessionInfo, Company[] companies) {

            // Create the session
            Session session = new Session(sessionInfo.SelectedScenario, companies, sessionInfo.SelectedGamemode) {
                TuningMod = sessionInfo.SelectedTuningMod
            };

            // Set the game mode
            session.AddSetting("gamemode_setting", sessionInfo.SelectedGamemode.Options[sessionInfo.SelectedGamemodeOption].Value);

            // Fill AI (if enabled)
            if (sessionInfo.FillAI) {
                if (sessionInfo.Allies.Length < sessionInfo.Axis.Length) {



                } else if (sessionInfo.Axis.Length < sessionInfo.Allies.Length) {



                }
            }

            // Return the new session
            return session;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public string ToJsonReference() => this.SessionID.ToString();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="jsonReference"></param>
        public void FromJsonReference(string jsonReference) => throw new NotSupportedException();

    }

}
