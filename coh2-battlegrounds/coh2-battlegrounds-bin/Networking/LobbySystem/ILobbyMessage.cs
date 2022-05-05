namespace Battlegrounds.Networking.LobbySystem;

public interface ILobbyMessage {

    public string Timestamp { get; }
    public string Timezone { get; }
    public string Sender { get; }
    public string Message { get; }
    public string Channel { get; }
    public string Colour { get; }

}
