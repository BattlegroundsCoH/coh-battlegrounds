using System.Diagnostics;
using System.Windows;
using System.Windows.Media;

using Battlegrounds.Functional;
using Battlegrounds.Game;
using Battlegrounds.Game.Database;
using Battlegrounds.Game.DataCompany;
using Battlegrounds.Game.Gameplay;
using Battlegrounds.Game.Match;
using Battlegrounds.Game.Match.Analyze;
using Battlegrounds.Game.Match.Composite;
using Battlegrounds.Game.Match.Finalizer;
using Battlegrounds.Game.Match.Play.Factory;
using Battlegrounds.Game.Match.Startup;
using Battlegrounds.Modding;
using Battlegrounds.Networking.LobbySystem;
using Battlegrounds.Util;
using Battlegrounds.Verification;

using BattlegroundsApp.Lobby.MVVM.Models;
using BattlegroundsApp.LocalData;

namespace BattlegroundsApp.Lobby.MatchHandling;

internal class SingleplayerModel : BasePlayModel, IPlayModel {

    public SingleplayerModel(LobbyAPI handler, LobbyChatSpectatorModel lobbyChat) : base(handler, lobbyChat) {
        
        // Startup strategy
        this.m_startupStrategy = new SingleplayerStartupStrategy {
            LocalCompanyCollector = () => this.m_selfCompany,
            SessionInfoCollector = () => this.m_info,
            PlayStrategyFactory = new OverwatchStrategyFactory(),
        };

        // Analysis strategy
        this.m_matchAnalyzer = new SingleplayerMatchAnalyzer {};

        // Finalizer strategy
        this.m_finalizeStrategy = new SingleplayerFinalizer {
            CompanyHandler = this.OnCompanySerialized,
        };

    }

    public void Prepare(ModPackage modPackage, PrepareOverHandler prepareOver, PrepareCancelHandler prepareCancelled) {

        // For singleplayer we can just invoke the base play
        this.BasePrepare(modPackage, prepareCancelled);

        // Trigger prepare over immediately
        Application.Current.Dispatcher.Invoke(() => {
            prepareOver?.Invoke(this);
        });

    }

    public void Play(PlayOverHandler matchOver) {

        // Set on complete handler
        this.m_controller.Complete += m => GameCompleteHandler(m, matchOver);
        this.m_controller.Error += (o, r) => GameErrorHandler(o, r, matchOver);

        // Begin match
        this.m_controller.Control();

    }

    private void GameErrorHandler(object o, string r, PlayOverHandler matchOver) {

        // Log error
        this.m_chat.SystemMessage($"Match Error - {r}", Colors.Red);

        // Invoke over event in lobby model.
        Application.Current.Dispatcher.Invoke(() => {
            matchOver.Invoke(this);
        });

    }

    private void GameCompleteHandler(IAnalyzedMatch match, PlayOverHandler handler) {

        // do stuff with match?
        if (match.IsFinalizableMatch) {
            this.m_chat.SystemMessage("Match over - Invalid match.", Colors.DarkGray);
        } else {
            this.m_chat.SystemMessage("Match over - Match saved.", Colors.DarkGray);
        }

        // Invoke over event in lobby model.
        Application.Current.Dispatcher.Invoke(() => {
            handler.Invoke(this);
        });

    }

}
