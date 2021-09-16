using Battlegrounds.Game;
using Battlegrounds.Networking.ChatSystem;
using Battlegrounds.Networking.LobbySystem.Playing;

namespace Battlegrounds.Networking.LobbySystem {

    public interface ILobby {

        bool AllowSpectators { get; }

        string Name { get; }

        int Humans { get; }

        int Capacity { get; }

        int Members { get; }

        ILobbyTeam Allies { get; }

        ILobbyTeam Axis { get; }

        IChatRoom GetChatRoom();

        ILobbyMatchContext CreateMatchContext();

        ILobbyParticipant[] GetParticipants();

        ILobbyParticipant GetParticipant(ulong participantID);

        ILobbyParticipant CreateParticipant(ulong participantID, string participantName);

        ILobbyAIParticipant CreateAIParticipant(AIDifficulty difficulty, ILobbyTeamSlot teamSlot);

        bool RemoveParticipant(ILobbyParticipant participant, bool kick);

        bool IsObserver(ILobbyParticipant participant);

        ILobbyTeam GetTeam(ILobbyParticipant participant);

        bool SetLobbySetting(string lobbySetting, string lobbySettingValue);

        string GetLobbySetting(string lobbySetting);

        void SwapSlots(ILobbyTeamSlot fromSlot, ILobbyTeamSlot toSlot);

        void SetTeamsCapacity(int maxTeamOccupants);

        bool SetSpectatorsAllowed(bool allowed);

    }

}
