using System.Windows;

namespace BattlegroundsApp {
    
    /// <summary>
    /// View State Machine for handling views for a <see cref="Window"/>. Inherits from <see cref="IStateMachine{ViewState}"/>. Abstract class
    /// </summary>
    public abstract class ViewStateMachine : Window, IStateMachine<ViewState> {

        // The currently active state
        private ViewState m_currentState;

        /// <summary>
        /// Gets or sets the current state active in the State Machine.
        /// </summary>
        public ViewState State {
            get => this.m_currentState;
            set => this.SetState(value);
        }

        /// <summary>
        /// Sets the currently active <see cref="ViewState"/>. Will inform active state it has lost focus.
        /// </summary>
        /// <param name="state">The <see cref="ViewState"/> to set as active.</param>
        public virtual void SetState(ViewState state) {
            this.m_currentState?.StateOnLostFocus();
            this.m_currentState = state;
            this.m_currentState.StateChangeRequest = this.GetRequestHandler();
            this.m_currentState.UpdateGUI(() => { this.m_currentState.StateOnFocus(); });
        }

        /// <summary>
        /// Requests a change in state based on a requesting object.
        /// </summary>
        /// <param name="request">The request object to use when determining what state should be changed to.</param>
        /// <returns>Will return <see langword="true"/> if the reuqested state was set as active. Otherwise <see langword="false"/>.</returns>
        public abstract bool StateChangeRequest(object request);

        /// <summary>
        /// Get the <see cref="StateChangeRequestHandler"/> that will hande request changes.
        /// </summary>
        /// <returns>The default <see cref="StateChangeRequestHandler"/> associated with the <see cref="ViewStateMachine"/> instance.</returns>
        public abstract StateChangeRequestHandler GetRequestHandler();

    }

}
