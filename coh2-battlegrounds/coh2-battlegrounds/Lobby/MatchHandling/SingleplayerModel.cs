using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

using Battlegrounds.Functional;
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

using BattlegroundsApp.LocalData;

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
    private MatchController m_controller;

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

        // Set lobby status and lock players in (at least in the multiplayer phase)

        // Create settings
        this.CreateMatchInfo(modPackage);

        // Add listeners
        this.m_startupStrategy.StartupFeedback += this.HandleStartupInformation;
        this.m_startupStrategy.StartupCancelled += this.HandleStartupCancel;

        // Create session
        this.m_session = new MultiplayerSession(this.m_handler);

        // Create controller
        this.m_controller = new();
        this.m_controller.SetStartupObjects(this.m_session, this.m_startupStrategy);
        this.m_controller.SetAnalysisObjects(this.m_session, this.m_matchAnalyzer);
        this.m_controller.SetFinalizerObjects(this.m_session, this.m_finalizeStrategy);

        // Trigger prepare over
        Application.Current.Dispatcher.Invoke(() => {
            prepareOver?.Invoke(this);
        });

    }

    private void CreateMatchInfo(ModPackage package) {

        // Collect settings
        string scenario = this.m_lobby.GetLobbySetting("selected_map");
        string gamemode = this.m_lobby.GetLobbySetting("selected_wc");
        string gamemodeValue = this.m_lobby.GetLobbySetting("selected_wco");
        bool enableSupply = this.m_lobby.GetLobbySetting("selected_supply") == "0" ? false : true;
        bool enableWeather = this.m_lobby.GetLobbySetting("selected_daynight") == "0" ? false : true;

        // Try get gamemode value
        if (!int.TryParse(gamemodeValue, out int gamemodeoption)) {
            Trace.WriteLine($"Failed to convert gamemode option {gamemodeoption} into an integer value.", nameof(SingleplayerModel));
            gamemodeoption = 0;
        }

        // Create three counters
        ValRef<byte> totalCounter = 0;
        ValRef<byte> alliesCounter = 0;
        ValRef<byte> axisCounter = 0;

        // Get allies
        var allies = this.m_lobby.Allies.Slots.Filter(x => x.SlotState is TeamSlotState.Occupied)
            .Map(x => x.SlotOccupant).Map(x => CreateParticipantFromLobbyMember(x, ParticipantTeam.TEAM_ALLIES, totalCounter, alliesCounter));

        // Get axis
        var axis = this.m_lobby.Axis.Slots.Filter(x => x.SlotState is TeamSlotState.Occupied)
            .Map(x => x.SlotOccupant).Map(x => CreateParticipantFromLobbyMember(x, ParticipantTeam.TEAM_AXIS, totalCounter, axisCounter));

        // Create info data
        this.m_info = new() {
            FillAI = false,
            SelectedScenario = ScenarioList.FromFilename(scenario),
            SelectedGamemode = WinconditionList.GetGamemodeByName(package.GamemodeGUID, gamemode),
            SelectedGamemodeOption = gamemodeoption,
            IsOptionValue = true,
            SelectedTuningMod = ModManager.GetMod<ITuningMod>(package.TuningGUID),
            Allies = allies,
            Axis = axis,
            EnableDayNightCycle = enableWeather,
            EnableSupply = enableSupply
        };

    }

    private SessionParticipant CreateParticipantFromLobbyMember(ILobbyParticipant participant, ParticipantTeam team, ValRef<byte> count, ValRef<byte> index) {
        byte tIndex = index.Change(x => x++);
        byte pIndex = count.Change(x => x++);
        if (participant is ILobbyAIParticipant ai) {
            var aiCompany = participant.Company;
            var c = aiCompany.IsAuto ? null : PlayerCompanies.FromNameAndFaction(aiCompany.Name, Faction.FromName(aiCompany.Faction));
            return new SessionParticipant(ai.AIDifficulty, c, team, tIndex, pIndex);
        } else {
            if (participant.IsSelf) {
                this.m_selfCompany = PlayerCompanies.FromNameAndFaction(participant.Company.Name, Faction.FromName(participant.Company.Faction));
            }
            return new SessionParticipant(participant.Name, participant.Id, null, team, tIndex, pIndex);
        }
    }

    private void HandleStartupCancel(IStartupStrategy sender, object caller, string reason) {
        // Display cancel
    }

    private void HandleStartupInformation(IStartupStrategy sender, object caller, string message) {
        // Display message
    }

    private void OnCompanySerialized(Company company) {

        // Run through a sanitizer
        try {

            // Save the company (by literally converting it to and from json for checksum violation checks).
            var selfCompany = CompanySerializer.GetCompanyFromJson(CompanySerializer.GetCompanyAsJson(company));

            // Save the company
            PlayerCompanies.SaveCompany(selfCompany);

        } catch (ChecksumViolationException checksumViolation) {

            // Log checksum violation
            Trace.WriteLine(checksumViolation, nameof(SingleplayerModel));

        }

    }

    public void Play(PlayOverHandler matchOver) {

        // Set on complete handler
        this.m_controller.Complete += m => _gameCompleteHandler(m, matchOver);

        // Begin match
        this.m_controller.Control();

    }

    private void _gameCompleteHandler(IAnalyzedMatch match, PlayOverHandler handler) {

        // do stuff with match?

        Application.Current.Dispatcher.Invoke(() => {
            handler.Invoke(this);
        });

    }

}
