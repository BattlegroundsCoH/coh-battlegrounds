namespace BattlegroundsApp.Controls.Lobby.Chatting {

    /// <summary>
    /// Delegate for handing receiving messages.
    /// </summary>
    /// <param name="sender">The user or system sending message.</param>
    /// <param name="message">The actual message.</param>
    public delegate void ChatMessageReceived(string sender, string message);

    /// <summary>
    /// Interface for bridging interactions between a chat UI and actual chat handler.
    /// </summary>
    public interface IChatController {

        /// <summary>
        /// Occurs whenever a chat message is received.
        /// </summary>
        event ChatMessageReceived OnReceived;

        /// <summary>
        /// Get the display name of self.
        /// </summary>
        string Self { get; }

        /// <summary>
        /// Send a message from the UI to other chat members.
        /// </summary>
        /// <param name="message">The string message to send.</param>
        void SendChatMessage(string message);

    }

}
