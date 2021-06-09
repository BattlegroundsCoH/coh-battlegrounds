using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

using Battlegrounds;
using Battlegrounds.Game;
using Battlegrounds.Game.DataCompany;
using Battlegrounds.Game.Gameplay;
using Battlegrounds.Game.Match;
using Battlegrounds.Networking.Lobby;
using Battlegrounds.Online.Lobby;

using BattlegroundsApp.LocalData;
using BattlegroundsApp.Views.ViewComponent;

namespace BattlegroundsApp.Models {

    public enum LobbyTeamType {
        Observers = 0,
        Allies = 1,
        Axis = 2
    }

    public class LobbyTeamManagementModel {

        public const int MAXTEAMPLAYERCOUNT = 4;

        private Dictionary<LobbyTeamType, TeamPlayerCard[]> m_teamSetup;

        private LobbyHandler m_handler;

        public event Action<LobbyTeamType, TeamPlayerCard, object, string> OnTeamEvent;

        public LobbyTeamManagementModel(TeamPlayerCard[][] teamPlayerCards, LobbyHandler lobbyHandler) {

            // Set the handler
            this.m_handler = lobbyHandler;

            // Prepare team grid
            this.m_teamSetup = new Dictionary<LobbyTeamType, TeamPlayerCard[]>() {
                [LobbyTeamType.Observers] = teamPlayerCards[0],
                [LobbyTeamType.Allies] = teamPlayerCards[1],
                [LobbyTeamType.Axis] = teamPlayerCards[2],
            };

        }

        public void SetMaxPlayers(int count) {

            if (count is < 0 or > MAXTEAMPLAYERCOUNT) {
                return;
            }

            for (int i = 0; i < MAXTEAMPLAYERCOUNT; i++) {
                this.m_teamSetup[LobbyTeamType.Allies][i].Visibility = i < (count / 2) ? Visibility.Visible : Visibility.Hidden;
                this.m_teamSetup[LobbyTeamType.Axis][i].Visibility = i < (count / 2) ? Visibility.Visible : Visibility.Hidden;
            }

        }

        public void SetMaxObservers(int count) {

            if (count is < 0 or > MAXTEAMPLAYERCOUNT) {
                return;
            }

            for (int i = 0; i < MAXTEAMPLAYERCOUNT; i++) {
                this.m_teamSetup[LobbyTeamType.Observers][i].Visibility = i < count ? Visibility.Visible : Visibility.Hidden;
            }

        }

        private ILobbyTeam GetLobbyTeamFromType(LobbyTeamType lobbyTeamType) => lobbyTeamType switch {
            LobbyTeamType.Allies => this.m_handler.Lobby.AlliesTeam,
            LobbyTeamType.Axis => this.m_handler.Lobby.AxisTeam,
            LobbyTeamType.Observers => this.m_handler.Lobby.SpectatorTeam,
            _ => throw new Exception()
        };

        public void RefreshAll(bool refreshObservers) {
            this.RefreshTeam(LobbyTeamType.Allies);
            this.RefreshTeam(LobbyTeamType.Axis);
            if (refreshObservers) {
                this.RefreshTeam(LobbyTeamType.Observers);
            }
        }

        public void RefreshTeam(LobbyTeamType teamType) {
            ILobbyTeam team = this.GetLobbyTeamFromType(teamType);
            for (int i = 0; i < MAXTEAMPLAYERCOUNT; i++) {
                this.RefreshCard(this.m_teamSetup[teamType][i], team.GetSlotAt(i), teamType == LobbyTeamType.Observers);
            }
        }

        public void RefreshCard(TeamPlayerCard playerCard, ILobbyTeamSlot slot, bool isObserver) {

            if (slot.SlotState == LobbyTeamSlotState.OPEN) {

                playerCard.SetCardState(TeamPlayerCard.OPENSTATE);

            } else if (slot.SlotState == LobbyTeamSlotState.LOCKED) {

                playerCard.SetCardState(TeamPlayerCard.OPENSTATE);

            } else if (slot.SlotState == LobbyTeamSlotState.OCCUPIED) {

                // Get the occupant
                ILobbyMember occupant = slot.SlotOccupant;

                // Determine viewstate
                if (isObserver) {
                    playerCard.SetCardState(TeamPlayerCard.OBSERVERSTATE);
                } else {
                    playerCard.SetCardState(occupant.IsLocalMachine ? TeamPlayerCard.SELFSTATE : TeamPlayerCard.OCCUPIEDSTATE);
                }

                // Set player visual data
                playerCard.Playername = occupant.Name;
                playerCard.Playercompany = occupant.CompanyName;
                playerCard.Playerarmy = occupant.Army;
                playerCard.IsAllies = !isObserver && Faction.FromName(occupant.Army).IsAllied;

                // If self, make refresh army and company data
                if (occupant.IsLocalMachine && !isObserver) {
                    playerCard.SetSelfDataIfNone();
                }

            }

        }

        private PlayercardCompanyItem CreateCompanyFromOccupant(ManagedLobbyMember member) {
            if (member is HumanLobbyMember human) {
                if (!string.IsNullOrEmpty(human.CompanyName) && human.CompanyName.CompareTo("NULL") != 0) {
                    return new PlayercardCompanyItem(CompanyItemState.Company, human.CompanyName, human.CompanyStrength);
                } else {
                    return new PlayercardCompanyItem(CompanyItemState.None, "NULL");
                }
            } else if (member is AILobbyMember ai) {
                if (!string.IsNullOrEmpty(ai.CompanyName) && ai.CompanyName.CompareTo("NULL") != 0) {
                    return new PlayercardCompanyItem(CompanyItemState.Company, ai.CompanyName, ai.CompanyStrength);
                } else if (ai.CompanyName.CompareTo("AUGEN") == 0) {
                    return new PlayercardCompanyItem(CompanyItemState.Generate, "AUGEN");
                } else {
                    return new PlayercardCompanyItem(CompanyItemState.None, "NULL");
                }
            } else {
                throw new NotImplementedException();
            }
        }

        public List<SessionParticipant> GetParticipants(ManagedLobbyTeamType team) {

            List<SessionParticipant> participants = new List<SessionParticipant>();

            return participants;

        }

        private ManagedLobbyTeamType GetTeamOfCard(TeamPlayerCard playerCard) 
            => this.m_teamSetup[LobbyTeamType.Allies].Contains(playerCard) ? ManagedLobbyTeamType.Allies : ManagedLobbyTeamType.Axis;

    }

}
