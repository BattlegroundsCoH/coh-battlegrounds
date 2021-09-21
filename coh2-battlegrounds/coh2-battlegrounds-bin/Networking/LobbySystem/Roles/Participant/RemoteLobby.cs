using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Battlegrounds.Game;
using Battlegrounds.Networking.ChatSystem;
using Battlegrounds.Networking.LobbySystem.Playing;
using Battlegrounds.Networking.Remoting.Objects;

namespace Battlegrounds.Networking.LobbySystem.Roles.Participant {

    public class RemoteLobby : ILobby {

        public bool AllowSpectators => throw new NotImplementedException();

        public string Name => throw new NotImplementedException();

        public int Humans => throw new NotImplementedException();

        public int Capacity => throw new NotImplementedException();

        public int Members => throw new NotImplementedException();

        public ILobbyTeam Allies => throw new NotImplementedException();

        public ILobbyTeam Axis => throw new NotImplementedException();

        public IObjectID ObjectID { get; }

        public event ObservableValueChangedHandler<ILobby> ValueChanged;
        public event ObservableMethodInvokedHandler<ILobby> MethodInvoked;

        public RemoteLobby(IObjectID objectID) {
            this.ObjectID = objectID;
        }

        public ILobbyAIParticipant CreateAIParticipant(AIDifficulty difficulty, ILobbyTeamSlot teamSlot, ILobbyCompany aiCompany) => throw new NotImplementedException();
        
        public ILobbyMatchContext CreateMatchContext() => throw new NotImplementedException();
        
        public ILobbyParticipant CreateParticipant(ulong participantID, string participantName) => throw new NotImplementedException();
        
        public IChatRoom GetChatRoom() => throw new NotImplementedException();
        
        public string GetLobbySetting(string lobbySetting) => throw new NotImplementedException();
        
        public ILobbyParticipant GetParticipant(ulong participantID) => throw new NotImplementedException();
        
        public ILobbyParticipant[] GetParticipants() => throw new NotImplementedException();
        
        public ILobbyTeam GetTeam(ILobbyParticipant participant) => throw new NotImplementedException();
        
        public bool IsObserver(ILobbyParticipant participant) => throw new NotImplementedException();
        
        public bool RemoveParticipant(ILobbyParticipant participant, bool kick) => throw new NotImplementedException();
        
        public bool SetLobbySetting(string lobbySetting, string lobbySettingValue) => throw new NotImplementedException();
        
        public bool SetSpectatorsAllowed(bool allowed) => throw new NotImplementedException();
        
        public bool SetTeamsCapacity(int maxTeamOccupants) => throw new NotImplementedException();

        public void SwapSlots(ILobbyTeamSlot fromSlot, ILobbyTeamSlot toSlot) => throw new NotImplementedException();

    }

}
