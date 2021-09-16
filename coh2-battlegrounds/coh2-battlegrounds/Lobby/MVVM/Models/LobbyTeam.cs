using System;
using System.Globalization;

using Battlegrounds.Game;
using Battlegrounds.Networking.LobbySystem;

namespace BattlegroundsApp.Lobby.MVVM.Models {

    public class LobbyTeam {

        private readonly ILobby m_lobby;
        private readonly ILobbyTeam m_team;

        public LobbySlot[] Slots { get; }

        public LobbyTeam(ILobbyTeam lobbyTeam, ILobby lobby) {

            // Store team
            this.m_team = lobbyTeam;

            // Store lobby
            this.m_lobby = lobby;

            // Define slot models
            this.Slots = new LobbySlot[4] {
                new(lobbyTeam.Slots[0], this),
                new(lobbyTeam.Slots[1], this),
                new(lobbyTeam.Slots[2], this),
                new(lobbyTeam.Slots[3], this)
            };

        }

        public void ShowPlayercard(LobbySlot slot) {

        }

        public void KickOccupant(LobbySlot slot) {

            // Kick the occupant
            if (this.m_lobby.RemoveParticipant(slot.NetworkInterface.SlotOccupant, true)) {

                // Trigger refresh on slot
                slot.RefreshVisuals();

            }

        }

        public void Lock(LobbySlot slot) {

        }

        public void Unlock(LobbySlot slot) {

        }

        public void AddAIPlayer(LobbySlot slot, string param) {

            // Get AI Difficulty
            AIDifficulty diff = (AIDifficulty)byte.Parse(param, CultureInfo.InvariantCulture);

            // Join the AI player
            if (this.m_lobby.CreateAIParticipant(diff, slot.NetworkInterface) is ILobbyAIParticipant ai) {

                // Trigger refresh on slot
                slot.RefreshVisuals();

            }

        }

    }

}
