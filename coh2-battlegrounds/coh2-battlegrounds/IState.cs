namespace BattlegroundsApp {

    /// <summary>
    /// Interface for representing a state in a <see cref="IStateMachine{T}"/>.
    /// </summary>
    public interface IState {

        /// <summary>
        /// Gets or sets the change state request handler.
        /// </summary>
        StateChangeRequestHandler StateChangeRequest { get; set; }

        /// <summary>
        /// Informs the <see cref="IState"/> that focus was gained.
        /// </summary>
        void StateOnFocus();

        /// <summary>
        /// Informs the <see cref="IState"/> focus was lost.
        /// </summary>
        void StateOnLostFocus();

    }

}
