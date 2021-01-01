using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

using Battlegrounds;
using Battlegrounds.Game;
using Battlegrounds.Game.Battlegrounds;
using Battlegrounds.Game.Gameplay;
using Battlegrounds.Online.Lobby;

using BattlegroundsApp.LocalData;
using BattlegroundsApp.Views.ViewComponent;

namespace BattlegroundsApp.Models {

    public class LobbyTeamManagementModel {

        public const int MAX_TEAM = 4;

        private Grid m_teamGrid;
        private int m_maxPlayerCount;
        private bool m_isHost;
        private Dictionary<ManagedLobbyTeamType, List<PlayerCardView>> m_teamSetup;

        public event Action<ManagedLobbyTeamType, PlayerCardView, object, string> OnTeamEvent;

        public int TotalPlayerCount => this.m_teamSetup[ManagedLobbyTeamType.Axis].Count(x => x.IsOccupied) + this.m_teamSetup[ManagedLobbyTeamType.Allies].Count(x => x.IsOccupied);

        public LobbyTeamManagementModel(Grid teamGrid) {
            
            this.m_teamGrid = teamGrid;
            this.m_teamSetup = new Dictionary<ManagedLobbyTeamType, List<PlayerCardView>>() {
                [ManagedLobbyTeamType.Allies] = new List<PlayerCardView>(),
                [ManagedLobbyTeamType.Axis] = new List<PlayerCardView>(),
            };
            
            for (int i = 0; i < MAX_TEAM; i++) {
                this.CreatePlayercard(i, ManagedLobbyTeamType.Allies);
                this.CreatePlayercard(i, ManagedLobbyTeamType.Axis);
            }
            
            this.SetMaxPlayers(2);
            this.m_isHost = false;

        }

        private void CreatePlayercard(int row, ManagedLobbyTeamType type) {
            
            Contract.Requires(row > 0);
            Contract.Requires(row <= MAX_TEAM);
            Contract.Requires(type == ManagedLobbyTeamType.Allies || type == ManagedLobbyTeamType.Axis);

            PlayerCardView view = new PlayerCardView();
            view.SetValue(Grid.ColumnProperty, type == ManagedLobbyTeamType.Allies ? 0 : 1);
            view.SetValue(Grid.RowProperty, row);
            view.OnPlayercardEvent += this.OnCardActionHandler;
            view.SetAvailableArmies(type == ManagedLobbyTeamType.Allies);
            
            this.m_teamSetup[type].Add(view);
            this.m_teamGrid.Children.Add(view);
        
        }

        public void SetMaxPlayers(int count) {
            
            Contract.Requires(count > 0);
            Contract.Requires(count <= 8);
            Contract.Requires(count % 2 == 0);

            this.m_maxPlayerCount = count;
            for (int i = 0; i < MAX_TEAM; i++) {
                this.m_teamSetup[ManagedLobbyTeamType.Allies][i].Visibility = i < (count / 2) ? Visibility.Visible : Visibility.Collapsed;
                this.m_teamSetup[ManagedLobbyTeamType.Axis][i].Visibility = i < (count / 2) ? Visibility.Visible : Visibility.Collapsed;
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

        private void OnCardActionHandler(PlayerCardView sender, string reason) {
            if (!(this.m_isHost || sender.PlayerSteamID == BattlegroundsInstance.LocalSteamuser.ID)) {
                return;
            }
            ManagedLobbyTeamType teamOf = this.m_teamSetup[ManagedLobbyTeamType.Allies].Contains(sender) ? ManagedLobbyTeamType.Allies : ManagedLobbyTeamType.Axis;
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
                    throw new NotImplementedException();
                case "UnlockSlot":
                    throw new NotImplementedException();
                case "MoveTo":
                    throw new NotImplementedException();
                default:
                    Trace.WriteLine($"Unhandled playercard event '{reason}'", "LobbyTeamManagementModel.cs");
                    break;
            }
        }

        public int GetTeamSize(ManagedLobbyTeamType size) => this.m_teamSetup[size].Count(x => x.IsOccupied);
        
        public PlayerCardView GetLocalPlayercard() {
            ulong localID = BattlegroundsInstance.LocalSteamuser.ID;
            foreach (var team in this.m_teamSetup) {
                foreach (var player in team.Value) {
                    if (player.PlayerSteamID == localID) {
                        return player;
                    }
                }
            }
            return null;
        }

        public void SetIsHost(bool isHost) => this.m_isHost = isHost;

    }

}
