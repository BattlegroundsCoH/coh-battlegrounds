using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Battlegrounds.Game.DataCompany;
using Battlegrounds.Game.Match;
using Battlegrounds.Game.Match.Analyze;
using Battlegrounds.Game.Match.Composite;
using Battlegrounds.Game.Match.Finalizer;
using Battlegrounds.Game.Match.Play.Factory;
using Battlegrounds.Game.Match.Startup;
using Battlegrounds.Modding;
using Battlegrounds.Networking.LobbySystem;

namespace BattlegroundsApp.Lobby.MatchHandling;

internal class SingleplayerModel : IPlayModel {

    // Load refs
    private readonly LobbyHandler m_handler;
    private readonly ILobby m_lobby;

    // The strategies to use
    private readonly IStartupStrategy m_startupStrategy;
    private readonly IAnalyzeStrategy m_matchAnalyzer;
    private readonly IFinalizeStrategy m_finalizeStrategy;

    // Local prepared data
    private SessionInfo m_info;
    private Company m_selfCompany;

    // The session handler
    private MultiplayerSession m_session;

    public SingleplayerModel(LobbyHandler handler) {
        
        // Set base stuff
        this.m_handler = handler;
        this.m_lobby = this.m_handler.Lobby;

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

    public void Prepare(ModPackage modPackage, PrepareOverHandler prepareOver, PrepareCancelledHandler prepareCancelled) {

        // Collect settings
        string scenario = this.m_lobby.GetLobbySetting("selected_map");
        string gamemode = this.m_lobby.GetLobbySetting("selected_wc");
        string gamemodeValue = this.m_lobby.GetLobbySetting("selected_wco");


        // Add listeners
        this.m_startupStrategy.StartupFeedback += this.HandleStartupInformation;
        this.m_startupStrategy.StartupCancelled += this.HandleStartupCancel;

        // Create session
        this.m_session = new MultiplayerSession(this.m_handler);


    }

    private void HandleStartupCancel(IStartupStrategy sender, object caller, string reason) {
        throw new NotImplementedException();
    }

    private void HandleStartupInformation(IStartupStrategy sender, object caller, string message) {
        throw new NotImplementedException();
    }

    private void OnCompanySerialized(Company company) {
        throw new NotImplementedException();
    }

    public void Play(PlayOverHandler matchOver) {

    }

}
