namespace Battlegrounds.Core.Lobbies.Standard;

public class ChatMessage(DateTime timestamp, string sender, string message) : ILobbyChatMessage {

    public string Sender => sender;
    public string Message => message;

    public DateTime Timestamp => timestamp;

}
