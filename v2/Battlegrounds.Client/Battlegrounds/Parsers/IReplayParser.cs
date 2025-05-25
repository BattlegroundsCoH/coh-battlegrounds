using Battlegrounds.Models.Replays;

namespace Battlegrounds.Parsers;

public interface IReplayParser {

    Replay ParseReplayFile(string replayLocation);

}
