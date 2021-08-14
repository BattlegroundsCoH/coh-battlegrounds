using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;

using Battlegrounds;
using Battlegrounds.Game;
using Battlegrounds.Game.DataCompany;
using Battlegrounds.Game.Gameplay;
using Battlegrounds.Game.Match;
using Battlegrounds.Modding;
using Battlegrounds.Networking.Lobby;
using Battlegrounds.Networking.Remoting.Query;
using Battlegrounds.Networking.Requests;

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

        private ILobbyTeam m_mockAllies;
        private ILobbyTeam m_mockAxis;
        private ILobbyTeam m_mockObservers;

        private Dictionary<LobbyTeamType, TeamPlayerCard[]> m_teamSetup;

        private LobbyHandler m_handler;

        private static CommandQuery GetTeamsQuery;
        private CommandQueryResult GetTeamsQueryLatestResult;

        public TeamPlayerCard Self { get; private set; }

        public int HumanCount => this.AlliedHumanCount + this.AxisHumanCount;

        public int AlliedHumanCount => this.m_teamSetup[LobbyTeamType.Allies].Count(x => x.CardState is TeamPlayerCard.SELFSTATE or TeamPlayerCard.OCCUPIEDSTATE);

        public int AxisHumanCount => this.m_teamSetup[LobbyTeamType.Axis].Count(x => x.CardState is TeamPlayerCard.SELFSTATE or TeamPlayerCard.OCCUPIEDSTATE);

        public event Action OnModelNotification;

        public LobbyTeamManagementModel(TeamPlayerCard[][] teamPlayerCards, LobbyHandler lobbyHandler) {

            // Set the handler
            this.m_handler = lobbyHandler;

            // Prepare team grid
            this.m_teamSetup = new Dictionary<LobbyTeamType, TeamPlayerCard[]>() {
                [LobbyTeamType.Observers] = teamPlayerCards[0],
                [LobbyTeamType.Allies] = teamPlayerCards[1],
                [LobbyTeamType.Axis] = teamPlayerCards[2],
            };

            // Init cards
            foreach (KeyValuePair<LobbyTeamType, TeamPlayerCard[]> kvp in this.m_teamSetup) {
                for (int i = 0; i < kvp.Value.Length; i++) {
                    kvp.Value[i].Init(this.m_handler, kvp.Key, i);
                    kvp.Value[i].RequestFullRefresh = x => this.RefreshCard(x, x.TeamSlot, x.TeamType);
                    kvp.Value[i].NotifyLobby = () => this.OnModelNotification?.Invoke();
                }
            }

            // Create queries if not host
            if (!lobbyHandler.IsHost) {

                // Create the 'Get Teams' Query
                GetTeamsQuery = lobbyHandler.Lobby.BeginQuery()
                    .GetProperties(nameof(ILobby.AlliesTeam), nameof(ILobby.AxisTeam), nameof(ILobby.SpectatorTeam))
                    .Vector()
                    .Loop(x => x.Duplicate().Stringify().Store("tmp_name").GetProperty(nameof(ILobbyTeam.Slots)).Loop(
                        y => y.GetProperties(nameof(ILobbyTeamSlot.SlotState), nameof(ILobbyTeamSlot.SlotOccupant)).TestNull(
                            yes => yes.Push(0ul, string.Empty, string.Empty, string.Empty, 0.0),
                            no => no.GetProperties(
                                nameof(ILobbyMember.ID),
                                nameof(ILobbyMember.Name),
                                nameof(ILobbyMember.Army),
                                nameof(ILobbyMember.CompanyName),
                                nameof(ILobbyMember.CompanyValue))
                        ).Vector(6), true).Vector().Load("tmp_name").Swap(1).Store("$stack-0"), false)
                    .GetQuery();

                Trace.WriteLine($"Created '{nameof(GetTeamsQuery)}' query.", nameof(LobbyTeamManagementModel));

            }

        }

        public bool All(LobbyTeamType team, Predicate<TeamPlayerCard> predicate) => this.m_teamSetup[team].All(x => predicate(x));

        public bool Any(LobbyTeamType team, Predicate<TeamPlayerCard> predicate) => this.m_teamSetup[team].Any(x => predicate(x));

        public void SetMaxPlayers(int count) {

            if (count is < 0 or > (2 * MAXTEAMPLAYERCOUNT)) {
                return;
            }

            for (int i = 0; i < MAXTEAMPLAYERCOUNT; i++) {
                bool show = i < (count / 2);
                this.m_teamSetup[LobbyTeamType.Allies][i].Visibility = show ? Visibility.Visible : Visibility.Hidden;
                this.m_teamSetup[LobbyTeamType.Axis][i].Visibility = show ? Visibility.Visible : Visibility.Hidden;
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
            LobbyTeamType.Allies => this.m_handler.IsHost ? this.m_handler.Lobby.AlliesTeam : this.m_mockAllies,
            LobbyTeamType.Axis => this.m_handler.IsHost ? this.m_handler.Lobby.AxisTeam : this.m_mockAxis,
            LobbyTeamType.Observers => this.m_handler.IsHost ? this.m_handler.Lobby.SpectatorTeam : this.m_mockObservers,
            _ => throw new Exception()
        };

        public void RefreshAll(bool refreshObservers) {

            if (!this.m_handler.IsHost) {
                this.GetTeamsQueryLatestResult = GetTeamsQuery.ExecuteRemote(this.m_handler.RequestHandler);
                if (!this.GetTeamsQueryLatestResult.WasExecuted) {
                    Trace.WriteLine("Failed to get response to command query requesting all team data.", nameof(LobbyTeamManagementModel));
                    return;
                } else {
                    this.CreateMockData();
                }
            }

            this.RefreshTeam(LobbyTeamType.Allies);
            this.RefreshTeam(LobbyTeamType.Axis);
            if (refreshObservers) {
                this.RefreshTeam(LobbyTeamType.Observers);
            }
        }

        public void RefreshTeam(LobbyTeamType teamType) {
            ILobbyTeam team = this.GetLobbyTeamFromType(teamType);
            int cap = team.Capacity;
            for (int i = 0; i < MAXTEAMPLAYERCOUNT; i++) {
                if (i < cap) {
                    this.RefreshCard(this.m_teamSetup[teamType][i], team.GetSlotAt(i), teamType);
                }
            }
        }

        public void RefreshCard(TeamPlayerCard playerCard, ILobbyTeamSlot slot, LobbyTeamType teamType) {

            playerCard.IsAllies = teamType == LobbyTeamType.Allies;

            if (slot.SlotState == LobbyTeamSlotState.OPEN) {

                playerCard.SetCardState(TeamPlayerCard.OPENSTATE);

            } else if (slot.SlotState == LobbyTeamSlotState.LOCKED) {

                playerCard.SetCardState(TeamPlayerCard.OPENSTATE);

            } else if (slot.SlotState == LobbyTeamSlotState.OCCUPIED) {

                // Get the occupant
                ILobbyMember occupant = slot.SlotOccupant;
                IAILobbyMember ai = occupant as IAILobbyMember;
                bool isAI = ai is not null;

                // Determine viewstate
                if (teamType == LobbyTeamType.Observers) {
                    playerCard.SetCardState(TeamPlayerCard.OBSERVERSTATE);
                } else {
                    if (this.m_handler.IsHost && isAI) {
                        playerCard.SetCardState(TeamPlayerCard.AISTATE);
                    } else {
                        playerCard.SetCardState(occupant.Equals(this.m_handler.Self) ? TeamPlayerCard.SELFSTATE : TeamPlayerCard.OCCUPIEDSTATE);
                    }
                }

                // Set player visual data
                playerCard.Playername = isAI ? ((AIDifficulty)ai.Difficulty).GetIngameDisplayName() : occupant.Name;
                playerCard.Playercompany = occupant.CompanyName;
                playerCard.Playerarmy = occupant.Army;
                playerCard.SetArmyIconIfNotHost();

                // Triger value update
                playerCard.RefreshVisualProperty(nameof(playerCard.Playername));
                playerCard.RefreshVisualProperty(nameof(playerCard.Playercompany));

                // If self, make refresh army and company data
                if (playerCard.CardState is TeamPlayerCard.SELFSTATE or TeamPlayerCard.AISTATE && teamType != LobbyTeamType.Observers) {
                    playerCard.RefreshArmyData();
                    playerCard.OnFactionChangedHandle = this.SelfChangedArmy;
                    playerCard.OnCompanyChangedHandle = this.SelfChangedCompany;
                    if (occupant.Equals(this.m_handler.Self)) {
                        this.Self = playerCard;
                    }
                } else {
                    if (this.m_handler.IsHost && isAI) {
                        playerCard.OnFactionChangedHandle = x => this.AIChangedArmy(ai, x);
                        playerCard.OnCompanyChangedHandle = x => this.AIChangedCompany(ai, x);
                    } else {
                        playerCard.OnCompanyChangedHandle = null;
                        playerCard.OnFactionChangedHandle = null;
                    }
                }

            }

        }

        private void SelfChangedCompany(TeamPlayerCompanyItem companyItem) {
            if (companyItem is not null) {
                if (companyItem.State is CompanyItemState.Company) {
                    this.m_handler.Lobby.Self.SetCompany(companyItem.Name, companyItem.Strength);
                } else {
                    this.m_handler.Lobby.Self.SetCompany(string.Empty, -1.0);
                }
            }
        }

        private void SelfChangedArmy(TeamPlayerArmyItem armyItem) {
            if (armyItem is not null && Faction.FromName(armyItem.Name) is Faction faction) {
                this.m_handler.Lobby.Self.SetArmy(faction.Name);
                this.Self.RefreshCompanyData();
            }
        }

        private void AIChangedCompany(IAILobbyMember lobbyAIMember, TeamPlayerCompanyItem companyItem) {
            if (companyItem is not null) {
                lobbyAIMember.SetCompany(companyItem.Name, companyItem.Strength);
            }
        }

        private void AIChangedArmy(IAILobbyMember lobbyAIMember, TeamPlayerArmyItem armyItem) {
            if (armyItem is not null && Faction.FromName(armyItem.Name) is Faction faction) {
                lobbyAIMember.SetArmy(faction.Name);
            }
        }

        public List<SessionParticipant> GetParticipants(LobbyTeamType team, ref byte j) {

            List<SessionParticipant> participants = new List<SessionParticipant>();
            SessionParticipantTeam participantTeam = team == LobbyTeamType.Allies ? SessionParticipantTeam.TEAM_ALLIES : SessionParticipantTeam.TEAM_AXIS;

            byte i = 0;
            foreach (TeamPlayerCard slot in this.m_teamSetup[team]) {
                SessionParticipant? participant = slot.CardState switch {
                    TeamPlayerCard.SELFSTATE or TeamPlayerCard.OCCUPIEDSTATE => new SessionParticipant(slot.Playername, slot.TeamSlot.SlotOccupant.ID, null, participantTeam, i, j),
                    TeamPlayerCard.AISTATE => new SessionParticipant((AIDifficulty)(slot.TeamSlot.SlotOccupant as IAILobbyMember).Difficulty, GetAICompany(slot), participantTeam, i, j),
                    _ => null
                };
                if (participant.HasValue) {
                    participants.Add(participant.Value);
                    i++;
                    j++;
                }
            }

            return participants;

        }

        private void CreateMockData() {

            // Create mock teams based on vector data
            this.m_mockAllies = new MockLobbyTeamModel(1, this.m_handler, this.GetTeamsQueryLatestResult["AlliesTeam"] as CommandQueryResultVector);
            this.m_mockAxis = new MockLobbyTeamModel(2, this.m_handler, this.GetTeamsQueryLatestResult["AxisTeam"] as CommandQueryResultVector);
            this.m_mockObservers = new MockLobbyTeamModel(0, this.m_handler, this.GetTeamsQueryLatestResult["SpectatorTeam"] as CommandQueryResultVector);

        }

        private static Company GetAICompany(TeamPlayerCard aiPlayercard) {
            if (aiPlayercard.AICompanySelector.SelectedItem is TeamPlayerCompanyItem companyItem) {
                if (companyItem.State == CompanyItemState.Company) {
                    return PlayerCompanies.FromNameAndFaction(companyItem.Name, Faction.FromName(companyItem.Army));
                } else if (companyItem.State == CompanyItemState.Generate) {
                    Trace.TraceWarning("Using default BG tuning mod to generate company.");
                    return CompanyGenerator.Generate(Faction.FromName(companyItem.Army), ModManager.GetPackage("mod_bg").TuningGUID.GUID, false, true, true);
                }
            }
            throw new Exception();
        }

    }

    public class MockLobbyTeamModel : ILobbyTeam {

        private CommandQueryResultVector m_data;
        private ILobbyTeamSlot[] m_slots;

        public int Capacity { get; }

        public int Size => this.m_slots.Count(x => x.SlotState is LobbyTeamSlotState.OCCUPIED);

        public int TeamIndex { get; }

        public bool HasOpenSlot => this.m_slots.Any(x => x.SlotState is LobbyTeamSlotState.OPEN);

        public ILobbyTeamSlot[] Slots => this.m_slots;

        public IRequestHandler RequestHandler { get; }

        public MockLobbyTeamModel(int tid, LobbyHandler handler, CommandQueryResultVector dataVector) {

            // Set data
            this.RequestHandler = handler.RequestHandler;
            this.m_data = dataVector.Reverse();

            // Set Team Index
            this.TeamIndex = tid;

            // Set capacity
            this.Capacity = this.m_data.Dimensions;
            this.m_slots = new ILobbyTeamSlot[this.Capacity];

            // Parse vector data
            for (int i = 0; i < this.m_data.Dimensions; i++) {

                // Get the slot data
                var slot = (this.m_data[i] as CommandQueryResultVector).Reverse();
                var (slotState, id, name, army, company, companyValue) = slot.ToTuple<string, ulong, string, string, string, double>();
                if (slotState is not "OCCUPIED") {
                    this.m_slots[i] = new MockLobbyTeamSlotModel(slotState, null);
                } else {

                    if (id == handler.Self.ID) {
                        this.m_slots[i] = new MockLobbyTeamSlotModel(slotState, handler.Self);
                    } else {
                        this.m_slots[i] = new MockLobbyTeamSlotModel(slotState, new MockLobbyTeamMemberModel(id, name, army, company, companyValue));
                    }

                }

            }

        }

        public bool CanSetCapacity(int capacity) => throw new NotSupportedException();

        public ILobbyTeamSlot GetSlotAt(int index) => this.m_slots[index];

        public bool IsMember(ILobbyMember member) {
            for (int i = 0; i < this.Slots.Length; i++) {
                if (this.Slots[i].SlotState is LobbyTeamSlotState.OCCUPIED && this.Slots[i].SlotOccupant.ID == member.ID) {
                    return true;
                }
            }
            return false;
        }

        public void JoinTeam(ILobbyMember member) => throw new NotSupportedException();

        public void LeaveTeam(ILobbyMember member) => throw new NotSupportedException();

        public void SetCapacity(int capacity) => throw new NotSupportedException();

        public void SwapSlots(ILobbyMember from, int to) => throw new NotSupportedException();

        public void SwapSlots(int from, int to) => throw new NotSupportedException();

    }

    public class MockLobbyTeamSlotModel : ILobbyTeamSlot {

        public LobbyTeamSlotState SlotState { get; set; }

        public ILobbyMember SlotOccupant { get; set; }

        public MockLobbyTeamSlotModel(string slotState, ILobbyMember member) {

            // Get slot state
            this.SlotState = Enum.Parse<LobbyTeamSlotState>(slotState);

            // Set member
            this.SlotOccupant = member;

        }

    }

    public class MockLobbyTeamMemberModel : ILobbyMember {

        public ulong ID { get; }

        public string Name { get; }

        public string Army { get; }

        public bool HasCompany => !string.IsNullOrEmpty(this.CompanyName);

        public string CompanyName { get; }

        public double CompanyValue { get; }

        public bool IsLocalMachine => false;

        public MockLobbyTeamMemberModel(ulong id, string name, string army, string company, double companyValue) {
            this.ID = id;
            this.Name = name;
            this.Army = army;
            this.CompanyName = company;
            this.CompanyValue = companyValue;
        }

        public void SetArmy(string army) => throw new NotSupportedException();

        public void SetCompany(string name, double value) => throw new NotSupportedException();

    }

}
