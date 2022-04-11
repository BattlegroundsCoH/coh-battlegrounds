using System.Windows;

using Battlegrounds.Networking.LobbySystem;

using BattlegroundsApp.Utilities;

namespace BattlegroundsApp.Lobby.MVVM.Models;

public abstract class LobbyContextMenu {

    public record LobbyContextAction(string Title, RelayCommand Click, bool Enabled, Visibility Visibility);

    public LobbyAPI Handle { get; }

    public LobbyContextMenu(LobbyAPI handle) {
        this.Handle = handle;
    }

}
