namespace Battlegrounds.Models.Lobbies;

public enum LobbyEventType {
    ParticipantJoined,
    ParticipantLeft,
    ParticipantUpdated,
    ParticipantReady,
    ParticipantUnready,
    ParticipantMessage,
    TeamUpdated,
    SlotUpdated,
    SettingUpdated,
    MapUpdated,
    GameStarted,
    GameCancelled,
    GameEnded,
    SystemMessage,
    SystemError,
    UpdatedCompany,
    DownloadInitiated,
    DownloadProgress,
    DownloadCompleted
}

public sealed record LobbyEvent(LobbyEventType EventType, object? Arg = null);
