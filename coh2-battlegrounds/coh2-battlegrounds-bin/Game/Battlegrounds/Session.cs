using System;
using System.Collections.Generic;
using Battlegrounds.Game.Database.json;
using Battlegrounds.Game.Gameplay;

namespace Battlegrounds.Game.Battlegrounds {
    
    /// <summary>
    /// 
    /// </summary>
    public class Session : IJsonObject {

        /// <summary>
        /// 
        /// </summary>
        public Company[] Companies { get; }

        /// <summary>
        /// 
        /// </summary>
        public string Scenario { get; }

        /// <summary>
        /// 
        /// </summary>
        public Wincondition Gamemode { get; }

        /// <summary>
        /// 
        /// </summary>
        public bool AllowAI { get; }

        /// <summary>
        /// 
        /// </summary>
        public Dictionary<string, object> Settings { get; }

        /// <summary>
        /// 
        /// </summary>
        public Guid SessionID { get; }

        /// <summary>
        /// 
        /// </summary>
        private Session(string scenario, Company[] companies, Wincondition gamemode, bool enableAI) {
            this.Settings = new Dictionary<string, object>();
            this.SessionID = Guid.NewGuid();
            this.Scenario = scenario;
            this.Companies = companies;
            this.Gamemode = gamemode;
            this.AllowAI = enableAI;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="setting"></param>
        /// <param name="value"></param>
        public void AddSetting(string setting, object value) {
            if (this.Settings.ContainsKey(setting)) {
                this.Settings[setting] = value;
            } else {
                this.Settings.Add(setting, value);
            }
        }

        private void CreateTeams() {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="scenario"></param>
        /// <param name="companies"></param>
        /// <param name="gamemode"></param>
        /// <param name="enableAI"></param>
        /// <returns></returns>
        public static Session CreateSession(string scenario, Company[] companies, Wincondition gamemode, bool enableAI) {

            // Create the session
            Session session = new Session(scenario, companies, gamemode, enableAI);
            session.CreateTeams();

            // Return the new session
            return session;

        }

    }

}
