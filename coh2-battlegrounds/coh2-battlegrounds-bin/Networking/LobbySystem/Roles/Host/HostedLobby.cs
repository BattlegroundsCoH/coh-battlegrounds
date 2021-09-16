using System;
using System.Collections.Generic;
using System.Linq;

using Battlegrounds.Game;
using Battlegrounds.Networking.ChatSystem;
using Battlegrounds.Networking.LobbySystem.Playing;
using Battlegrounds.Networking.Requests;

namespace Battlegrounds.Networking.LobbySystem.Roles.Host {

    public class HostedLobby : ILobby {

        private bool m_allowSpectators;

        private readonly Dictionary<string, string> m_settings;
        private readonly List<ILobbyParticipant> m_participants;

        public bool AllowSpectators => this.m_allowSpectators;

        public string Name { get; }

        public int Humans => this.m_participants.Count(x => x is not ILobbyAIParticipant);

        public int Capacity => this.Allies.SlotCapacity + this.Axis.SlotCapacity;

        public int Members => this.Allies.SlotCount + this.Axis.SlotCount;

        public ILobbyTeam Allies { get; }

        public ILobbyTeam Axis { get; }

        public IManagingRequestHandler RequestHandler { get; set; }

        public HostedLobby(string name) {

            // Set name
            this.Name = name;

            // Create teams
            this.Allies = new HostedLobbyTeam(this);
            this.Axis = new HostedLobbyTeam(this);

            // Create other data
            this.m_settings = new();
            this.m_participants = new();
            this.m_allowSpectators = true;

        }

        public ILobbyAIParticipant CreateAIParticipant(AIDifficulty difficulty, ILobbyTeamSlot teamSlot) {
            
            // Verify input
            if (teamSlot.SlotState is TeamSlotState.Occupied or TeamSlotState.Disabled) {
                throw new InvalidOperationException("Cannot add AI player to occupied or disabled slot");
            }

            // Create participant
            ILobbyAIParticipant participant = new HostedLobbyAIParticipant(difficulty);

            // Return participant if they were added
            return this.AddParticipant(participant, teamSlot) ? participant : null;

        }

        public ILobbyParticipant CreateParticipant(ulong participantID, string participantName) {

            // Create participant
            ILobbyParticipant participant = new HostedLobbyParticipant(participantID, participantName);

            // Find the first available slot
            var slot = this.FindFirstAvailableSlot();

            // Return participant if they were added
            return this.AddParticipant(participant, slot) ? participant : null;

        }
        
        private bool AddParticipant(ILobbyParticipant participant, ILobbyTeamSlot teamSlot) {

            // Add as observer
            if (teamSlot is null && this.m_allowSpectators) {
                this.m_participants.Add(participant);
                return true;
            }

            // If state is open
            if (teamSlot.SetOccupant(participant)) {
                this.m_participants.Add(participant);
                return true;
            }

            // Return false ==> Failed to add participant
            return false;

        }

        public IChatRoom GetChatRoom() => throw new NotImplementedException();

        public string GetLobbySetting(string lobbySetting) => this.m_settings.GetValueOrDefault(lobbySetting, string.Empty);

        public ILobbyParticipant GetParticipant(ulong participantID) => this.m_participants.FirstOrDefault(x => x.Id == participantID);

        public ILobbyParticipant[] GetParticipants() => this.m_participants.ToArray();

        public bool IsObserver(ILobbyParticipant participant)
            => !this.Allies.IsTeamMember(participant) && !this.Axis.IsTeamMember(participant) && this.m_participants.Contains(participant);

        public ILobbyTeam GetTeam(ILobbyParticipant participant)
            => this.Allies.IsTeamMember(participant) ? this.Allies : (this.Axis.IsTeamMember(participant) ? this.Axis : null);

        public bool RemoveParticipant(ILobbyParticipant participant, bool kick) {

            // Cannot kick or remove self
            if (participant.IsSelf) {
                return false;
            }

            // Remove from any occupied slot and remove from participant list
            if (this.Allies.Remove(participant) || this.Axis.Remove(participant) || this.IsObserver(participant)) {
                return this.m_participants.Remove(participant);
            }

            // Somehow failed then
            return false;

        }

        public bool SetLobbySetting(string lobbySetting, string lobbySettingValue) {
            this.m_settings[lobbySetting] = lobbySettingValue;
            return true;
        }
        
        public void SwapSlots(ILobbyTeamSlot fromSlot, ILobbyTeamSlot toSlot) => throw new NotImplementedException();

        public void SetTeamsCapacity(int maxTeamOccupants) {

        }

        public bool SetSpectatorsAllowed(bool allowed) {
            
            // If any participants are observers, we cannot do this
            if (this.m_participants.Any(x => this.IsObserver(x))) {
                return false;
            }

            // Update flag and return true
            this.m_allowSpectators = allowed;
            return true;

        }

        private ILobbyTeamSlot FindFirstAvailableSlot() {
            ILobbyTeamSlot slot = null;
            if (this.Allies.SlotCount > this.Axis.SlotCount && this.Axis.SlotCount != this.Axis.SlotCapacity) {
                slot = this.Axis.Slots.FirstOrDefault(x => x.SlotState is TeamSlotState.Open);
            } else if (this.Axis.SlotCount >= this.Allies.SlotCount && this.Allies.SlotCount != this.Allies.SlotCapacity) {
                slot = this.Allies.Slots.FirstOrDefault(x => x.SlotState is TeamSlotState.Open);
            }
            return slot;
        }

        public ILobbyMatchContext CreateMatchContext() => null;

    }

}
