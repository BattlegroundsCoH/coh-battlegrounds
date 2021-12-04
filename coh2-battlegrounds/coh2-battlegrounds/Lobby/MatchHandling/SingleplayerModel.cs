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

internal class SingleplayerModel : IPlayModel {

    // Load refs
    private readonly LobbyAPI m_handle;
    private readonly LobbyChatSpectatorModel m_chat;

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

    public SingleplayerModel(LobbyAPI handler, LobbyChatSpectatorModel lobbyChat) {
        
        // Set base stuff
        this.m_handle = handler;
        this.m_chat = lobbyChat;

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
        this.m_session = new MultiplayerSession(this.m_handle);

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
        string scenario = this.m_handle.Settings["selected_map"];
        string gamemode = this.m_handle.Settings["selected_wc"];
        string gamemodeValue = this.m_handle.Settings["selected_wco"];
        bool enableSupply = this.m_handle.Settings["selected_supply"] != "0";
        bool enableWeather = this.m_handle.Settings["selected_daynight"] != "0";

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
        var allies = this.m_handle.Allies.Slots.Filter(x => x.State is 1)
            .Map(x => x.Occupant).Map(x => CreateParticipantFromLobbyMember(x, ParticipantTeam.TEAM_ALLIES, totalCounter, alliesCounter));

        // Get axis
        var axis = this.m_handle.Axis.Slots.Filter(x => x.State is 1)
            .Map(x => x.Occupant).Map(x => CreateParticipantFromLobbyMember(x, ParticipantTeam.TEAM_AXIS, totalCounter, axisCounter));

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

    private SessionParticipant CreateParticipantFromLobbyMember(LobbyAPIStructs.LobbyMember participant, ParticipantTeam team, ValRef<byte> count, ValRef<byte> index) {
        byte tIndex = index.Change(x => x++);
        byte pIndex = count.Change(x => x++);
        if (participant.Role is 3) {
            var aiCompany = participant.Company;
            var c = aiCompany.IsAuto ? null : PlayerCompanies.FromNameAndFaction(aiCompany.Name, Faction.FromName(aiCompany.Army));
            return new SessionParticipant((AIDifficulty)participant.AILevel, c, team, tIndex, pIndex);
        } else {
            if (participant.MemberID == this.m_handle.Self.ID) {
                this.m_selfCompany = PlayerCompanies.FromNameAndFaction(participant.Company.Name, Faction.FromName(participant.Company.Army));
            }
            return new SessionParticipant(participant.DisplayName, participant.MemberID, null, team, tIndex, pIndex);
        }
    }

    private void HandleStartupCancel(IStartupStrategy sender, object caller, string reason)
        => this.m_chat.SystemMessage(reason, Colors.Red);

    private void HandleStartupInformation(IStartupStrategy sender, object caller, string message)
        => this.m_chat.SystemMessage(message, Colors.DarkGray);

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
        this.m_controller.Error += (o, r) => _gameErrorHandler(o, r, matchOver);

        // Begin match
        this.m_controller.Control();

    }

    private void _gameErrorHandler(object o, string r, PlayOverHandler matchOver) {

        // Log error
        this.m_chat.SystemMessage($"Match Error - {r}", Colors.Red);

        // Invoke over event in lobby model.
        Application.Current.Dispatcher.Invoke(() => {
            matchOver.Invoke(this);
        });

    }

    private void _gameCompleteHandler(IAnalyzedMatch match, PlayOverHandler handler) {

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
