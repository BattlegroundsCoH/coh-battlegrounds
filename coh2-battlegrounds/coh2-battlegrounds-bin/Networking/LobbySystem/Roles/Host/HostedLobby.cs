using System;
using System.Collections.Generic;
using System.Linq;

using Battlegrounds.Game;
using Battlegrounds.Networking.ChatSystem;
using Battlegrounds.Networking.LobbySystem.Playing;
using Battlegrounds.Networking.Remoting.Reflection;
using Battlegrounds.Networking.Requests;
using Battlegrounds.Networking.Server;

namespace Battlegrounds.Networking.LobbySystem.Roles.Host {

    public class HostedLobby : ILobby {

        private bool m_allowSpectators;

        private readonly Dictionary<string, string> m_settings;
        private readonly List<ILobbyParticipant> m_participants;

        public event ObservableValueChangedHandler<ILobby> ValueChanged;
        public event ObservableMethodInvokedHandler<ILobby> MethodInvoked;

        [RemotableProperty(GetOnly = true)]
        public bool AllowSpectators => this.m_allowSpectators;

        [RemotableProperty(GetOnly = true)]
        public string Name { get; }

        [RemotableProperty(GetOnly = true)]
        public int Humans => this.m_participants.Count(x => x is not ILobbyAIParticipant);

        [RemotableProperty(GetOnly = true)]
        public int Capacity => this.Allies.SlotCapacity + this.Axis.SlotCapacity;

        [RemotableProperty(GetOnly = true)]
        public int Members => this.Allies.SlotCount + this.Axis.SlotCount;

        [RemotableProperty(GetOnly = true)]
        public ILobbyTeam Allies { get; }

        [RemotableProperty(GetOnly = true)]
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

        public ILobbyAIParticipant CreateAIParticipant(AIDifficulty difficulty, ILobbyTeamSlot teamSlot, ILobbyCompany aiCompany) {

            // Verify input
            if (teamSlot.SlotState is TeamSlotState.Occupied or TeamSlotState.Disabled) {
                throw new InvalidOperationException("Cannot add AI player to occupied or disabled slot");
            }

            // Create participant
            ILobbyAIParticipant participant = new HostedLobbyAIParticipant(difficulty);
            _ = participant.SetCompany(aiCompany);

            // Return participant if they were added
            return this.AddParticipant(participant, teamSlot) ? participant : null;

        }

        [RemotableMethod]
        public ILobbyParticipant CreateParticipant(ulong participantID, string participantName) {

            // Create participant
            ILobbyParticipant participant = new HostedLobbyParticipant(participantID, participantName);

            // Find the first available slot
            var slot = this.FindFirstAvailableSlot();

            // Add participant
            if (this.AddParticipant(participant, slot)) {

                // Broadcast event
                this.MethodInvoked?.Invoke(this, nameof(CreateParticipant), participantID, participantName);

                // Return participant
                return participant;

            }

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

        [RemotableMethod]
        public IChatRoom GetChatRoom() => throw new NotImplementedException();

        [RemotableMethod]
        public string GetLobbySetting(string lobbySetting) => this.m_settings.GetValueOrDefault(lobbySetting, string.Empty);

        [RemotableMethod]
        public ILobbyParticipant GetParticipant(ulong participantID) => this.m_participants.FirstOrDefault(x => x.Id == participantID);

        [RemotableMethod]
        public ILobbyParticipant[] GetParticipants() => this.m_participants.ToArray();

        [RemotableMethod]
        public bool IsObserver(ILobbyParticipant participant)
            => !this.Allies.IsTeamMember(participant) && !this.Axis.IsTeamMember(participant) && this.m_participants.Contains(participant);

        [RemotableMethod]
        public ILobbyTeam GetTeam(ILobbyParticipant participant)
            => this.Allies.IsTeamMember(participant) ? this.Allies : (this.Axis.IsTeamMember(participant) ? this.Axis : null);

        [RemotableMethod]
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

            // Set value
            this.m_settings[lobbySetting] = lobbySettingValue;

            // Notify
            this.ValueChanged?.Invoke(this, new(lobbySetting, lobbySettingValue));

            // Try update server setting (if any)
            this.UpdateServerSetting();

            // Return true
            return true;

        }

        private void UpdateServerSetting() {
            if (this.m_settings.TryGetValue("selected_map", out string map)
                && this.m_settings.TryGetValue("selected_wc", out string gamemode)
                && this.m_settings.TryGetValue("selected_wco", out string gamemodeoption)) {

                // Generate string to upload
                string uploadstr = string.IsNullOrEmpty(gamemodeoption) ? "" : $" ({gamemodeoption})";
                uploadstr = $"{map}, {gamemode}" + uploadstr;

                // Update server hub
                this.ValueChanged?.Invoke(this, new(nameof(IServerLobby.LobbyPlaymode), uploadstr) { IsBrokerEvent = true });

            }
        }

        [RemotableMethod]
        public void SwapSlots(ILobbyTeamSlot fromSlot, ILobbyTeamSlot toSlot) => throw new NotImplementedException();

        public bool SetTeamsCapacity(int maxTeamOccupants) {

            // Verify possible
            if (this.Allies.SlotCount > maxTeamOccupants || this.Axis.SlotCount > maxTeamOccupants) {
                return false;
            }

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

        public bool SetSpectatorsAllowed(bool allowed) {

            // If any participants are observers, we cannot do this
            if (this.m_participants.Any(x => this.IsObserver(x))) {
                return false;
            }

            // Update flag
            this.m_allowSpectators = allowed;

            // Notify
            this.ValueChanged?.Invoke(this, new(nameof(this.AllowSpectators), this.m_allowSpectators));

            // Return true
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
