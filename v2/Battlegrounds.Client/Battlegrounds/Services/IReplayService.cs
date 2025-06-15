using Battlegrounds.Models.Playing;
using Battlegrounds.Models.Replays;

namespace Battlegrounds.Services;

public interface IReplayService {
    
    Task<ReplayAnalysisResult> AnalyseReplay(string replayLocation, string gameId);    

    Task<ReplayAnalysisResult> AnalyseReplay<T>(string replayLocation) where T : Game;

}
