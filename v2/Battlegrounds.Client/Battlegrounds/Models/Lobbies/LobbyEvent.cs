namespace Battlegrounds.Models.Lobbies;

public enum LobbyEventType {
    ConnectionStateChanged,
    SnapshotApplied,
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
    MatchOver,
    TrayMessage, // Never sent or received by the server, only used for the client to display messages in the tray
    TrayMessageHide, // Never sent or received by the server, only used for the client to hide messages in the tray
    SystemMessage,
    SystemError,
    UpdatedCompany,
    DownloadInitiated,
    DownloadProgress,
    DownloadCompleted,
    SlotCompanyDownloadProgress, // Never sent or received by the server, only used for the client to update the download progress of a company in a slot
}

public enum LobbyConnectionState {
    Connecting,
    Connected,
    Reconnecting,
    Disconnected,
    Disposed,
}

public sealed record SlotCompanyDownloadUpdate(
    int TeamId,
    int SlotId,
    string CompanyId,
    long SlotRevision,
    float? Progress = null,
    Models.Companies.Company? Company = null);

public sealed record LobbyEvent(LobbyEventType EventType, object? Arg = null, long Revision = 0);
