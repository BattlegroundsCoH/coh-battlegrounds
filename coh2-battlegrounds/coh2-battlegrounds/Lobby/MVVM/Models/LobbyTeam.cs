using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using System.Threading.Tasks;

using Battlegrounds.Game;
using Battlegrounds.Networking.LobbySystem;
using BattlegroundsApp.LocalData;

namespace BattlegroundsApp.Lobby.MVVM.Models {

    public class LobbyTeam {

        private LobbyAPIStructs.LobbyTeam m_interface;

        public LobbySlot[] Slots { get; }

        public ObservableCollection<LobbyCompanyItem> AvailableCompanies { get; set; }

        public LobbyAPIStructs.LobbyTeam Interface => this.m_interface;

        public LobbyTeam(LobbyAPIStructs.LobbyTeam lobbyTeam) {

            // Store team
            this.m_interface = lobbyTeam;

            // Define slot models
            this.Slots = new LobbySlot[4] {
                new(lobbyTeam.Slots[0], this),
                new(lobbyTeam.Slots[1], this),
                new(lobbyTeam.Slots[2], this),
                new(lobbyTeam.Slots[3], this)
            };

        }

        public void RefreshTeam(LobbyAPIStructs.LobbyTeam team) {
            
            // Set new interface
            this.m_interface = team;

            // Loop over slots and update
            for (int i = 0; i < team.Slots.Length; i++) {
                this.RefreshSlot(this.Slots[i], team.Slots[i]);
            }

        }

        public void RefreshSlot(LobbySlot slot, LobbyAPIStructs.LobbySlot interfaceObject) {

            // Update interface object
            slot.Interface = interfaceObject;

            // Triger refresh
            slot.RefreshVisuals();

        }

        public void ShowPlayercard(LobbySlot slot) {

        }

        public void KickOccupant(LobbySlot slot) {

            // Run async so we dont block stuff
            Task.Run(() => {

                // Remove Participant
                slot.Interface.API.RemoveOccupant(this.Interface.TeamID, slot.Interface.SlotID);

                Application.Current.Dispatcher.Invoke(() => {

                    // Trigger refresh on slot
                    slot.RefreshVisuals();

                });

            });

        }

        private LobbyCompanyItem GetDefaultCompany(int team) {

            // Fetch all of specified faction
            var available = PlayerCompanies.FindAll(x => (x.Army.IsAllied ? 0 : 1) == team);

            // If any available
            if (available.Count > 0) {
                return new LobbyCompanyItem(available[0]);
            } else {
                return new LobbyCompanyItem(LobbyCompanyItem.COMPANY_AUTO);
            }

        }

        public void AddAIPlayer(LobbySlot slot, string param) {

            // Get AI Difficulty
            AIDifficulty diff = (AIDifficulty)byte.Parse(param, CultureInfo.InvariantCulture);

            // Get company
            var company = GetDefaultCompany(slot.TeamID);

            // Run async so we dont block stuff
            Task.Run(() => {

                // Create AI
                this.Interface.API.AddAI(this.Interface.TeamID, slot.Interface.SlotID, (int)diff, company.GetAPIObject());

            });

        }

        public (bool, bool) CanPlay() {

            // Get slots in interface
            var slots = this.m_interface.Slots;

            // flag
            bool flag1 = false; // any valid player
            bool flag2 = true; // No invalid players

            // Loop over slots and check if any valid
            for (int i = 0; i < slots.Length; i++) {
                if (slots[i].State == 1) {
                    flag1 = true;
                    if (slots[i].Occupant.Company.IsNone) {
                        flag2 = false;
                    } else {
                        flag2 = flag2 && true;
                    }
                }
            }

            // Return if both flags yielded true
            return (flag1, flag2);

        }

    }

}
