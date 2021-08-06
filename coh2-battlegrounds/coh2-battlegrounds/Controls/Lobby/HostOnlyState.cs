namespace BattlegroundsApp.Controls.Lobby {

    public class HostOnlyState : BasicState {

        public override bool IsCorrectState(LobbyControlContext context) => context.IsHost;

        public override void SetStateIdentifier(ulong ownerID, bool isAI) { }

    }

}
