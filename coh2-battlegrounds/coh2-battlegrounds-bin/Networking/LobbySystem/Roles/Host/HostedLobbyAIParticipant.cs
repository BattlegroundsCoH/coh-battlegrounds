using System;

using Battlegrounds.Game;

namespace Battlegrounds.Networking.LobbySystem.Roles.Host {

    public class HostedLobbyAIParticipant : HostedLobbyParticipant, ILobbyAIParticipant {

        private AIDifficulty m_diff;

        public AIDifficulty AIDifficulty { get; }

        public override string Name => this.AIDifficulty.GetIngameDisplayName();

        public HostedLobbyAIParticipant(AIDifficulty difficulty) : base(0, string.Empty) 
            => this.m_diff = difficulty;

        public bool SetDifficulty(AIDifficulty difficulty) {
            if (difficulty is AIDifficulty.Human) {
                throw new ArgumentException($"Invalid difficulty level '{difficulty}'.", nameof(difficulty));
            }
            this.m_diff = difficulty;
            return true;
        }

    }

}
