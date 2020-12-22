using System;
using System.Collections.Generic;
using System.Linq;
using Battlegrounds.Functional;

namespace Battlegrounds.Online.Lobby {
    
    /// <summary>
    /// Simple representation of which teams types can be represented.
    /// </summary>
    public enum ManagedLobbyTeamType {
        
        /// <summary>
        /// Spectator Team (May spectate a match).
        /// </summary>
        /// <remarks>
        /// May be used for surplus players when map size changes.
        /// </remarks>
        Spectator = 0,
        
        /// <summary>
        /// Allied Team
        /// </summary>
        Allies = 1,
        
        /// <summary>
        /// Axis Team
        /// </summary>
        Axis = 2,

    }

    /// <summary>
    /// Sealed class representing a Team in the <see cref="ManagedLobby"/> object.
    /// </summary>
    public sealed class ManagedLobbyTeam {

        public static readonly ManagedLobbyTeamType[] TeamTypes = { ManagedLobbyTeamType.Allies, ManagedLobbyTeamType.Axis, ManagedLobbyTeamType.Spectator };

        private ManagedLobby m_lobby;
        private ManagedLobbyTeamSlot[] m_slots;

        /// <summary>
        /// The available <see cref="ManagedLobbyTeamSlot"/> slots in this team.
        /// </summary>
        public ManagedLobbyTeamSlot[] Slots => this.m_slots;

        /// <summary>
        /// All <see cref="ManagedLobbyMember"/> members in the team.
        /// </summary>
        public ManagedLobbyMember[] Members => this.m_slots.Where(x => x.State == ManagedLobbyTeamSlotState.Occupied).Select(x => x.Occupant).ToArray();

        /// <summary>
        /// The max capacity of this team.
        /// </summary>
        public int Capacity => this.Slots.Length;

        /// <summary>
        /// The amount of active members on the team.
        /// </summary>
        public int Count => this.Members.Length;

        /// <summary>
        /// The <see cref="ManagedLobbyTeamType"/> the team is a representation of.
        /// </summary>
        public ManagedLobbyTeamType Team { get; }

        public ManagedLobbyTeam(ManagedLobby lobby, byte teamsize, ManagedLobbyTeamType teamtype) {
            this.Team = teamtype;
            this.m_lobby = lobby;
            this.m_slots = new ManagedLobbyTeamSlot[teamsize];
            for (int i = 0; i < teamsize; i++) {
                this.m_slots[i] = new ManagedLobbyTeamSlot(i);
            }
        }

        public void ForEachMember(Action<ManagedLobbyMember> action) {
            foreach (ManagedLobbyMember member in this.Members) {
                action(member);
            }
        }

        public bool AllMembers(Predicate<ManagedLobbyMember> predicate) {
            foreach (ManagedLobbyMember member in this.Members) {
                if (!predicate(member)) {
                    return false;
                }
            }
            return true;
        }

        public ManagedLobbyMember[] SetCapacity(int max) {
            ManagedLobbyTeamSlot[] slots = new ManagedLobbyTeamSlot[max];
            List<ManagedLobbyMember> removed = new List<ManagedLobbyMember>();
            for (int i = 0; i < slots.Length; i++) {
                slots[i] = new ManagedLobbyTeamSlot(i);
                if (i < this.m_slots.Length && this.m_slots[i] is not null && this.m_slots[i].State == ManagedLobbyTeamSlotState.Occupied) {
                    slots[i].SetOccupant(this.m_slots[i].Occupant);

                }
            }
            for (int i = slots.Length; i < this.m_slots.Length; i++) {
                if (this.m_slots[i].State == ManagedLobbyTeamSlotState.Occupied) {
                    removed.Add(this.m_slots[i].Occupant);
                }
            }
            this.m_slots = slots;
            return removed.ToArray();
        }

        public bool Join(ManagedLobbyMember member, bool silent = true) {
            for (int i = 0; i < this.m_slots.Length; i++) {
                if (this.m_slots[i].State == ManagedLobbyTeamSlotState.Open) {
                    this.m_slots[i].SetOccupant(member);
                    if (!silent) {
                        this.m_lobby.SetUserInformation(member.ID, "tid", (int)this.Team);
                        this.m_lobby.SetUserInformation(member.ID, "pos", i);
                    }
                    return true;
                }
            }
            return false;
        }

        public void Leave(ManagedLobbyMember member) {
            for (int i = 0; i < this.m_slots.Length; i++) {
                if (this.m_slots[i].Occupant == member) {
                    this.m_slots[i].SetOccupant(null);
                }
            }
        }

        public void Clear() {
            for (int i = 0; i < this.m_slots.Length; i++) {
                this.m_slots[i].SetOccupant(null);
            }
        }

        public ManagedLobbyMember GetLobbyMember(ulong playerID) => this.m_slots.FirstOrDefault(x => x.Occupant is not null && x.Occupant.ID == playerID)?.Occupant;

        public void TrySetMemberPosition(ManagedLobbyMember member, int position, bool silent = true) {
            if (this.m_slots.Length >= position) {
                return;
            }
            if (this.m_slots[position].Occupant is null) {
                int prev = this.m_slots.IndexOf(x => x.Occupant == member);
                if (prev != -1) {
                    this.m_slots[position].SetOccupant(member);
                    this.m_slots[prev].SetOccupant(null);
                    if (!silent) {
                        this.m_lobby.SetUserInformation(member.ID, "pos", position);
                    }
                }
            }
        }

        public static ManagedLobbyTeamType GetTeamTypeFromFaction(string faction) 
            => faction.CompareTo("german") == 0 || faction.CompareTo("west_german") == 0 ? ManagedLobbyTeamType.Axis : ManagedLobbyTeamType.Allies;

    }

}
