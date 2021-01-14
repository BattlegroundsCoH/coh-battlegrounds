using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace BattlegroundsApp.Controls.Lobby {

    /// <summary>
    /// Interaction logic for LobbyControl.xaml
    /// </summary>
    public partial class LobbyControl : TabControl, IStateMachine<LobbyControlState> {

        private LobbyControlState m_controlState;
        private LobbyControlContext m_context;

        public LobbyControlState State { get => this.m_controlState; set => this.SetState(value); }

        public StateChangeRequestHandler GetRequestHandler() => this.StateChangeRequest;

        public LobbyControl() : base() {
            Style style = new Style();
            style.Setters.Add(new Setter(VisibilityProperty, Visibility.Collapsed));
            this.ItemContainerStyle = style;
            this.Background = Brushes.Transparent;
            this.BorderBrush = Brushes.Transparent;
            this.BorderThickness = new Thickness(0);
        }

        public void SetState(LobbyControlState state) {
            this.m_controlState?.StateOnLostFocus();
            this.Dispatcher.Invoke(() => {
                this.SelectedIndex = this.Items.IndexOf(state);
                this.m_controlState = state;
                this.m_controlState.StateOnFocus();
            });
        }

        public bool StateChangeRequest(object request) => false;

        public virtual void SetStateBasedOnContext(bool isHost, bool isAI, ulong selfID) {
            this.m_context = new LobbyControlContext() { ClientID = selfID, IsHost = isHost, IsAI = isAI };
            foreach (object state in this.Items) {
                if (state is LobbyControlState controlState) {
                    if (controlState.IsCorrectState(this.m_context)) {
                        this.State = this.OnStateChange(controlState, this.m_context);
                        return;
                    }
                }
            }
        }

        protected virtual LobbyControlState OnStateChange(LobbyControlState newState, LobbyControlContext controlContext) => newState;

    }

}
