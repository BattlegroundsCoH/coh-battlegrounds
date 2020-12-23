using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace BattlegroundsApp {
    public abstract class CompanyBuilderMachine : ViewState, IStateMachine<ViewState> {

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
