using System;
using System.Threading.Tasks;
using Battlegrounds.Game;

namespace Battlegrounds.Online.Lobby {
    
    public sealed class AILobbyMember : ManagedLobbyMember {

        private ManagedLobby m_lobby;
        private AIDifficulty m_diff;
        private ulong m_uniqueID;
        private string m_faction;

        /// <summary>
        /// 
        /// </summary>
        public AIDifficulty Difficulty => this.m_diff;

        public override ulong ID => this.m_uniqueID;

        public override string Name => this.m_diff.GetIngameDisplayName();

        public override string Faction => this.m_faction;

        public override string CompanyName => string.Empty;

        public override double CompanyStrength => 0;

        public AILobbyMember(ManagedLobby lobby, AIDifficulty difficulty, string faction, ulong id) {
            this.m_lobby = lobby;
            this.m_diff = difficulty;
            this.m_faction = faction;
            this.m_uniqueID = id;
        }

        public override void UpdateFaction(string faction) => this.m_faction = faction;

        public override void UpdateCompany(string name, double strength) {}

        public override bool IsSamePlayer(ManagedLobbyMember other) {
            if (other is AILobbyMember ai) {
                return ai.ID == other.ID;
            } else {
                return false;
            }
        }

    }

}
