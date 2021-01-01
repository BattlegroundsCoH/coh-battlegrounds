using System.Windows.Controls;

namespace BattlegroundsApp.Controls.Lobby {

    public abstract class LobbyControlState : TabItem, IState {

        public StateChangeRequestHandler StateChangeRequest { get; set; } = null;

        public LobbyControlState() {
            this.Header = null;
        }

        public abstract void StateOnFocus();

        public abstract void StateOnLostFocus();

        public abstract void SetStateIdentifier(ulong ownerID, bool isAI);

        public abstract bool IsCorrectState(LobbyControlContext context);

    }

}
