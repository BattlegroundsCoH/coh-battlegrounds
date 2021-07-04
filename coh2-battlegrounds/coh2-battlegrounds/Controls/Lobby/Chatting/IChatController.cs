namespace BattlegroundsApp.Controls.Lobby.Chatting {

    /// <summary>
    /// Delegate for handling sending messages.
    /// </summary>
    /// <param name="channel">The channel ID</param>
    /// <param name="message">The actual message.</param>
    public delegate void ChatMessageSent(int channel, string message);

    /// <summary>
    /// Interface for bridging interactions between a chat UI and actual chat handler.
    /// </summary>
    public interface IChatController {

        /// <summary>
        /// Occurs whenever a chat message is sent.
        /// </summary>
        ChatMessageSent OnSend { get; }

    }

}
