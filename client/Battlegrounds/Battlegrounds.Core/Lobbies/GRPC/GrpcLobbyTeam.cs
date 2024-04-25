using Battlegrounds.Grpc;

namespace Battlegrounds.Core.Lobbies.GRPC;

public sealed class GrpcLobbyTeam : ILobbyTeam {

    private string _name = "team1";
    private string _alliance = "allies";
    private GrpcLobbySlot[] _slots = [
        new GrpcLobbySlot(),
        new GrpcLobbySlot(),
        new GrpcLobbySlot(),
        new GrpcLobbySlot()
    ];

    public string Name => _name;

    public string Alliance => _alliance;

    public ILobbySlot[] Slots => _slots;

    public void FromProto(LobbyTeam team) {
        _name = team.Name;
        _alliance = team.Alliance;
        _slots[0].FromProto(team.Slots[0]);
        _slots[1].FromProto(team.Slots[1]);
        _slots[2].FromProto(team.Slots[2]);
        _slots[3].FromProto(team.Slots[3]);
    }

}
