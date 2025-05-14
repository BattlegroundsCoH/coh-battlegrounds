using System.Windows.Controls;

using Battlegrounds.Models;
using Battlegrounds.ViewModels;
using Battlegrounds.Views;

using Microsoft.Extensions.DependencyInjection;

namespace Battlegrounds.Factories;

public static class LobbyViewFactory {

    public static UserControl CreateLobbyViewForLobby(IServiceProvider serviceProvider, ILobby lobby) {
        var lobbyViewModel = ActivatorUtilities.CreateInstance<LobbyViewModel>(serviceProvider, lobby);
        var lobbyView = _ = ActivatorUtilities.CreateInstance<LobbyView>(serviceProvider, lobbyViewModel);
        return lobbyView;
    }

}
