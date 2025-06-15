namespace Battlegrounds.Models.Lobbies;

public enum ChatChannel {
    All,
    Team
}

public sealed record ChatMessage(string SenderId, string Sender, ChatChannel Channel, string Message);