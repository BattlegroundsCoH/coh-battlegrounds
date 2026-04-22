using Battlegrounds.Models.Playing;
using Battlegrounds.Models.Replays;
using Battlegrounds.Parsers;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Battlegrounds.Services.Playing;

/// <summary>
/// Provides game replay file analysis services with support for multiple game types.
/// </summary>
/// <param name="serviceProvider">Service provider used to resolve game-specific replay parsers.</param>
/// <param name="logger">Logger for diagnostic messages and error tracking.</param>
public sealed class ReplayService(IServiceProvider serviceProvider, ILogger<ReplayService> logger) : IReplayService {

    private readonly ILogger<ReplayService> _logger = logger;

    public async Task<ReplayAnalysisResult> AnalyseReplay(string replayLocation, string gameId) {
        try {
            var replay = gameId switch {
                CoH3.GameId => await Task.FromResult(ParseCoH3ReplayFile(replayLocation)),
                _ => throw new NotImplementedException($"Replay analysis for {gameId} is not implemented.")
            };
            return new ReplayAnalysisResult {
                Replay = replay,
                GameId = gameId
            };
        } catch (Exception ex) {
            _logger.LogError(ex, "Failed to analyse replay file at {ReplayLocation} for game {GameId}", replayLocation, gameId);
            return new ReplayAnalysisResult {
                Failed = true,
                GameId = gameId
            };
        }
    }

    public Task<ReplayAnalysisResult> AnalyseReplay<T>(string replayLocation) where T : Game {
        if (typeof(T) == typeof(CoH3))
            return AnalyseReplay(replayLocation, CoH3.GameId);
        throw new NotImplementedException($"Replay analysis for {typeof(T).Name} is not implemented.");
    }

    private Replay ParseCoH3ReplayFile(string replayLocation) {
        CoH3ReplayParser parser = serviceProvider.GetRequiredService<CoH3ReplayParser>();
        return parser.ParseReplayFile(replayLocation);
    }

}
