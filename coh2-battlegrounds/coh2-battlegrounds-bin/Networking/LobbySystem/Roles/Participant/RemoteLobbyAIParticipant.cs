using System;

using Battlegrounds.Game;
using Battlegrounds.Networking.Memory;
using Battlegrounds.Networking.Remoting.Objects;
using Battlegrounds.Networking.Remoting.Reflection;

namespace Battlegrounds.Networking.LobbySystem.Roles.Participant {

    public class RemoteLobbyAIParticipant : RemoteLobbyParticipant, ILobbyAIParticipant {

        private readonly ValueCache<AIDifficulty> m_difficulty;

        public AIDifficulty AIDifficulty
            => this.m_difficulty.GetCachedValue(() => this.m_handle.GetRemoteProperty<AIDifficulty, ILobbyAIParticipant>(this.ObjectID));

        public override string Name => this.AIDifficulty.GetIngameDisplayName();

        public override bool IsSelf => false;

        public RemoteLobbyAIParticipant(IObjectID objectID, IRemoteHandle remoteHandle, AIDifficulty aIDifficulty) : base(objectID, remoteHandle, 0, string.Empty)
            => this.m_difficulty = new(aIDifficulty, TimeSpan.FromMinutes(0.5));

        [RemotableMethod]
        public bool SetDifficulty(AIDifficulty difficulty) {
            this.m_difficulty.SetCachedValue(difficulty);
            return true;
        }

    }

}
