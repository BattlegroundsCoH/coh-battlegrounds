namespace Battlegrounds.Networking.LobbySystem;

/// <summary>
/// Event handler for <see cref="OnlineLobbyHandle"/> events.
/// </summary>
public delegate void LobbyEventHandler();

/// <summary>
/// Event handler for <see cref="OnlineLobbyHandle"/> events with an argument object.
/// </summary>
/// <typeparam name="T">The argument object type.</typeparam>
/// <param name="args">The arguments to pass aling to the handler.</param>
public delegate void LobbyEventHandler<T>(T args);

/// <summary>
/// Event arguments to a startup cancelled event.
/// </summary>
/// <param name="CancelId">The ID of the user who cancelled the startup.</param>
/// <param name="CancelName">The name of the user who cancelled the startup.</param>
public record LobbyMatchStartupCancelledEventArgs(ulong CancelId, string CancelName);

/// <summary>
/// Event arguments to the settings changed event.
/// </summary>
/// <param name="SettingsKey">The setting that was changed.</param>
/// <param name="SettingsValue">The new value of the setting.</param>
public record LobbySettingsChangedEventArgs(string SettingsKey, string SettingsValue);

/// <summary>
/// Event arguments to the system message event.
/// </summary>
/// <param name="MemberId">The ID of the user who caused the message.</param>
/// <param name="SystemMessage">The message to display.</param>
/// <param name="SystemContext">The context of the system message.</param>
public record LobbySystemMessageEventArgs(ulong MemberId, string SystemMessage, string SystemContext);

/// <summary>
/// Event arguments for the company changed event.
/// </summary>
/// <param name="TeamId">The team of the changed company.</param>
/// <param name="SlotId">The slot of the changed company.</param>
/// <param name="Company">The new company.</param>
public record LobbyCompanyChangedEventArgs(int TeamId, int SlotId, ILobbyCompany Company);

/// <summary>
/// Event arguments for match the event a match is halted.
/// </summary>
/// <param name="IsError">Flag marking whether the infor is an error or general information.</param>
/// <param name="Type">The type of halt (Where in the match process we were halted, and the severity)</param>
/// <param name="Reason">The reason given for the halt.</param>
public record LobbyMatchInfoEventArgs(bool IsError, string Type, string Reason);

/// <summary>
/// 
/// </summary>
/// <param name="isSuccess"></param>
/// <param name="api"></param>
public delegate void LobbyConnectCallback(bool isSuccess, ILobbyHandle? api);
