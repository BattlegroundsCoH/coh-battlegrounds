using Battlegrounds;
using Battlegrounds.Game.Database;
using Battlegrounds.Game.Gameplay;

namespace BattlegroundsApp.Models {
    
    /// <summary>
    /// Model representation of the selected <see cref="Battlegrounds.Game.Gameplay.Wincondition"/> in the lobby.
    /// </summary>
    public class LobbyGamemodeModel {

        private string m_wincondition;
        private int m_selectedSetting;

        private Wincondition m_wc;

        /// <summary>
        /// Get the name of the selected wincondition
        /// </summary>
        public string GamemodeName => this.m_wincondition;

        /// <summary>
        /// Get the selected option index.
        /// </summary>
        public int GamemodeOptionIndex => this.m_selectedSetting;

        /// <summary>
        /// Get the detected wincondition option.
        /// </summary>
        public Wincondition Wincondition => this.m_wc;

        /// <summary>
        /// Create new gamemode model with default settings (No selected gamemode or option).
        /// </summary>
        public LobbyGamemodeModel() {
            this.m_wincondition = string.Empty;
            this.m_selectedSetting = 0;
        }

        /// <summary>
        /// Set to <see cref="BattlegroundsInstance"/> defaults.
        /// </summary>
        public void SetDefaults() {
            this.m_wincondition = BattlegroundsInstance.LastPlayedGamemode;
            this.m_selectedSetting = BattlegroundsInstance.LastPlayedGamemodeSetting;
        }

        /// <summary>
        /// Save current values as <see cref="BattlegroundsInstance"/> defaults.
        /// </summary>
        public void SaveDefaults() {
            BattlegroundsInstance.LastPlayedGamemode = this.m_wincondition;
            BattlegroundsInstance.LastPlayedGamemodeSetting = this.m_selectedSetting;
        }

        /// <summary>
        /// Check if the selected gamemode can be retrieved.
        /// </summary>
        /// <returns><see langword="true"/> if the gamemode was found. Otherwise <see langword="false"/>.</returns>
        public bool GetGamemode() {
            try {
                this.m_wc = WinconditionList.GetWinconditionByName(this.m_wincondition);
            } catch {
                return false;
            }
            return this.m_wc is not null;
        }

        /// <summary>
        /// Set the wincondition.
        /// </summary>
        /// <param name="wincondition"></param>
        public void SetWincondition(string wincondition) => this.m_wincondition = wincondition;

        /// <summary>
        /// Set the wincondition.
        /// </summary>
        /// <param name="wincondition"></param>
        public void SetWincondition(Wincondition wincondition) {
            this.m_wc = wincondition;
            this.m_wincondition = wincondition.Name;
        }

        /// <summary>
        /// Set the wincondition setting.
        /// </summary>
        /// <param name="value"></param>
        public void SetSetting(int value) => this.m_selectedSetting = value;

    }

}
