using System.Windows;
using System.Windows.Controls;

namespace BattlegroundsApp.Controls.Lobby {

    public abstract class LobbyControlState : TabItem, IState {

        public static readonly DependencyProperty StateNameProperty = DependencyProperty.Register("StateName", typeof(string), typeof(LobbyControlState));

        public StateChangeRequestHandler StateChangeRequest { get; set; } = null;

        public string StateName { get => this.GetValue(StateNameProperty) as string; set => this.SetValue(StateNameProperty, value); }

        public LobbyControlState() {
            this.Header = null;
            this.Margin = new Thickness(0);
            this.Padding = new Thickness(0);
            if (string.IsNullOrEmpty(this.StateName)) {
                this.StateName = this.GetType().Name;
            }
        }

        public abstract void StateOnFocus();

        public abstract void StateOnLostFocus();

        public abstract void SetStateIdentifier(ulong ownerID, bool isAI);

        public abstract bool IsCorrectState(LobbyControlContext context);

    }

}
