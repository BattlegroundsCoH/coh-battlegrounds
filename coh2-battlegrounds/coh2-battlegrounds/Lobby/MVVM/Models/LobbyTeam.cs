using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using System.Threading.Tasks;

using Battlegrounds.Game;
using Battlegrounds.Networking.LobbySystem;

namespace BattlegroundsApp.Lobby.MVVM.Models {

    public class LobbyTeam {

        public LobbySlot[] Slots { get; }

        public ObservableCollection<LobbyCompanyItem> AvailableCompanies { get; set; }

        public LobbyAPIStructs.LobbyTeam Interface { get; }

        public LobbyTeam(LobbyAPIStructs.LobbyTeam lobbyTeam) {

            // Store team
            this.Interface = lobbyTeam;

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

            // Run async so we dont block stuff
            Task.Run(() => {

                // Remove Participant
                slot.NetworkInterface.API.RemoveOccupant(this.Interface.TeamID, slot.NetworkInterface.SlotID);

                Application.Current.Dispatcher.Invoke(() => {

                    // Trigger refresh on slot
                    slot.RefreshVisuals();

                });

            });

        }

        public void AddAIPlayer(LobbySlot slot, string param) {

            // Get AI Difficulty
            AIDifficulty diff = (AIDifficulty)byte.Parse(param, CultureInfo.InvariantCulture);

            // Run async so we dont block stuff
            Task.Run(() => {

                // Remove Participant
                this.Interface.API.AddAI(this.Interface.TeamID, slot.NetworkInterface.SlotID, (int)diff, null);

                // Update UI
                Application.Current.Dispatcher.Invoke(() => {

                    // Set default company index
                    slot.SelectedCompanyIndex = 0;

                    // Trigger refresh on slot
                    slot.RefreshVisuals();

                });

            });

        }

    }

}
