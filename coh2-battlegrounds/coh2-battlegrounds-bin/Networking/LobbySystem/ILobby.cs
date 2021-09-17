using Battlegrounds.Game;
using Battlegrounds.Networking.ChatSystem;
using Battlegrounds.Networking.LobbySystem.Playing;

namespace Battlegrounds.Networking.LobbySystem {

    /// <summary>
    /// 
    /// </summary>
    public interface ILobby : INetworkObjectObservable<ILobby> {

        /// <summary>
        /// 
        /// </summary>
        bool AllowSpectators { get; }

        /// <summary>
        /// 
        /// </summary>
        string Name { get; }

        /// <summary>
        /// 
        /// </summary>
        int Humans { get; }

        /// <summary>
        /// 
        /// </summary>
        int Capacity { get; }

        /// <summary>
        /// 
        /// </summary>
        int Members { get; }

        /// <summary>
        /// 
        /// </summary>
        ILobbyTeam Allies { get; }

        /// <summary>
        /// 
        /// </summary>
        ILobbyTeam Axis { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        IChatRoom GetChatRoom();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        ILobbyMatchContext CreateMatchContext();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        ILobbyParticipant[] GetParticipants();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="participantID"></param>
        /// <returns></returns>
        ILobbyParticipant GetParticipant(ulong participantID);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="participantID"></param>
        /// <param name="participantName"></param>
        /// <returns></returns>
        ILobbyParticipant CreateParticipant(ulong participantID, string participantName);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="difficulty"></param>
        /// <param name="teamSlot"></param>
        /// <returns></returns>
        ILobbyAIParticipant CreateAIParticipant(AIDifficulty difficulty, ILobbyTeamSlot teamSlot, ILobbyCompany aiCompany);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="participant"></param>
        /// <param name="kick"></param>
        /// <returns></returns>
        bool RemoveParticipant(ILobbyParticipant participant, bool kick);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="participant"></param>
        /// <returns></returns>
        bool IsObserver(ILobbyParticipant participant);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="participant"></param>
        /// <returns></returns>
        ILobbyTeam GetTeam(ILobbyParticipant participant);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="lobbySetting"></param>
        /// <param name="lobbySettingValue"></param>
        /// <returns></returns>
        bool SetLobbySetting(string lobbySetting, string lobbySettingValue);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="lobbySetting"></param>
        /// <returns></returns>
        string GetLobbySetting(string lobbySetting);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fromSlot"></param>
        /// <param name="toSlot"></param>
        void SwapSlots(ILobbyTeamSlot fromSlot, ILobbyTeamSlot toSlot);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="maxTeamOccupants"></param>
        /// <returns></returns>
        bool SetTeamsCapacity(int maxTeamOccupants);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="allowed"></param>
        /// <returns></returns>
        bool SetSpectatorsAllowed(bool allowed);

    }

}
