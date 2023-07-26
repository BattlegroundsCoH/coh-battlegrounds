using System;
using System.Windows;
using System.Windows.Media;

using Battlegrounds.Game.Match.Analyze;
using Battlegrounds.Game.Match.Finalizer;
using Battlegrounds.Game.Match.Play.Factory;
using Battlegrounds.Game.Match.Startup;
using Battlegrounds.Lobby.Components;
using Battlegrounds.Modding;

using Battlegrounds.Networking.LobbySystem;

namespace Battlegrounds.Lobby.Playing;

/// <summary>
/// 
/// </summary>
public sealed class SingleplayerModel : BasePlayModel, IPlayModel {

    public SingleplayerModel(ILobbyHandle handler, ChatSpectator lobbyChat) : base(handler, lobbyChat) {

        // Startup strategy
        this.m_startupStrategy = new SingleplayerStartupStrategy(handler.Game) {
            LocalCompanyCollector = () => this.m_selfCompany,
            SessionInfoCollector = () => this.m_info,
            PlayStrategyFactory = new OverwatchStrategyFactory(handler.Game),
        };

        // Analysis strategy
        this.m_matchAnalyzer = new SingleplayerMatchAnalyzer { };

        // Finalizer strategy
        this.m_finalizeStrategy = new SingleplayerFinalizer {
            CompanyHandler = this.OnCompanySerialized,
        };

    }

    public void Prepare(IModPackage modPackage, PrepareOverHandler prepareOver, PrepareCancelHandler prepareCancelled) {

        // For singleplayer we can just invoke the base play
        this.BasePrepare(modPackage, prepareCancelled);

        // Trigger prepare over immediately
        Application.Current.Dispatcher.Invoke(() => {
            prepareOver?.Invoke(this);
        });

    }

    public void Play(GameStartupHandler? startupHandler, PlayOverHandler matchOver) {

        // Ensure controller exists
        if (this.m_controller is null)
            throw new InvalidOperationException("Cannot invoke 'Play' until a controller instance has been defined.");

        // Set on complete handler
        this.m_controller.Complete += m => GameCompleteHandler(m, matchOver);
        this.m_controller.Error += (_, r) => GameErrorHandler(r, matchOver);

        // Invoke startup handler
        startupHandler?.Invoke();

        // Begin match
        this.m_controller.Control();

    }

    private void GameErrorHandler(string r, PlayOverHandler matchOver) {

        // Log error
        this.m_chat.SystemMessage(BattlegroundsContext.Localize.GetString("SystemMessage_MatchError", r), Colors.Red);

        // Invoke over event in lobby model.
        Application.Current.Dispatcher.Invoke(() => {
            matchOver.Invoke(this);
        });

    }

    private void GameCompleteHandler(IAnalyzedMatch match, PlayOverHandler handler) {

        // do stuff with match?
        if (match.IsFinalizableMatch) {
            this.m_chat.SystemMessage(BattlegroundsContext.Localize.GetString("SystemMessage_MatchSaved"), Colors.DarkGray);
        } else {
            this.m_chat.SystemMessage(BattlegroundsContext.Localize.GetString("SystemMessage_MatchInvalid"), Colors.DarkGray);
        }

        // Invoke over event in lobby model.
        Application.Current.Dispatcher.Invoke(() => {
            handler.Invoke(this);
        });

    }

}
