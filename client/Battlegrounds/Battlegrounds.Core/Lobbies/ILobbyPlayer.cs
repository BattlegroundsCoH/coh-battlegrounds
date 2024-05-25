namespace Battlegrounds.Core.Lobbies;

public interface ILobbyPlayer {

    string Name { get; }

    string Faction { get; }

    string CompanyName { get; }

    string CompanyId { get; }

    ulong PlayerId { get; }

}
