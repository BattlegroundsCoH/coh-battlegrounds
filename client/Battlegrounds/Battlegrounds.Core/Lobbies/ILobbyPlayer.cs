namespace Battlegrounds.Core.Lobbies;

public interface ILobbyPlayer {

    string Name { get; }

    string Faction { get; }

    string CompanyName { get; }

    ulong PlayerId { get; }

}
