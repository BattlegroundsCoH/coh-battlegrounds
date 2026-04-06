using System.IO;

using Battlegrounds.Models.Companies;
using Battlegrounds.Models.Lobbies;
using Battlegrounds.Models.Matches;
using Battlegrounds.Models.Replays;
using Battlegrounds.Parsers;

namespace Battlegrounds.Models.Playing;

public sealed class SimulatedAppRunParameters {
    public TimeSpan SimulateAppRunTime { get; init; } = TimeSpan.FromSeconds(5);
}

public sealed class SimulatedAppInstance(Game game, bool launchSuccessful, SimulatedAppRunParameters appRunParameters, ILobby lobby) : GameAppInstance {

    private readonly ILobby _lobby = lobby;

    private sealed record SimulatedCoH3ReplayPlayer(Participant Participant, Team.Slot Slot, int TeamId, ReplayPlayer ReplayPlayer, Company? Company);

    public override Game Game => game;

    public override Task<bool> Launch(params string[] args) => Task.FromResult(launchSuccessful);

    public override async Task<MatchPlayResult> WaitForMatch() {

        var delay = Task.Delay(appRunParameters.SimulateAppRunTime);
        var replayPath = GenerateReplay();
        await delay;

        return new MatchPlayResult {
            Failed = false,
            ErrorMessage = string.Empty,
            ScarError = false,
            BugSplat = false,
            ReplayFilePath = replayPath,
        };

    }

    private string GenerateReplay() {
        if (game is CoH3 coh3) {
            return GenerateCoH3Replay(coh3);
        }
        return string.Empty;
    }

    private string GenerateCoH3Replay(CoH3 coh3) {

        string resultPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "my games",
                "Company of Heroes 3",
                "playback",
                $"{DateTime.Now:yyyy-MM-dd_HH-mm-ss}_simulated_replay.rec"
            );

        Directory.CreateDirectory(Path.GetDirectoryName(resultPath) ?? throw new InvalidOperationException("Unable to determine replay output directory."));

        var players = GetSimulatedCoH3ReplayPlayers();
        var events = BuildSimulatedCoH3ReplayEvents(players, coh3);

        var writer = new CoH3ReplayWriter();
        byte[] replayData = writer.WriteDummyReplay([.. players.Select(x => x.ReplayPlayer)], [.. events]);
        File.WriteAllBytes(resultPath, replayData);

        return resultPath;
    
    }

    private List<SimulatedCoH3ReplayPlayer> GetSimulatedCoH3ReplayPlayers() {
        List<SimulatedCoH3ReplayPlayer> players = [];

        AddTeamPlayers(_lobby.Team1, teamId: 0, players);
        AddTeamPlayers(_lobby.Team2, teamId: 1, players);

        return players.OrderBy(x => x.ReplayPlayer.PlayerId).ToList();
    }

    private void AddTeamPlayers(Team team, int teamId, List<SimulatedCoH3ReplayPlayer> players) {
        foreach (var slot in team.Slots.Where(x => !x.Hidden && !x.Locked && !string.IsNullOrEmpty(x.ParticipantId))) {
            var participant = _lobby.Participants.FirstOrDefault(x => x.ParticipantId == slot.ParticipantId);
            if (participant is null) {
                continue;
            }

            _lobby.Companies.TryGetValue(slot.CompanyId, out var company);

            string faction = !string.IsNullOrWhiteSpace(slot.Faction)
                ? slot.Faction
                : company?.Faction ?? string.Empty;

            string aiProfile = participant.IsAIParticipant
                ? $"simulated_{slot.Difficulty.Name.ToLowerInvariant()}"
                : string.Empty;

            var replayPlayer = new ReplayPlayer(
                1000 + participant.LobbyId,
                teamId,
                participant.ParticipantName,
                (ulong)Math.Max(participant.LobbyId, 0),
                0,
                faction,
                aiProfile);

            players.Add(new SimulatedCoH3ReplayPlayer(participant, slot, teamId, replayPlayer, company));
        }
    }

    private List<ReplayEvent> BuildSimulatedCoH3ReplayEvents(List<SimulatedCoH3ReplayPlayer> players, CoH3 coh3) {
        List<ReplayEvent> events = [];

        events.Add(new MatchStartReplayEvent(
            TimeSpan.Zero,
            Guid.CreateVersion7().ToString(),
            GetSimulatedModVersion(coh3),
            _lobby.Map.ScenarioName,
            [.. players.Select(x => new MatchStartReplayEvent.PlayerData(
                x.ReplayPlayer.PlayerId,
                x.Participant.ParticipantName,
                x.Slot.CompanyId,
                x.Participant.LobbyId))]));

        int winningTeamId = GetWinningTeamId(players);
        int tick = 1;

        foreach (var player in players) {
            if (player.Company is null) {
                continue;
            }

            foreach (var squad in player.Company.Squads.OrderBy(x => x.Id)) {
                events.Add(new SquadDeployedEvent(GetTickTimestamp(tick++), player.ReplayPlayer, (ushort)squad.Id));

                if (player.TeamId == winningTeamId) {
                    events.Add(new SquadRecalledEvent(
                        GetTickTimestamp(tick++),
                        player.ReplayPlayer,
                        (ushort)squad.Id,
                        squad.Experience,
                        squad.TotalInfantryKills,
                        squad.TotalVehicleKills,
                        0));
                } else {
                    events.Add(new SquadKilledEvent(GetTickTimestamp(tick++), player.ReplayPlayer, (ushort)squad.Id));
                }
            }
        }

        events.Add(new MatchOverReplayEvent(
            GetTickTimestamp(Math.Max(tick, 1)),
            [.. players.Where(x => x.TeamId == winningTeamId).Select(x => x.ReplayPlayer.PlayerId)],
            [.. players.Where(x => x.TeamId != winningTeamId).Select(x => x.ReplayPlayer.PlayerId)],
            [.. players.Select(x => new MatchOverReplayEvent.PlayerStatistics(
                x.ReplayPlayer.PlayerId,
                x.TeamId,
                x.Participant.ParticipantName,
                x.Participant.LobbyId,
                x.Company?.Squads.Sum(s => s.TotalKills) ?? 0,
                x.TeamId == winningTeamId ? 0 : x.Company?.Squads.Count ?? 0))]));

        return events;
    }

    private int GetWinningTeamId(List<SimulatedCoH3ReplayPlayer> players) {
        string? localPlayerId = _lobby.GetLocalPlayerId();
        return players.FirstOrDefault(x => x.Participant.ParticipantId == localPlayerId)?.TeamId ?? 0;
    }

    private static TimeSpan GetTickTimestamp(int tick) => TimeSpan.FromSeconds(tick * CoH3ReplayParser.COH3_TICK_RATE);

    private static string GetSimulatedModVersion(CoH3 coh3) {
        string modName = Path.GetFileNameWithoutExtension(coh3.ModProjectPath);
        return string.IsNullOrWhiteSpace(modName) ? "simulated" : modName;
    }

}
