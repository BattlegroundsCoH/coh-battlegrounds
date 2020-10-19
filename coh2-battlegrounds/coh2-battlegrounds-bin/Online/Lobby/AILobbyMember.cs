using System;
using System.Threading.Tasks;
using Battlegrounds.Game;

namespace Battlegrounds.Online.Lobby {
    
    public sealed class AILobbyMember : ManagedLobbyMember {

        private ManagedLobby m_lobby;
        private AIDifficulty m_diff;
        private ulong m_uniqueID;
        private string m_faction;
        private int m_index;

        /// <summary>
        /// 
        /// </summary>
        public AIDifficulty Difficulty => this.m_diff;

        public override ulong ID => this.m_uniqueID;

        public override string Name => this.m_diff.GetIngameDisplayName();

        public override string Faction => this.m_faction;

        public override string CompanyName => string.Empty;

        public override double CompanyStrength => 0;

        public override int LobbyIndex => this.m_index;

        public AILobbyMember(ManagedLobby lobby, AIDifficulty difficulty, string faction, int index) {
            if (lobby.IsHost) {
                this.m_lobby = lobby;
                this.m_diff = difficulty;
                this.m_faction = faction;
                this.m_index = index;
                Task.Run(async () => {
                    this.m_uniqueID = (ulong)await this.m_lobby.TryCreateAIPlayer(difficulty, faction, index);
                });
            } else {
                throw new InvalidOperationException("Unable to add new AI lobby member when caller is not host!");
            }
        }

        public AILobbyMember(ManagedLobby lobby, AIDifficulty difficulty, string faction, int index, ulong id) {
            if (lobby.IsHost) {
                throw new InvalidOperationException("Unable to add new AI lobby member when caller is host!");
            } else {
                this.m_lobby = lobby;
                this.m_diff = difficulty;
                this.m_faction = faction;
                this.m_index = index;
                this.m_uniqueID = id;
            }
        }

        public override bool IsSamePlayer(ManagedLobbyMember other) {
            if (other is AILobbyMember ai) {
                return ai.ID == other.ID;
            } else {
                return false;
            }
        }

    }

}
