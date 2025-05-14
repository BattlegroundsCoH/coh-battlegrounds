namespace Battlegrounds.Models;

public sealed class BrowserLobby {

    public required string Id { get; init; }

    public required string Name { get; init; }

    public required string Host { get; init; }

    public required bool IsPasswordProtected { get; init; } = false;

    public required string Map { get; init; }

    public required string GameMode { get; init; }

    public required int MaxPlayers { get; init; }

    public required int CurrentPlayers { get; init; }

    public string Players => $"{CurrentPlayers}/{MaxPlayers}";

    public bool CanJoin => CurrentPlayers < MaxPlayers;

}
