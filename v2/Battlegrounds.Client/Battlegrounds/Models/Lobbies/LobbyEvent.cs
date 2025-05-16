namespace Battlegrounds.Models.Lobbies;

public enum LobbyEventType {
    ParticipantJoined,
    ParticipantLeft,
    ParticipantUpdated,
    ParticipantReady,
    ParticipantUnready,
    ParticipantMessage,
    TeamUpdated,
    SettingUpdated,
    MapUpdated,
    GameStarted,
    GameEnded,
    SystemMessage,
    SystemError,
}

public sealed record LobbyEvent(LobbyEventType EventType, object? Arg);
