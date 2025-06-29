using Battlegrounds.Models;
using Battlegrounds.Models.Lobbies;
using Battlegrounds.Models.Playing;
using Battlegrounds.Services;

using Microsoft.Extensions.Logging;

namespace Battlegrounds.Factories;

public readonly struct LobbySetup {
    public string Name { get; init; }
    public Participant Self { get; init; }
    public Team Team1 { get; init; }
    public Team Team2 { get; init; }
    public List<LobbySetting> Settings { get; init; }
    public Game Game { get; init; }
    public Map Map { get; init; }
    public HashSet<Participant> Participants { get; init; }
}

public sealed class LobbySetupFromConfigFactory(Configuration configuration, IGameMapService mapService, ILogger<LobbySetupFromConfigFactory> logger) {

    private readonly Configuration _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration), "Configuration cannot be null.");
    private readonly IGameMapService _mapService = mapService ?? throw new ArgumentNullException(nameof(mapService), "Game map service cannot be null.");
    private readonly ILogger<LobbySetupFromConfigFactory> _logger = logger ?? throw new ArgumentNullException(nameof(logger), "Logger cannot be null.");

    public ValueTask<LobbySetup> FromConfig(string name, Game game, User host) => FromConfig(name, game, host, _configuration);

    public async ValueTask<LobbySetup> FromConfig(string name, Game game, User host, Configuration configuration) {
        Participant self = new Participant(0, host.UserId, host.UserDisplayName, false, true);
        HashSet<Participant> participants = [self];
        var lobbySetup = new LobbySetup {
            Name = name,
            Self = self,
            Map = await _mapService.GetLatestMapAsync(game.Id) ?? throw new InvalidOperationException("No map found for the specified game."),
            Settings = GetSettings(game, configuration),
            Game = game,
            Team1 = GetLatestTeamSetup(1, game, configuration, self, participants),
            Team2 = GetLatestTeamSetup(2, game, configuration, self, participants)
        };
        _logger.LogInformation("Created lobby setup from config: {@LobbySetup}", lobbySetup);
        return lobbySetup;
    }

    private static List<LobbySetting> GetSettings(Game game, Configuration cfg) {
        List<LobbySetting> settings = [
            new LobbySetting { Name = LobbySetting.SETTING_GAMEMODE, Type = LobbySettingType.Selection, Options = [
                new ("Domination", "domination"),
                new ("Victory Points", "victory_points")]
            },
            // TODO: More settings
        ];
        var gameSettings = cfg.GetLatestLobbySettings(game.Id);
        foreach (var setting in gameSettings) {
            if (settings.FirstOrDefault(s => s.Name == setting.Key) is LobbySetting existing) {
                existing.Value = setting.Value;
            }
        }
        return settings;
    }

    private static Team GetLatestTeamSetup(int idx, Game game, Configuration cfg, Participant self, HashSet<Participant> participants) {
        var tt = cfg.GetTeamType(idx, game.Id);
        Team.Slot[] slots = new Team.Slot[4];
        for (int i = 0; i < 4; i++) {
            var cfgSlot = cfg.GetTeamSlot(idx, i, game.Id);
            string participantId = cfgSlot.IsLocal ? self.ParticipantId : string.Empty;
            if (!cfgSlot.IsLocal && !string.IsNullOrEmpty(cfgSlot.Faction)) {
                int id = idx * 4 + i;
                Participant aiParticipant = new Participant(id, id.ToString(), string.Empty, true, true);
                participants.Add(aiParticipant);
                participantId = aiParticipant.ParticipantId;
            }
            slots[i] = new Team.Slot(i, participantId, cfgSlot.Faction, cfgSlot.CompanyId, cfgSlot.Difficulty, cfgSlot.IsHidden, cfgSlot.IsLocked);
        }
        return new Team(tt, tt switch {
            TeamType.Allies => "Allies",
            TeamType.Axis => "Axis",
            _ => throw new ArgumentOutOfRangeException(nameof(idx), $"Unknown team type: {tt}")
        }, slots);
    }

}
