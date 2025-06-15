using Battlegrounds.Models.Companies;
using Battlegrounds.Models.Lobbies;

using Serilog;

namespace Battlegrounds.Models.Replays;

public sealed class ReplayAnalysisResult {

    private static readonly ILogger _logger = Log.ForContext<ReplayAnalysisResult>();

    public bool Failed { get; init; }

    public string GameId { get; init; } = string.Empty;

    public Replay? Replay { get; init; } = null;

    public MatchResult GetMatchResult(ILobby lobby) {

        if (Failed || Replay is null) {
            return MatchResult.Unknown;
        }

        Dictionary<int, LinkedList<ReplayEvent>> companyChanges = [];
        Dictionary<int, MatchPlayerOverEvent> playerOverEvents = [];
        Dictionary<int, HashSet<ushort>> deployedSquads = [];
        foreach (var player in Replay.Players) {
            companyChanges[player.PlayerId] = [];
            deployedSquads[player.PlayerId] = [];
        }

        List<BadMatchEvent> badEvents = [];

        MatchStartReplayEvent? registeredStartEvent = null;
        MatchOverReplayEvent? registeredOverEvent = null;
        foreach (var replayEvent in Replay.Events.OrderBy(x => x.Timestamp)) {
            if (replayEvent is MatchStartReplayEvent startEvent) {
                registeredStartEvent ??= startEvent;
                continue;
            } else if (replayEvent is MatchOverReplayEvent overEvent) {
                registeredOverEvent ??= overEvent;
                continue;
            }
            if (replayEvent.Player is null) {
                _logger.Warning("{Event} without player at {Timestamp}", replayEvent.GetType().Name, replayEvent.Timestamp);
                badEvents.Add(new BadMatchEvent(replayEvent, $"{replayEvent.GetType().Name} without player"));
                continue;
            }
            int pid = replayEvent.Player.PlayerId;
            switch (replayEvent) {
                case SquadDeployedEvent deployedEvent: {
                    if (!deployedSquads[pid].Add(deployedEvent.SquadCompanyId)) {
                        _logger.Warning("Squad {SquadCompanyId} deployed multiple times by player {PlayerId} at {Timestamp}", deployedEvent.SquadCompanyId, pid, deployedEvent.Timestamp);
                        badEvents.Add(new BadMatchEvent(deployedEvent, $"Squad {deployedEvent.SquadCompanyId} deployed multiple times"));
                    }
                    companyChanges[pid].AddLast(deployedEvent);
                    break;
                }
                case SquadKilledEvent killedEvent: {
                    if (!deployedSquads[pid].Remove(killedEvent.SquadCompanyId)) {
                        _logger.Warning("Squad {SquadCompanyId} killed without being deployed by player {PlayerId} at {Timestamp}", killedEvent.SquadCompanyId, pid, killedEvent.Timestamp);
                        badEvents.Add(new BadMatchEvent(killedEvent, $"Squad {killedEvent.SquadCompanyId} killed without being deployed"));
                    }
                    companyChanges[pid].AddLast(killedEvent);
                    break;
                }
                case SquadWeaponPickupEvent pickupEvent: {
                    if (!deployedSquads[pid].Contains(pickupEvent.SquadCompanyId)) {
                        _logger.Warning("Weapon pickup for undeployed squad {SquadCompanyId} by player {PlayerId} at {Timestamp}", pickupEvent.SquadCompanyId, pid, pickupEvent.Timestamp);
                        badEvents.Add(new BadMatchEvent(pickupEvent, $"Weapon pickup for undeployed squad {pickupEvent.SquadCompanyId}"));
                    }
                    companyChanges[pid].AddLast(pickupEvent);
                    break;
                }
                case SquadRecalledEvent recalledEvent: {
                    if (!deployedSquads[pid].Remove(recalledEvent.SquadCompanyId)) {
                        _logger.Warning("Squad {SquadCompanyId} recalled without being deployed by player {PlayerId} at {Timestamp}", recalledEvent.SquadCompanyId, pid, recalledEvent.Timestamp);
                        badEvents.Add(new BadMatchEvent(recalledEvent, $"Squad {recalledEvent.SquadCompanyId} recalled without being deployed"));
                    }
                    companyChanges[pid].AddLast(recalledEvent);
                    break;
                }
                case MatchPlayerOverEvent playerOverEvent: {
                    if (playerOverEvent.Player is null) {
                        _logger.Warning("{Event} without player at {Timestamp}", nameof(MatchPlayerOverEvent), playerOverEvent.Timestamp);
                        badEvents.Add(new BadMatchEvent(playerOverEvent, $"{nameof(MatchPlayerOverEvent)} without player"));
                        continue;
                    }
                    playerOverEvents[pid] = playerOverEvent;
                    break;
                }
                default:
                    _logger.Warning("Unhandled replay event type: {EventType}", replayEvent.GetType().Name);
                    break;
            }
        }

        if (registeredStartEvent is null) {
            _logger.Warning("No match start event found in replay for game {GameId}", GameId);
            return MatchResult.Invalid;
        }

        // Now we map ingame player IDs to participant IDs
        Dictionary<int, string> playerIdToParticipantId = [];
        Dictionary<string, string> playerCompanies = [];
        HashSet<string> winners = [];
        HashSet<string> losers = [];
        foreach (var playerData in registeredStartEvent.Players) {
            Participant? participant = null;
            foreach (var p in lobby.Participants) {
                if (p.LobbyId == playerData.ModId) {
                    participant = p;
                    break;
                }
            }
            if (participant is null) {
                _logger.Warning("No participant found for player {PlayerId} in lobby {LobbyName}", playerData.PlayerId, lobby.Name);
                badEvents.Add(new BadMatchEvent(registeredStartEvent, $"No participant found for player {playerData.PlayerId}"));
                continue;
            }
            if (registeredOverEvent is not null) {
                if (registeredOverEvent.Winners.Contains(playerData.PlayerId)) {
                    winners.Add(participant.ParticipantId); // Ignore duplicate winners
                } else if (registeredOverEvent.Losers.Contains(playerData.PlayerId)) {
                    losers.Add(participant.ParticipantId); // Ignore duplicate losers
                } else {
                    _logger.Warning("Player {PlayerId} not found in winners or losers in match over event", playerData.PlayerId);
                    badEvents.Add(new BadMatchEvent(registeredOverEvent, $"Player {playerData.PlayerId} not found in winners or losers"));
                }
            }
            playerIdToParticipantId[playerData.PlayerId] = participant.ParticipantId;
            playerCompanies[participant.ParticipantId] = playerData.CompanyId; // Map participant ID to company ID
        }

        // Transform company changes to match the participant IDs and filter companies not mapped to a participant
        var participantCompanyChanges =
            (from kvp in companyChanges
            where playerIdToParticipantId.ContainsKey(kvp.Key)
            select new KeyValuePair<string, LinkedList<CompanyEventModifier>>(
                playerIdToParticipantId[kvp.Key],
                ConvertToModifiers(kvp.Value)
            )).ToDictionary();

        if (participantCompanyChanges.Count != playerIdToParticipantId.Count) {
            _logger.Warning("Not all players have company changes mapped to them. Expected {Expected}, got {Got}", playerIdToParticipantId.Count, participantCompanyChanges.Count);
            badEvents.Add(new BadMatchEvent(registeredStartEvent, "Not all players have company changes mapped to them"));
        }

        return new MatchResult() {
            GameId = GameId,
            MatchId = registeredStartEvent.MatchId,
            ModVersion = registeredStartEvent.ModVersion,
            Scenario = registeredStartEvent.Scenario,
            MatchDuration = Replay.Duration,
            CompanyModifiers = participantCompanyChanges,
            PlayerCompanies = playerCompanies,
            BadEvents = badEvents,
            Concluded = registeredOverEvent is not null,
            Winners = winners,
            Losers = losers,
            IsValid = badEvents.Count == 0 && !string.IsNullOrEmpty(registeredStartEvent.MatchId) && !string.IsNullOrEmpty(registeredStartEvent.ModVersion),
        };

    }

    private static LinkedList<CompanyEventModifier> ConvertToModifiers(LinkedList<ReplayEvent> events) {
        LinkedList<CompanyEventModifier> modifiers = new LinkedList<CompanyEventModifier>();
        foreach (var replayEvent in events) {
            if (replayEvent is SquadDeployedEvent deployedEvent) {
                modifiers.AddLast(CompanyEventModifier.InMatch(deployedEvent.SquadCompanyId));
            } else if (replayEvent is SquadKilledEvent killedEvent) {
                modifiers.AddLast(CompanyEventModifier.Kill(killedEvent.SquadCompanyId));
            } else if (replayEvent is SquadWeaponPickupEvent pickupEvent) {
                modifiers.AddLast(CompanyEventModifier.Pickup(pickupEvent.SquadCompanyId, pickupEvent.IsEntityBlueprint ? pickupEvent.WeaponName : string.Empty)); // Warning, may be a problem for CoH2 which uses slot items
            } else if (replayEvent is SquadRecalledEvent recalledEvent) {
                modifiers.AddLast(CompanyEventModifier.ExperienceGain(recalledEvent.SquadCompanyId, recalledEvent.Experience));
                modifiers.AddLast(CompanyEventModifier.Statistics(recalledEvent.SquadCompanyId, recalledEvent.InfantryKills, recalledEvent.VehicleKills));
            } else if (replayEvent is MatchPlayerOverEvent playerOver) {
                foreach (var squad in playerOver.DeployedUnits) {
                    modifiers.AddLast(CompanyEventModifier.ExperienceGain(squad.SquadCompanyId, squad.Experience));
                    modifiers.AddLast(CompanyEventModifier.Statistics(squad.SquadCompanyId, squad.InfantryKills, squad.VehicleKills));
                }
            } else {
                _logger.Warning("Unhandled replay event type: {ReplayType}", replayEvent.GetType().Name);
            }
        }
        return modifiers;
    }

}
