namespace Battlegrounds.Core.Games.Factions;

public interface IFaction {

    byte FactionIndex { get; }

    string Name { get; }

    string Alliance { get; }

    string GameId { get; }

    string ScarReferenceId { get; }

}
