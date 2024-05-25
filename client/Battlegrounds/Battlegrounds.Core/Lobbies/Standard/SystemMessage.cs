namespace Battlegrounds.Core.Lobbies.Standard;

public class SystemMessage(DateTime timestamp, string message) : ILobbyChatMessage {

    public string Sender => "SYSTEM";

    public string Message => message;

    public DateTime Timestamp => timestamp;

}
