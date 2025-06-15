using Battlegrounds.Models.Playing;
using Battlegrounds.Models.Replays;
using Battlegrounds.Parsers;

using Microsoft.Extensions.DependencyInjection;

namespace Battlegrounds.Services;

public sealed class ReplayService(IServiceProvider serviceProvider) : IReplayService {

    public async Task<ReplayAnalysisResult> AnalyseReplay(string replayLocation, string gameId) {
        var replay = gameId switch {
            CoH3.GameId => await Task.FromResult(ParseCoH3ReplayFile(replayLocation)),
            _ => throw new NotImplementedException($"Replay analysis for {gameId} is not implemented.")
        };
        // TODO: Compile replay events into actions to apply to companies
        return new ReplayAnalysisResult {
            Replay = replay,
            GameId = gameId
        };
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
