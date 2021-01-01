using Battlegrounds;

namespace BattlegroundsApp.Controls.Lobby {
    
    public class SelfState : BasicState {

        public override bool IsCorrectState(LobbyControlContext context) => context.IsAI && context.IsHost || context.ClientID == BattlegroundsInstance.LocalSteamuser.ID;

        public override void SetStateIdentifier(ulong ownerID, bool isAI) { 
        }

    }

}
