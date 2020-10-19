using System;

namespace Battlegrounds.Online.Lobby {

    public sealed class HumanLobbyMember : ManagedLobbyMember {

        public override ulong ID => throw new NotImplementedException();

        public override string Name => throw new NotImplementedException();

        public override string Faction => throw new NotImplementedException();

        public override string CompanyName => throw new NotImplementedException();

        public override double CompanyStrength => throw new NotImplementedException();

        public override int LobbyIndex => throw new NotImplementedException();

        public override bool IsSamePlayer(ManagedLobbyMember other) {
            if (other is HumanLobbyMember otherHuman) {
                return otherHuman.ID == this.ID;
            } else {
                return false;
            }
        }

    }

}
