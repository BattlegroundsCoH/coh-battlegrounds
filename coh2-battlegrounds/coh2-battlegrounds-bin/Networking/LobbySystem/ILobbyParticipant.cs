namespace Battlegrounds.Networking.LobbySystem {

    public interface ILobbyParticipant : INetworkObjectObservable<ILobbyParticipant> {

        ulong Id { get; }

        string Name { get; }

        bool IsSelf { get; }

        ILobbyCompany Company { get; }

        bool SetCompany(ILobbyCompany company);

    }

}
