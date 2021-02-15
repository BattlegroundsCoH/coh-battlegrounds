using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Battlegrounds;
using Battlegrounds.Game.Database;
using Battlegrounds.Game.Gameplay;

namespace BattlegroundsApp.Models {
    
    /// <summary>
    /// 
    /// </summary>
    public class LobbyGamemodeModel {

        private string m_wincondition;
        private int m_selectedSetting;

        private Wincondition m_wc;

        /// <summary>
        /// 
        /// </summary>
        public string GamemodeName => this.m_wincondition;

        /// <summary>
        /// 
        /// </summary>
        public int GamemodeOptionIndex => this.m_selectedSetting;

        /// <summary>
        /// 
        /// </summary>
        public Wincondition Wincondition => this.m_wc;

        public LobbyGamemodeModel() {
            this.m_wincondition = string.Empty;
            this.m_selectedSetting = 0;
        }

        public void SetDefaults() {
            this.m_wincondition = BattlegroundsInstance.LastPlayedGamemode;
            this.m_selectedSetting = BattlegroundsInstance.LastPlayedGamemodeSetting;
        }

        public void SaveDefaults() {
            BattlegroundsInstance.LastPlayedGamemode = this.m_wincondition;
            BattlegroundsInstance.LastPlayedGamemodeSetting = this.m_selectedSetting;
        }

        public bool GetGamemode() {
            try {
                this.m_wc = WinconditionList.GetWinconditionByName(this.m_wincondition);
            } catch {
                return false;
            }
            return this.m_wc is not null;
        }

        public void SetWincondition(string wincondition) => this.m_wincondition = wincondition;

        public void SetWincondition(Wincondition wincondition) {
            this.m_wc = wincondition;
            this.m_wincondition = wincondition.Name;
        }

        public void SetSetting(int value) => this.m_selectedSetting = value;

    }

}
