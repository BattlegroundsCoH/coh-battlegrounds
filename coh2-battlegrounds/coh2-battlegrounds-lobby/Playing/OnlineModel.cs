using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

using Battlegrounds.Game.Match.Analyze;
using Battlegrounds.Game.Match.Finalizer;
using Battlegrounds.Game.Match.Play.Factory;
using Battlegrounds.Game.Match.Startup;
using Battlegrounds.Lobby.Components;
using Battlegrounds.Modding;
using Battlegrounds.Networking.Communication.Connections;

using Battlegrounds.Networking.LobbySystem;

namespace Battlegrounds.Lobby.Playing;

public sealed class OnlineModel : BasePlayModel, IPlayModel {

    public OnlineModel(ILobbyHandle handler, ChatSpectator lobbyChat, UploadProgressCallbackHandler callbackHandler, uint cancelTime) : base(handler, lobbyChat) {

        // Startup strategy
        this.m_startupStrategy = new OnlineStartupStrategy {
            LocalCompanyCollector = () => this.m_selfCompany,
            SessionInfoCollector = () => this.m_info,
            GamemodeUploadProgress = callbackHandler,
            PlayStrategyFactory = new OverwatchStrategyFactory(),
            StopMatchSeconds = cancelTime

        };

        // Analysis strategy
        this.m_matchAnalyzer = new OnlineMatchAnalyzer { };

        // Finalizer strategy
        this.m_finalizeStrategy = new MultiplayerFinalizer {
            CompanyHandler = this.OnCompanySerialized,
        };

    }

    public void Prepare(ModPackage modPackage, PrepareOverHandler prepareOver, PrepareCancelHandler prepareCancelled) {

        // Invoke async so we can wait on others without freezing
        Task.Run(() => {

            // Invoke base
            this.BasePrepare(modPackage, prepareCancelled);

            // Trigger prepare over on GUI thread
            Application.Current.Dispatcher.Invoke(() => {
                prepareOver?.Invoke(this);
            });

        });

    }

    public void Play(GameStartupHandler? startupHandler, PlayOverHandler matchOver) {

        // Ensure controller exists
        if (this.m_controller is null)
            throw new InvalidOperationException("Cannot invoke 'Play' until a controller instance has been defined.");

        // Set on complete handler
        this.m_controller.Complete += m => GameCompleteHandler(m, matchOver);
        this.m_controller.Error += (o, r) => GameErrorHandler(r, matchOver);

        // Invoke startup handler
        startupHandler?.Invoke();

        // Begin match
        this.m_controller.Control();

    }

    private void GameErrorHandler(string r, PlayOverHandler matchOver) {

        // Log error
        this.m_chat.SystemMessage(BattlegroundsInstance.Localize.GetString("SystemMessage_MatchError", r), Colors.Red);

        // Notify participants of error
        this.m_handle.NotifyError("MatchError", r);

        // Invoke over event in lobby model.
        Application.Current.Dispatcher.Invoke(() => {
            matchOver.Invoke(this);
        });

    }

    private void GameCompleteHandler(IAnalyzedMatch match, PlayOverHandler handler) {

        // do stuff with match?
        if (match.IsFinalizableMatch) {
            this.m_chat.SystemMessage(BattlegroundsInstance.Localize.GetString("SystemMessage_MatchSaved"), Colors.DarkGray);
        } else {
            this.m_chat.SystemMessage(BattlegroundsInstance.Localize.GetString("SystemMessage_MatchInvalid"), Colors.DarkGray);
        }

        // Invoke over event in lobby model.
        Application.Current.Dispatcher.Invoke(() => {
            handler.Invoke(this);
        });

    }

}
