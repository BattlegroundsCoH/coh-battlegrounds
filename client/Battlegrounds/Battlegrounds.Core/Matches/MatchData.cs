using Battlegrounds.Core.Companies;
using Battlegrounds.Core.Games;
using Battlegrounds.Core.Games.Gamemodes;
using Battlegrounds.Core.Games.Scenarios;

namespace Battlegrounds.Core.Matches;

public record MatchPlayer(int TeamIndex, ulong PlayerId, ICompany Company, string Name, AIDifficulty Difficulty);

public record MatchData(IGamemode Gamemode, IScenario Scenario, MatchPlayer[] Team1, MatchPlayer[] Team2, IDictionary<string, string> Settings);
