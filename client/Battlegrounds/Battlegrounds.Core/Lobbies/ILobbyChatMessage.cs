namespace Battlegrounds.Core.Lobbies;

public interface ILobbyChatMessage {

    string Sender { get; }

    string Message { get; }

    DateTime Timestamp { get; }

}
