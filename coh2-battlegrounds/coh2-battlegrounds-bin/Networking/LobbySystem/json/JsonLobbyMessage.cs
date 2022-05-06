namespace Battlegrounds.Networking.LobbySystem.Json;

public record JsonLobbyMessage(string Timestamp, string Timezone, string Sender, string Message, string Channel, string Colour) : ILobbyMessage;
