namespace Battlegrounds.Online.Lobby {
    
    /// <summary>
    /// Represents a status report on a <see cref="ManagedLobby"/> query.
    /// </summary>
    public readonly struct ManagedLobbyStatus {
        
        /// <summary>
        /// Was query a success.
        /// </summary>
        public bool Success { get; }

        /// <summary>
        /// The given message returned by the query.
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// Create new <see cref="ManagedLobbyStatus"/> instance with a boolean flag telling the success state of the query.
        /// </summary>
        /// <param name="_s">The boolean success flag to set.</param>
        public ManagedLobbyStatus(bool _s) {
            this.Success = _s;
            this.Message = string.Empty;
        }

        /// <summary>
        /// Create new <see cref="ManagedLobbyStatus"/> instance with a boolean flag telling the success state of the query, and the message that was returned.
        /// </summary>
        /// <param name="_s">The boolean success flag to set.</param>
        /// <param name="_m">The attached message from the query.</param>
        public ManagedLobbyStatus(bool _s, string _m) {
            this.Success = _s;
            this.Message = _m;
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString() => this.Message;

    }

}
