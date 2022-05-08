namespace Battlegrounds.Networking.LobbySystem;

/// <summary>
/// 
/// </summary>
public interface ILobbyChatNotifier {

    /// <summary>
    /// 
    /// </summary>
    public event LobbyEventHandler<ILobbyMessage>? OnChatMessage;

    /// <summary>
    /// Event triggered when the system announces a message.
    /// </summary>
    public event LobbyEventHandler<LobbySystemMessageEventArgs>? OnSystemMessage;

}
