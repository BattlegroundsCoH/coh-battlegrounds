using System;

using Battlegrounds.Game;
using Battlegrounds.ErrorHandling.Networking;
using Battlegrounds.Networking.ChatSystem;
using Battlegrounds.Networking.LobbySystem.Playing;
using Battlegrounds.Networking.Memory;
using Battlegrounds.Networking.Remoting.Objects;
using Battlegrounds.Networking.Remoting.Query;
using Battlegrounds.Networking.Remoting.Reflection;

namespace Battlegrounds.Networking.LobbySystem.Roles.Participant {

    public class RemoteLobby : ILobby, IRemoteReference {

        private readonly IRemoteHandle m_handle;
        private readonly ValueCache<bool> m_spectatorsAllowed;
        private readonly ObjectCache<string> m_lobbyName;
        private readonly ValueCache<int> m_lobbyCap;
        private readonly ValueCache<int> m_lobbyMembers;

        private ILobbyTeam m_alliedTeam;
        private ILobbyTeam m_axisTeam;

        private CollectionCache<ILobbyParticipant> m_participants;

        public bool AllowSpectators => this.m_spectatorsAllowed.GetCachedValue(() => this.m_handle.GetRemoteProperty<bool, ILobby>(this.ObjectID));

        public string Name => this.m_lobbyName.GetCachedValue(() => this.m_handle.GetRemoteProperty<string, ILobby>(this.ObjectID));

        public int Humans => 0; // It doesn't make much sense for the local machine to access this.

        public int Capacity => this.m_lobbyCap.GetCachedValue(() => this.m_handle.GetRemoteProperty<int, ILobby>(this.ObjectID));

        public int Members => this.m_lobbyMembers.GetCachedValue(() => this.m_handle.GetRemoteProperty<int, ILobby>(this.ObjectID));

        public ILobbyTeam Allies => this.m_alliedTeam;

        public ILobbyTeam Axis => this.m_axisTeam;

        public IObjectID ObjectID { get; }

        public event ObservableValueChangedHandler<ILobby> ValueChanged;
        public event ObservableMethodInvokedHandler<ILobby> MethodInvoked;

        public RemoteLobby(IObjectID objectID, IRemoteHandle remoteHandle) {

            // Set base values
            this.ObjectID = objectID;
            this.m_handle = remoteHandle;

            // Create cache values
            this.m_spectatorsAllowed = new(TimeSpan.FromMinutes(1));
            this.m_lobbyName = new(TimeSpan.FromMinutes(1));
            this.m_lobbyCap = new(TimeSpan.FromMinutes(0.25));
            this.m_lobbyMembers = new(TimeSpan.FromMinutes(0.25));

        }

        /// <summary>
        /// Fully initialise the remote lobby data.
        /// </summary>
        public void InitRemote() {

            // Create participant list
            this.m_participants = new(TimeSpan.FromMinutes(0.5));

            // Create query
            var _refreshQuery = new CommandQueryBuilder().GetObject(this.ObjectID)
                .GetProperties(nameof(ILobby.Axis), nameof(ILobby.Allies))
                .Loop(x => x.CreateIdentifier().Swap(1).GetProperty(nameof(ILobbyTeam.Slots))
                    .Loop(y =>
                        y.CreateIdentifier().Swap(1).GetProperties(nameof(ILobbyTeamSlot.SlotState), nameof(ILobbyTeamSlot.SlotOccupant)).Vector().Store("slot%i")
                    , true).Store("team%i"), true)
                .GetQuery();

            // Run collector
            var result = this.m_handle.Query(_refreshQuery);
            if (!result.WasExecuted) {
                throw new RemoteQueryFailedException();
            }

            // Get teams
            this.m_alliedTeam = this.m_handle.GetRemotableObject<ILobbyTeam>("lobby.Allies", (id, _) => new RemoteLobbyTeam(id, this.m_handle, this));
            this.m_axisTeam = this.m_handle.GetRemotableObject<ILobbyTeam>("lobby.Axis", (id, _) => new RemoteLobbyTeam(id, this.m_handle, this));

        }

        [RemotableMethod]
        public ILobbyAIParticipant CreateAIParticipant(AIDifficulty difficulty, ILobbyTeamSlot teamSlot, ILobbyCompany aiCompany) {
            return null;
        }

        [RemotableMethod]
        public ILobbyParticipant CreateParticipant(ulong participantID, string participantName) {
            return null;
        }

        [RemotableMethod]
        public ILobbyMatchContext CreateMatchContext() => throw new InvokePermissionAccessDeniedException("Cannot create match context when not hosting.");

        public IChatRoom GetChatRoom() => throw new NotImplementedException();

        public string GetLobbySetting(string lobbySetting)
            => this.m_handle.RemoteCall<string, ILobby>(this.ObjectID, nameof(this.GetLobbySetting), lobbySetting);

        public ILobbyParticipant GetParticipant(ulong participantID) => throw new NotImplementedException();

        public ILobbyParticipant[] GetParticipants() => throw new NotImplementedException();

        public bool IsObserver(ILobbyParticipant participant)
            => !this.Allies.IsTeamMember(participant) && !this.Axis.IsTeamMember(participant) && this.m_participants.Contains(participant);

        public ILobbyTeam GetTeam(ILobbyParticipant participant)
            => this.Allies.IsTeamMember(participant) ? this.Allies : (this.Axis.IsTeamMember(participant) ? this.Axis : null);

        [RemotableMethod]
        public bool RemoveParticipant(ILobbyParticipant participant, bool kick) {
            if (participant.IsSelf) {
                return kick ? throw new KickedFromLobbyException("Kicked from lobby.") : true;
            } else {
                this.MethodInvoked?.Invoke(this, nameof(RemoveParticipant), participant);
                return this.m_participants.Remove(participant);
            }
        }

        [RemotableMethod]
        public bool SetLobbySetting(string lobbySetting, string lobbySettingValue) {
            this.ValueChanged?.Invoke(this, new(lobbySetting, lobbySettingValue) { GUIOnly = true });
            return true;
        }

        [RemotableMethod]
        public bool SetSpectatorsAllowed(bool allowed) {
            this.m_spectatorsAllowed.SetCachedValue(allowed);
            return true;
        }

        [RemotableMethod]
        public bool SetTeamsCapacity(int maxTeamOccupants) {

            // Change capacity
            if (this.Allies.SetCapacity(maxTeamOccupants) && this.Axis.SetCapacity(maxTeamOccupants)) {

                // Notify
                this.ValueChanged?.Invoke(this, new(nameof(this.Capacity)));

                // Return success
                return true;

            }

            // Return false - something failed
            return false;

        }

        [RemotableMethod]
        public void SwapSlots(ILobbyTeamSlot fromSlot, ILobbyTeamSlot toSlot) => throw new NotImplementedException();

    }

}
