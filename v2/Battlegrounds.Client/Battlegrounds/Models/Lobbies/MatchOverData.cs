using Battlegrounds.Models.Blueprints;
using Battlegrounds.Models.Companies;
using Battlegrounds.Models.Replays;

namespace Battlegrounds.Models.Lobbies;

/// <summary>
/// Represents a per-squad performance summary derived from match modifiers.
/// </summary>
/// <param name="SquadId">The squad identifier.</param>
/// <param name="InfantryKilled">Total infantry killed by this squad.</param>
/// <param name="VehiclesDestroyed">Total vehicles destroyed by this squad.</param>
/// <param name="Losses">Number of losses this squad suffered.</param>
/// <param name="ExperienceGained">Total experience gained by this squad.</param>
/// <param name="WasKilled">Whether this squad was killed during the match.</param>
/// <param name="PickedUpBlueprint">Optional blueprint picked up by this squad.</param>
public sealed record SquadMatchSummary(
    int SquadId,
    SquadBlueprint? Blueprint,
    int InfantryKilled,
    int VehiclesDestroyed,
    int Losses,
    float ExperienceGained,
    bool WasKilled,
    string? PickedUpBlueprint);

/// <summary>
/// Represents a player-centric summary of a match result, suitable for display on a post-match screen.
/// </summary>
/// <remarks>
/// This class is a flattened projection of <see cref="MatchResult"/> for a specific player. Use
/// <see cref="FromMatchResultForPlayer"/> to construct an instance from a raw match result.
/// </remarks>
public sealed class MatchOverData {

    /// <summary>Gets the match identifier.</summary>
    public string MatchId { get; init; } = string.Empty;

    /// <summary>Gets the game identifier.</summary>
    public string GameId { get; init; } = string.Empty;

    /// <summary>Gets the scenario (map) played.</summary>
    public string Scenario { get; init; } = string.Empty;

    /// <summary>Gets the total duration of the match.</summary>
    public TimeSpan MatchDuration { get; init; } = TimeSpan.Zero;

    /// <summary>Gets whether the match concluded naturally, as opposed to being abandoned.</summary>
    public bool Concluded { get; init; } = false;

    /// <summary>Gets whether this player won the match.</summary>
    public bool IsVictory { get; init; } = false;

    /// <summary>Gets the ID of the company this player used.</summary>
    public string CompanyId { get; init; } = string.Empty;

    /// <summary>Gets whether the match result is considered valid.</summary>
    public bool IsValid { get; init; } = true;

    /// <summary>Gets whether any bad events were recorded during the match.</summary>
    public bool HasBadEvents { get; init; } = false;

    /// <summary>Gets the per-squad performance summaries for this player.</summary>
    public IReadOnlyList<SquadMatchSummary> SquadSummaries { get; init; } = [];

    /// <summary>
    /// Creates a <see cref="MatchOverData"/> instance tailored for a specific player from a raw <see cref="MatchResult"/>.
    /// </summary>
    /// <param name="result">The match result to project from.</param>
    /// <param name="playerId">The ID of the player to project the result for.</param>
    /// <param name="matchCompanies">A dictionary of company data for the match, used to provide context for the player's performance.</param>
    /// <returns>A populated <see cref="MatchOverData"/>, or an invalid instance if the result or player ID is invalid.</returns>
    public static MatchOverData FromMatchResultForPlayer(MatchResult result, string playerId, Dictionary<string, Company>? matchCompanies = null) {
        if (!result.IsValid || string.IsNullOrEmpty(playerId))
            return new MatchOverData { IsValid = false };

        result.PlayerCompanies.TryGetValue(playerId, out var companyId);
        result.CompanyModifiers.TryGetValue(playerId, out var modifiers);

        Company? playerCompany = matchCompanies is not null && matchCompanies.TryGetValue(companyId ?? string.Empty, out var comp) ? comp : null;

        var squadData = new Dictionary<int, (int kills, int vehicles, int losses, float xp, bool killed, string? pickup)>();

        foreach (var mod in modifiers ?? []) {
            if (!squadData.ContainsKey(mod.SquadId))
                squadData[mod.SquadId] = (0, 0, 0, 0f, false, null);

            var current = squadData[mod.SquadId];
            squadData[mod.SquadId] = mod.EventType switch {
                CompanyEventModifier.EVENT_TYPE_STATISTICS => current with {
                    kills = current.kills + mod.IntValue1,
                    vehicles = current.vehicles + mod.IntValue2,
                    losses = current.losses + mod.IntValue3
                },
                CompanyEventModifier.EVENT_TYPE_EXPERIENCE_GAIN => current with {
                    xp = current.xp + mod.FloatValue
                },
                CompanyEventModifier.EVENT_TYPE_KILL_SQUAD => current with { killed = true },
                CompanyEventModifier.EVENT_TYPE_PICKUP => current with { pickup = mod.BlueprintArg },
                _ => current
            };
        }

        var summaries = squadData
            .Select(kv => CreateMatchSummary(playerCompany, kv.Key, kv.Value.kills, kv.Value.vehicles, kv.Value.losses, kv.Value.xp, kv.Value.killed, kv.Value.pickup))
            .ToList();

        return new MatchOverData {
            MatchId = result.MatchId,
            GameId = result.GameId,
            Scenario = result.Scenario,
            MatchDuration = result.MatchDuration,
            Concluded = result.Concluded,
            IsVictory = result.Winners.Contains(playerId),
            CompanyId = companyId ?? string.Empty,
            IsValid = result.IsValid,
            HasBadEvents = result.BadEvents.Count > 0,
            SquadSummaries = summaries
        };
    }

    private static SquadMatchSummary CreateMatchSummary(Company? company, int squadId, int kills, int vehicles, int losses, float xp, bool killed, string? pickup) {
        Squad? squad = company is not null
            ? company.Squads.FirstOrDefault(x => x.Id == squadId) ?? throw new InvalidOperationException($"Squad with ID {squadId} not found in company {company.Id}")
            : null;
        return new SquadMatchSummary(
            SquadId: squadId,
            Blueprint: squad?.Blueprint,
            InfantryKilled: kills,
            VehiclesDestroyed: vehicles,
            Losses: losses,
            ExperienceGained: killed ? 0 : (squad is not null ? xp - squad.Experience : xp),
            WasKilled: killed,
            PickedUpBlueprint: killed ? string.Empty : pickup
        );
    }

}
