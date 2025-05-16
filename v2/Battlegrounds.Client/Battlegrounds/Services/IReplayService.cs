using Battlegrounds.Models.Replays;

namespace Battlegrounds.Services;

public interface IReplayService {
    
    Task<ReplayAnalysisResult> AnalyseReplay(string replayLocation);

}
