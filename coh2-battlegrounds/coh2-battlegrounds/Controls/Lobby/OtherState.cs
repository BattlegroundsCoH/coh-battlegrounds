using Battlegrounds;

namespace BattlegroundsApp.Controls.Lobby {

    public class OtherState : BasicState {

        public override bool IsCorrectState(LobbyControlContext context) => true;

        public override void SetStateIdentifier(ulong ownerID, bool isAI) { }

    }

}
