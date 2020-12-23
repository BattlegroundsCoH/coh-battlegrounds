using System.Windows;

namespace BattlegroundsApp {
    
    public abstract class ViewStateMachine : Window, IStateMachine<ViewState> {

        private ViewState m_currentState;

        public ViewState State {
            get => this.m_currentState;
            set => this.SetState(value);
        }

        public virtual void SetState(ViewState state) {
            this.m_currentState?.StateOnLostFocus();
            this.m_currentState = state;
            this.m_currentState.StateChangeRequest = this.GetRequestHandler();
            this.m_currentState.StateOnFocus();
        }

        public abstract bool StateChangeRequest(object request);

        public abstract StateChangeRequestHandler GetRequestHandler();

    }

}
