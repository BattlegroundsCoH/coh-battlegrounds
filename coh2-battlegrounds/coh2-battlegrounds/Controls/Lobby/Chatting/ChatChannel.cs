using System.Windows.Media;

namespace BattlegroundsApp.Controls.Lobby.Chatting {

    /// <summary>
    /// Represents a channel of chat in a <see cref="ChatView"/>.
    /// </summary>
    public class ChatChannel {

        /// <summary>
        /// Get or set chat channel name.
        /// </summary>
        public string ChannelName { get; set; }

        /// <summary>
        /// Get or set the chat channel colour.
        /// </summary>
        public Color ChannelColour { get; set; }

        public override string ToString() => this.ChannelName;

    }

}
