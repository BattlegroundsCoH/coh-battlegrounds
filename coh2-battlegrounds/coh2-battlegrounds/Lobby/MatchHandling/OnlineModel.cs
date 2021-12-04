using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Battlegrounds.Modding;
using Battlegrounds.Networking.LobbySystem;

using BattlegroundsApp.Lobby.MVVM.Models;

namespace BattlegroundsApp.Lobby.MatchHandling;

internal class OnlineModel : IPlayModel {

    private readonly LobbyAPI m_handle;

    public OnlineModel(LobbyAPI handler, LobbyChatSpectatorModel lobbyChat) {
        this.m_handle = handler;
    }

    public void Prepare(ModPackage modPackage, PrepareOverHandler prepareOver, PrepareCancelledHandler prepareCancelled) {

    }

    public void Play(PlayOverHandler matchOver) {

    }

}
