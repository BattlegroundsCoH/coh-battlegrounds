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

    public class LobbyTeamManagementModel {

        public const int MAX_TEAM_PLAYERCOUNT = 4;

        private Grid m_teamGrid;
        private int m_maxPlayerCount;
        private Dictionary<ManagedLobbyTeamType, List<PlayerCardView>> m_teamSetup;

        private LobbyHandler m_handler;

        public event Action<ManagedLobbyTeamType, PlayerCardView, object, string> OnTeamEvent;

        /// <summary>
        /// Get the total amount of players (Including AI).
        /// </summary>
        public int TotalPlayerCount => this.m_teamSetup[ManagedLobbyTeamType.Axis].Count(x => x.IsOccupied) + this.m_teamSetup[ManagedLobbyTeamType.Allies].Count(x => x.IsOccupied);

        /// <summary>
        /// Get the total amount of human players.
        /// </summary>
        public int TotalHumanCount =>
            this.m_teamSetup[ManagedLobbyTeamType.Axis].Count(x => !x.IsAI && x.IsOccupied) + this.m_teamSetup[ManagedLobbyTeamType.Allies].Count(x => !x.IsAI && x.IsOccupied);

        public LobbyTeamManagementModel(Grid teamGrid, LobbyHandler lobbyHandler) {

            // Set the handler
            this.m_handler = lobbyHandler;

            // Prepare team grid
            this.m_teamGrid = teamGrid;
            this.m_teamSetup = new Dictionary<ManagedLobbyTeamType, List<PlayerCardView>>() {
                [ManagedLobbyTeamType.Allies] = new List<PlayerCardView>(),
                [ManagedLobbyTeamType.Axis] = new List<PlayerCardView>(),
            };
            
            // Create player cards.
            for (int i = 0; i < MAX_TEAM_PLAYERCOUNT; i++) {
                this.CreatePlayercard(i, ManagedLobbyTeamType.Allies);
                this.CreatePlayercard(i, ManagedLobbyTeamType.Axis);
            }

            // Set host state
            this.SetIsHost(this.m_handler.IsHost);

        }

        private void CreatePlayercard(int row, ManagedLobbyTeamType type) {
            
            PlayerCardView view = new PlayerCardView(row);
            view.SetValue(Grid.ColumnProperty, type == ManagedLobbyTeamType.Allies ? 0 : 1);
            view.SetValue(Grid.RowProperty, row);
            view.OnPlayercardEvent += this.OnCardActionHandler;
            view.SetAvailableArmies(type == ManagedLobbyTeamType.Allies);
            
            this.m_teamSetup[type].Add(view);
            this.m_teamGrid.Children.Add(view);
        
        }

        public void SetMaxPlayers(int count) {
            if (count > 0) {
                this.m_maxPlayerCount = count;
                for (int i = 0; i < MAX_TEAM_PLAYERCOUNT; i++) {
                    this.m_teamSetup[ManagedLobbyTeamType.Allies][i].Visibility = i < (count / 2) ? Visibility.Visible : Visibility.Collapsed;
                    this.m_teamSetup[ManagedLobbyTeamType.Axis][i].Visibility = i < (count / 2) ? Visibility.Visible : Visibility.Collapsed;
                }
            } else {
                Trace.WriteLine("Unable to set max player count to a negative value.", "LobbyTeamManagementModel");
            }
        }

        public void UpdateTeamview(ManagedLobby lobby, bool isHost) {

            foreach (var pair in this.m_teamSetup) {

                var team = lobby.GetTeam(pair.Key);

                for (int i = 0; i < team.Slots.Length; i++) {
                    if (team.Slots[i].State == ManagedLobbyTeamSlotState.Occupied) {
                        var occ = team.Slots[i].Occupant;
                        pair.Value[i].SetStateBasedOnContext(isHost, occ is AILobbyMember, occ.ID);
                        if (occ is AILobbyMember aiMember) {
                            pair.Value[i].SetAIData(aiMember.Difficulty, aiMember.Faction);
                        } else {
                            pair.Value[i].SetPlayerData(occ.Name, occ.Faction, this.CreateCompanyFromOccupant(occ));
                        }
                        pair.Value[i].SetCardState(PlayercardViewstate.Occupied);
                    } else {
                        pair.Value[i].SetStateBasedOnContext(isHost, false, ulong.MaxValue);
                        pair.Value[i].SetCardState(i < this.m_maxPlayerCount / 2 ? PlayercardViewstate.Open : PlayercardViewstate.Locked);
                    }
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

            byte i = 0;
            foreach (var player in this.m_teamSetup[team]) {
                if (player.IsOccupied) {
                    if (player.IsAI) {
                        participants.Add(new SessionParticipant(
                            AIDifficulty.AI_Hard,
                            this.GetAICompany(player), 
                            (team == ManagedLobbyTeamType.Allies) ? SessionParticipantTeam.TEAM_ALLIES : SessionParticipantTeam.TEAM_AXIS, 
                            i));
                    } else {
                        participants.Add(new SessionParticipant(
                            player.PlayerName,
                            player.PlayerSteamID,
                            null,
                            (team == ManagedLobbyTeamType.Allies) ? SessionParticipantTeam.TEAM_ALLIES : SessionParticipantTeam.TEAM_AXIS,
                            i));
                    }
                    i++;
                }
            }

            return participants;

        }

        private Company GetAICompany(PlayerCardView view) {
            Faction faction = Faction.FromName(view.PlayerArmy);
            if (view.PlayerSelectedCompanyItem.State == CompanyItemState.Company) {
                return PlayerCompanies.FromNameAndFaction(view.PlayerSelectedCompanyItem.Name, faction);
            } else if (view.PlayerSelectedCompanyItem.State == CompanyItemState.Generate) {
                return CompanyGenerator.Generate(faction, BattlegroundsInstance.BattleGroundsTuningMod.Guid.ToString().Replace("-", ""), false, true, true);
            } else {
                throw new Exception();
            }
        }

        private ManagedLobbyTeamType GetTeamOfCard(PlayerCardView playerCard) 
            => this.m_teamSetup[ManagedLobbyTeamType.Allies].Contains(playerCard) ? ManagedLobbyTeamType.Allies : ManagedLobbyTeamType.Axis;

        private void OnCardActionHandler(PlayerCardView sender, string reason) {
            
            if (!(this.m_handler.IsHost || BattlegroundsInstance.IsLocalUser(sender.PlayerSteamID))) {
                return;
            }

            ManagedLobbyTeamType teamOf = this.GetTeamOfCard(sender);
            switch (reason) {
                case "AddAI":
                    if (teamOf == ManagedLobbyTeamType.Allies) {
                        sender.SetAIData(AIDifficulty.AI_Hard, "soviet");
                        OnTeamEvent?.Invoke(ManagedLobbyTeamType.Allies, sender, this.TotalPlayerCount, reason);
                    } else {
                        sender.SetAIData(AIDifficulty.AI_Hard, "german");
                        OnTeamEvent?.Invoke(ManagedLobbyTeamType.Axis, sender, this.TotalPlayerCount, reason);
                    }
                    break;
                case "ChangedArmy":
                    OnTeamEvent?.Invoke(teamOf, sender, sender.PlayerArmy, reason);
                    break;
                case "ChangedCompany":
                    OnTeamEvent?.Invoke(teamOf, sender, sender.PlayerSelectedCompanyItem, reason);
                    break;
                case "RemovePlayer":
                    sender.SetCardState(PlayercardViewstate.Open);
                    OnTeamEvent?.Invoke(teamOf, sender, this.m_teamSetup[teamOf].IndexOf(sender), reason);
                    break;
                case "LockSlot":
                    sender.SetCardState(PlayercardViewstate.Locked);
                    this.OnTeamEvent?.Invoke(teamOf, sender, null, reason);
                    break;
                case "UnlockSlot":
                    sender.SetCardState(PlayercardViewstate.Open);
                    this.OnTeamEvent?.Invoke(teamOf, sender, null, reason);
                    break;
                case "MoveTo":
                    this.MoveTeam(GetLocalPlayercard(), sender);
                    break;
                default:
                    Trace.WriteLine($"Unhandled playercard event '{reason}'", "LobbyTeamManagementModel");
                    break;
            }
        }

        private void MoveTeam(PlayerCardView from, PlayerCardView to) {
            var fromTeam = this.GetTeamOfCard(from);
            var toTeam = this.GetTeamOfCard(to);
            string updatedReason = "MoveToAxis";
            if (fromTeam == toTeam) {
                updatedReason = "MoveTo";
            } else if (toTeam == ManagedLobbyTeamType.Allies) {
                updatedReason = "MoveToAllies";
            }
            this.OnTeamEvent?.Invoke(fromTeam, from, to, updatedReason);
        }

        public int GetTeamSize(ManagedLobbyTeamType size) => this.m_teamSetup[size].Count(x => x.IsOccupied);
        
        public PlayerCardView GetLocalPlayercard() {
            foreach (var team in this.m_teamSetup) {
                foreach (var player in team.Value) {
                    if (BattlegroundsInstance.IsLocalUser(player.PlayerSteamID)) {
                        return player;
                    }
                }
            }
            return null;
        }

        private void SetIsHost(bool isHost) {
            foreach (var pair in this.m_teamSetup) {
                foreach (var card in pair.Value) {
                    card.SetStateBasedOnContext(isHost, card.IsAI, card.PlayerSteamID);
                }
            }
        }

    }

}
