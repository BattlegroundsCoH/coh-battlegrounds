using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Windows.Media;

using Battlegrounds.AI;
using Battlegrounds.AI.Lobby;
using Battlegrounds.DataLocal;
using Battlegrounds.Functional;
using Battlegrounds.Game.DataCompany;
using Battlegrounds.Game.Gameplay;
using Battlegrounds.Game.Match.Analyze;
using Battlegrounds.Game.Match.Composite;
using Battlegrounds.Game.Match.Finalizer;
using Battlegrounds.Game.Match.Startup;
using Battlegrounds.Game.Match;
using Battlegrounds.Game;
using Battlegrounds.Modding.Content;
using Battlegrounds.Modding;
using Battlegrounds.Networking.LobbySystem;
using Battlegrounds.Util;
using Battlegrounds.Util.Threading;
using Battlegrounds.Verification;
using Battlegrounds.Lobby.Components;
using Battlegrounds.Game.Blueprints;
using Battlegrounds.Game.Scenarios;
using Battlegrounds.Logging;

namespace Battlegrounds.Lobby.Playing;

/// <summary>
/// Abstract base class that handles gameplay startup, match setup, and game finish operations.
/// </summary>
public abstract class BasePlayModel {

    private static readonly Logger logger = Logger.CreateLogger();

    // Load refs
    protected readonly ILobbyHandle m_handle;
    protected readonly IChatSpectator m_chat;
    protected readonly IDispatcher m_dispatcher;

    // The strategies to use
    protected IStartupStrategy? m_startupStrategy;
    protected IAnalyzeStrategy? m_matchAnalyzer;
    protected IFinalizeStrategy? m_finalizeStrategy;

    // Local prepared data
    protected SessionInfo m_info;
    protected Company? m_selfCompany;

    // The session handler
    protected MultiplayerSession? m_session;
    protected MatchController? m_controller;

    // handlers for cancel/error cases
    private PrepareCancelHandler? m_prepCancelHandler;

    /// <summary>
    /// Initialise a <see cref="BasePlayModel"/> instance.
    /// </summary>
    /// <param name="handle">The lobby handle to create play instance for.</param>
    /// <param name="lobbyChat">The chat instance to use when communicating progress.</param>
    /// <param name="dispatcher">The dispatcher to invoke UI thread actions through.</param>
    public BasePlayModel(ILobbyHandle handle, IChatSpectator lobbyChat, IDispatcher dispatcher) {

        // Set base stuff
        this.m_handle = handle;
        this.m_chat = lobbyChat;
        this.m_dispatcher = dispatcher;

    }

    /// <summary>
    /// Prepares the play model for gameplay.
    /// </summary>
    /// <param name="modPackage">The mod package to use for preparation.</param>
    /// <param name="cancelHandler">Handler to call when the preparation is cancelled.</param>
    protected void BasePrepare(IModPackage modPackage, PrepareCancelHandler cancelHandler) {

        // Error if not set up
        if (this.m_startupStrategy is null) {
            throw new StartupException("Failed to invoke 'BasePlayModel.BasePrepare'. Please make sure the startup strategy is set.");
        }

        // Error if not set up
        if (this.m_matchAnalyzer is null) {
            throw new StartupException("Failed to invoke 'BasePlayModel.BasePrepare'. Please make sure the analyzer strategy is set.");
        }

        // Error if not set up
        if (this.m_finalizeStrategy is null) {
            throw new StartupException("Failed to invoke 'BasePlayModel.BasePrepare'. Please make sure the finalise strategy is set.");
        }

        // Create settings
        this.CreateMatchInfo(modPackage);

        // Add listeners
        this.m_startupStrategy.StartupFeedback += this.HandleStartupInformation;
        this.m_startupStrategy.StartupCancelled += this.HandleStartupCancel;

        // Create session
        this.m_session = new MultiplayerSession(this.m_handle);

        // Create controller
        this.m_controller = new MatchController(m_handle.Game);
        this.m_controller.SetStartupObjects(this.m_session, this.m_startupStrategy);
        this.m_controller.SetAnalysisObjects(this.m_session, this.m_matchAnalyzer);
        this.m_controller.SetFinalizerObjects(this.m_session, this.m_finalizeStrategy);

        // Set crash/err handler
        this.m_prepCancelHandler = cancelHandler;

    }

    /// <summary>
    /// Creates match information from the given mod package.
    /// </summary>
    /// <param name="package">The mod package to use for creating the match information.</param>
    protected void CreateMatchInfo(IModPackage package) {

        // Get settings (most up-to-date)
        var settings = this.m_handle.Settings;

        // Collect settings
        string scenario = settings.GetOrDefault("selected_map", string.Empty);
        string gamemode = settings.GetOrDefault("selected_wc", string.Empty);
        string gamemodeValue = settings.GetOrDefault("selected_wco", string.Empty);
        bool enableSupply = settings.GetOrDefault("selected_supply", "0") != "0";
        bool enableWeather = settings.GetOrDefault("selected_daynight", "0") != "0";

        // Try get gamemode value
        if (!int.TryParse(gamemodeValue, out int gamemodeoption)) {
            logger.Warning($"Failed to convert gamemode option {gamemodeoption} into an integer value.");
            gamemodeoption = 0;
        }

        // Create three counters
        ValRef<byte> totalCounter = 0;
        ValRef<byte> alliesCounter = 0;
        ValRef<byte> axisCounter = 0;

        // Get teams (most up-to-date)
        var alliesLobby = this.m_handle.Allies;
        var axisLobby = this.m_handle.Axis;

        // Get allies
        var allies = alliesLobby.Slots.Filter(x => x.State is 1)
            .Map(x => x.Occupant)
            .Map(x => x is null ? throw new StartupException("Invalid allied occupant") : CreateParticipantFromLobbyMember(x, ParticipantTeam.TEAM_ALLIES, totalCounter, alliesCounter));

        // Get axis
        var axis = axisLobby.Slots.Filter(x => x.State is 1)
            .Map(x => x.Occupant)
            .Map(x => x is null ? throw new StartupException("Invalid axis occupant") : CreateParticipantFromLobbyMember(x, ParticipantTeam.TEAM_AXIS, totalCounter, axisCounter));

        // Get scenario
        var scen = BattlegroundsContext.DataSource.GetScenarioList(package, this.m_handle.Game)!.FromFilename(scenario)
            ?? throw new StartupException($"Failed to fetch scenario {scenario} from scenario list.");

        // Grab gamemode
        var gamemodeInstance = BattlegroundsContext.DataSource.GetGamemodeList(package, this.m_handle.Game)!.GetGamemodeByName(package.GamemodeGUID, gamemode)
            ?? throw new StartupException($"Failed to find gamemode with name '{gamemode}' from wincondition list (mod package = {package.ID}).");

        // Grab tuning
        var tuningInstance = BattlegroundsContext.ModManager.GetMod<ITuningMod>(package.TuningGUID) 
            ?? throw new StartupException($"Failed to fetch tuning mod.");

        // Create plan containers
        SessionPlanEntityInfo[] sessionEntities;
        SessionPlanGoalInfo[] sessionGoals;
        SessionPlanSquadInfo[] sessionSquads;

        // Grab reverse flag
        var revflag = this.m_handle.AreTeamRolesSwapped();

        // Create plan info
        if (gamemodeInstance.HasPlanning && this.m_handle.PlanningHandle is not null) {

            // Union participants and map by index (and remove AI from consideration, as they have no valid UID)
            var allParticipants = allies.Concat(axis);
            var participants = allParticipants.Filter(x => x.IsHuman).ToLookup(x => x.GetId());

            // Grab elements
            var elements = this.m_handle.PlanningHandle.GetPlanningElements(4);

            // Get gamode instance
            var mode = package.Gamemodes.FirstOrDefault(x => x.ID == gamemodeInstance.Name, new());

            // Invoke helper functions
            sessionGoals = CreatePlanningGoals(participants, elements, (int)scen.PlayableSize.Length);
            sessionEntities = CreatePlanningEntities(participants, elements, (int)scen.PlayableSize.Length, mode, package);
            sessionSquads = CreatePlanningSquads(participants, elements, (int)scen.PlayableSize.Length);

            // Determine if there's need for AI planning
            this.CreateAIPlans(scen, revflag ? allies : axis, (byte)(revflag ? 0 : 1), ref sessionSquads, ref sessionEntities, sessionGoals, mode, package);

        } else {

            // Init empty data
            sessionEntities = Array.Empty<SessionPlanEntityInfo>();
            sessionGoals = Array.Empty<SessionPlanGoalInfo>();
            sessionSquads = Array.Empty<SessionPlanSquadInfo>();

        }

        // Create container for additional settings
        var additionalSettings = new Dictionary<string, int>();

        // Add custom settings
        for (int i = 0; i < gamemodeInstance.AuxiliaryOptions.Length; i++) {
            string optionKey = gamemodeInstance.AuxiliaryOptions[i].Name;
            if (int.TryParse(this.m_handle.Settings[optionKey], out int v)) {
                additionalSettings[optionKey] = v;
            }
        }

        // Create info data
        this.m_info = new() {
            FillAI = false,
            SelectedScenario = scen,
            SelectedGamemode = gamemodeInstance,
            SelectedGamemodeOption = gamemodeoption,
            IsOptionValue = true,
            SelectedTuningMod = tuningInstance,
            Allies = allies,
            Axis = axis,
            EnableDayNightCycle = enableWeather,
            EnableSupply = enableSupply,
            Squads = sessionSquads,
            Goals = sessionGoals,
            Entities = sessionEntities,
            IsFixedTeamOrder = gamemodeInstance.RequireFixed,
            ReverseTeamOrder = revflag,
            AdditionalOptions = additionalSettings
        };

        // Log this object (Helpful for debugging)
        logger.Info($"Session Startup Info:\n{JsonSerializer.Serialize(this.m_info, new JsonSerializerOptions() { WriteIndented = true })}");

    }

    /// <summary>
    /// Creates a participant for a session from a lobby member.
    /// </summary>
    /// <param name="participant">The lobby member to transform into a session participant.</param>
    /// <param name="team">The team of the participant.</param>
    /// <param name="count">The total count of participants.</param>
    /// <param name="index">The index of the participant.</param>
    /// <returns>The created session participant.</returns>
    protected SessionParticipant CreateParticipantFromLobbyMember(ILobbyMember participant, ParticipantTeam team, ValRef<byte> count, ValRef<byte> index) {

        // Update indicies
        byte tIndex = index.Change(x => ++x);
        byte pIndex = count.Change(x => ++x);

        // Add participant based on role
        if (participant.Role is 3) {
            var aiCompany = participant.Company ?? throw new StartupException("AI startup company was null!");
            var c = aiCompany.IsAuto ? null : Companies.FromNameAndFaction(aiCompany.Name, Faction.FromName(aiCompany.Army, this.m_handle.Game));
            return new SessionParticipant((AIDifficulty)participant.AILevel, c, team, tIndex, pIndex);
        } else {
            if (participant.MemberID == this.m_handle.Self.ID) {
                if (participant.Company is not ILobbyCompany c) {
                    throw new StartupException("Invalid startup company.");
                }
                this.m_selfCompany = Companies.FromNameAndFaction(c.Name, Faction.FromName(participant.Company.Army, this.m_handle.Game));
            }
            return new SessionParticipant(participant.DisplayName, participant.MemberID, null, team, tIndex, pIndex);
        }

    }

    private static GamePosition InvertPosition(GamePosition position)
        => position with {
            Y = -position.Y
        };

    protected static SessionPlanEntityInfo[] CreatePlanningEntities(IDictionary<ulong, SessionParticipant> participants, ILobbyPlanElement[] planElements, int height, Gamemode gamemode, IModPackage package) {

        // Grab planning entities
        var planEntities = planElements.Filter(x => x.IsEntity && x.ObjectiveType is PlanningObjectiveType.None);

        // Create array
        var entities = new SessionPlanEntityInfo[planEntities.Length];

        // Loop over all entities
        for (int i = 0; i < planEntities.Length; i++) {

            // Get participant
            var p = participants[planEntities[i].ElementOwnerId];

            // Create entity
            entities[i] = new() {
                TeamOwner = (int)p.TeamIndex,
                TeamMemberOwner = p.PlayerIndexOnTeam,
                Blueprint = package.GetDataSource().GetBlueprints(GameCase.CompanyOfHeroes2).FromBlueprintName<EntityBlueprint>(planEntities[i].Blueprint),
                Spawn = InvertPosition(planEntities[i].SpawnPosition),
                Lookat = planEntities[i].LookatPosition is GamePosition look ? InvertPosition(look) : null,
                IsDirectional = planEntities[i].IsDirectional,
                Width = gamemode.GetPlanningEntity(planEntities[i].Blueprint).Width
            };

        }

        // Return
        return entities;

    }

    protected static SessionPlanSquadInfo[] CreatePlanningSquads(IDictionary<ulong, SessionParticipant> participants, ILobbyPlanElement[] planElements, int height) {

        // Grab planning squads
        var planSquads = planElements.Filter(x => !x.IsEntity && x.ObjectiveType is PlanningObjectiveType.None);

        // Create array
        var squads = new SessionPlanSquadInfo[planSquads.Length];

        // Loop over all squads
        for (int i = 0; i < planSquads.Length; i++) {

            // Get participant
            var p = participants[planSquads[i].ElementOwnerId];

            // Create squad
            squads[i] = new() {
                TeamOwner = (int)p.TeamIndex,
                TeamMemberOwner = p.PlayerIndexOnTeam,
                SpawnId = planSquads[i].CompanyId,
                Spawn = InvertPosition(planSquads[i].SpawnPosition),
                Lookat = planSquads[i].LookatPosition is GamePosition look ? InvertPosition(look) : null
            };

        }

        // Return
        return squads;

    }

    protected static SessionPlanGoalInfo[] CreatePlanningGoals(IDictionary<ulong, SessionParticipant> participants, ILobbyPlanElement[] planElements, int height) {

        // Grab all goals
        var planGoals = planElements.Filter(x => x.ObjectiveType is not PlanningObjectiveType.None);

        // Create goals array
        var goals = new SessionPlanGoalInfo[planGoals.Length];

        // Loop over all goals
        for (int i = 0; i < planGoals.Length; i++) {

            // get participant
            var p = participants[planGoals[i].ElementOwnerId];

            // Create goal
            goals[i] = new() {
                ObjectiveTeam = (byte)p.TeamIndex,
                ObjectivePlayer = p.PlayerIndexOnTeam,
                ObjectiveType = (byte)planGoals[i].ObjectiveType,
                ObjectiveIndex = (byte)planGoals[i].ObjectiveOrder,
                ObjectivePosition = InvertPosition(planGoals[i].SpawnPosition)
            };

        }

        // Return goals
        return goals;

    }

    protected void CreateAIPlans(IScenario scenario, SessionParticipant[] defenders, byte tid,
        ref SessionPlanSquadInfo[] units, ref SessionPlanEntityInfo[] structures, SessionPlanGoalInfo[] goals, Gamemode gamemode, IModPackage package) {

        // Grab allies AIs
        var aiDefenders = defenders.Filter(x => !x.IsHuman);

        // Ensure there's AI
        if (aiDefenders.Length is 0) {
            return;
        }

        // Notify user the AI defence is being generated.
        this.ShowStartupInformation("Generating AI-Defence Plan");

        // Try grab analysis
        var analysis = AIDatabase.GetMapAnalysis(scenario.RelativeFilename);

        // Create planner
        var aiplan = new AIDefencePlanner(analysis, gamemode);
        aiplan.SetDefenceGoals(goals.Filter(x => x.ObjectiveType is 1).Map(x => x.ObjectivePosition));
        aiplan.SetHumanDefences(
            units.Map(x => (x.Spawn, x.Lookat)),
            structures.Map(x => (x.Blueprint, x.Spawn, x.Lookat))
        );
        aiplan.Subdivide(scenario, tid, aiDefenders.Map(x => x.PlayerIndexOnTeam));

        // Loop over defending ai players and place defences
        for (byte i = 0; i < aiDefenders.Length; i++) {

            // Grab company
            var company = aiDefenders[i].SelectedCompany ?? throw new Exception("Failed to generate defence plan for unknown company.");

            // Create plan for defender
            aiplan.CreateDefencePlan(tid, i, company, scenario);

        }

        // Extract squads
        units = units.Concat(aiplan.GetSquads().Map(x => {
            return new SessionPlanSquadInfo() {
                TeamOwner = tid,
                TeamMemberOwner = x.AIIndex + 1,
                SpawnId = x.PlanElement.CompanyId,
                Spawn = x.PlanElement.SpawnPosition,
                Lookat = x.PlanElement.LookatPosition
            };
        }));

        // Extract entities
        structures = structures.Concat(aiplan.GetEntities().Map(x => {
            return new SessionPlanEntityInfo() {
                TeamOwner = tid,
                TeamMemberOwner = x.AIIndex + 1,
                Blueprint = package.GetDataSource().GetBlueprints(GameCase.CompanyOfHeroes2).FromBlueprintName<EntityBlueprint>(x.PlanElement.Blueprint),
                Spawn = x.PlanElement.SpawnPosition,
                Lookat = x.PlanElement.LookatPosition,
                IsDirectional = x.PlanElement.IsDirectional,
                Width = gamemode.GetPlanningEntity(x.PlanElement.Blueprint).Width
            };
        }));

    }

    /// <summary>
    /// Handles the event when the game startup is cancelled.
    /// </summary>
    /// <param name="sender">The startup strategy that triggered the event.</param>
    /// <param name="caller">The object that invoked the method.</param>
    /// <param name="reason">The reason for cancellation.</param>
    protected void HandleStartupCancel(IStartupStrategy sender, object? caller, string reason) {
        this.m_chat.SystemMessage(reason, Colors.Red);
        this.m_handle.NotifyError("startup", reason);
        this.m_prepCancelHandler?.Invoke(this);
    }

    /// <summary>
    /// Handles the event when the game startup is providing an information.
    /// </summary>
    /// <param name="sender">The startup strategy that triggered the event.</param>
    /// <param name="caller">The object that invoked the method.</param>
    /// <param name="message">The information message.</param>
    protected void HandleStartupInformation(IStartupStrategy sender, object? caller, string message)
        => this.ShowStartupInformation(message);

    private void ShowStartupInformation(string message) {
        this.m_chat.SystemMessage(message, Colors.DarkGray);
        this.m_handle.NotifyMatch("startup", message);
    }

    /// <summary>
    /// Handles the serialization event of a company.
    /// </summary>
    /// <param name="company">The company that has been serialized.</param>
    protected virtual void OnCompanySerialized(Company company) {

        // Run through a sanitizer
        try {

            // Save the company
            Companies.SaveCompany(company);

        } catch (ChecksumViolationException checksumViolation) {

            // Log checksum violation
            logger.Warning(checksumViolation);

        } catch (OperationCanceledException cancelledException) {

            // Log checksum violation
            logger.Warning(cancelledException);

        }

    }

}
