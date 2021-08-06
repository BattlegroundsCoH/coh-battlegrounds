namespace BattlegroundsApp {

    /// <summary>
    /// Interface for a State Machine representation.
    /// </summary>
    /// <typeparam name="T">The specific implementation of the <see cref="IState"/>.</typeparam>
    public interface IStateMachine<T> where T : IState {

        /// <summary>
        /// Gets or sets the currently active <see cref="IState"/>.
        /// </summary>
        public T State { get; set; }

        /// <summary>
        /// Sets the currently active <see cref="IState"/>.
        /// </summary>
        /// <param name="state">The state to set as currently active.</param>
        void SetState(T state);

        /// <summary>
        /// Handle change state requests through a request object.
        /// </summary>
        /// <param name="request">The request object.</param>
        /// <returns>Will return <see langword="true"/> if the reuqested state was set as active. Otherwise <see langword="false"/>.</returns>
        bool StateChangeRequest(object request);

        /// <summary>
        /// Get the <see cref="StateChangeRequestHandler"/> that will hande request changes.
        /// </summary>
        /// <returns>The default <see cref="StateChangeRequestHandler"/> associated with the <see cref="ViewStateMachine"/> instance.</returns>
        StateChangeRequestHandler GetRequestHandler();

    }

}
