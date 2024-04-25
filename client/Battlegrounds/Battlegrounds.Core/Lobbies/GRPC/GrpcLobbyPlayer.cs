using Battlegrounds.Grpc;

namespace Battlegrounds.Core.Lobbies.GRPC;

public sealed class GrpcLobbyPlayer : ILobbyPlayer {

    private string _name = string.Empty;
    private string _faction = "none";
    private string _company = "none";
    private ulong _id = 0;

    public string Name => _name;

    public string Faction => _faction;

    public string CompanyName => _company;

    public ulong PlayerId => _id;

    public GrpcLobbyPlayer? FromProto(LobbySlot lobbySlot) {
        _name = lobbySlot.HasUserDisplayName ? lobbySlot.UserDisplayName : "";
        _faction = lobbySlot.UserCompany is not null ? lobbySlot.UserCompany.CompanyFactionId : "none";
        _company = lobbySlot.UserCompany is not null ? lobbySlot.UserCompany.CompanyName : "none";
        _id = lobbySlot.HasUserId ? lobbySlot.UserId : 0;
        return this;
    }

}
