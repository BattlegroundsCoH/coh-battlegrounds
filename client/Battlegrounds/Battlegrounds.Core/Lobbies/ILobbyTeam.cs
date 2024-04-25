namespace Battlegrounds.Core.Lobbies;

public interface ILobbyTeam {

    string Name { get; }

    string Alliance { get; }

    ILobbySlot[] Slots { get; }

}
