using System.Windows.Controls;

namespace BattlegroundsApp.Controls.CompanyBuilderControls {
    public abstract class CompanyBuilderControlState : TabItem, IState {

        public StateChangeRequestHandler StateChangeRequest { get; set; } = null;

        public CompanyBuilderControlState() {
            this.Header = null;
        }

        public abstract void StateOnFocus();
        public abstract void StateOnLostFocus();
        public abstract void SetStateIdentifier();
        public abstract bool IsCorrectState();
    }
}
