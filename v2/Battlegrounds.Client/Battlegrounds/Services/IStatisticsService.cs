using Battlegrounds.Models.Statistics;

namespace Battlegrounds.Services;

public interface IStatisticsService {

    Task IsLoaded { get; }

    Task LoadStatisticsAsync();

    Task RegisterPlayedMatchAsync(MatchPlayed match);

    IReadOnlyList<MatchPlayed> GetPlayedMatches();

}
