using System;
using Battlegrounds.Game.Gameplay;
using Battlegrounds.Json;
using Battlegrounds.Steam;
using Battlegrounds.Game.DataCompany;

namespace Battlegrounds.Game.Match {

    /// <summary>
    /// Enum representing a team a <see cref="SessionParticipant"/> can be on.
    /// </summary>
    public enum SessionParticipantTeam : byte {

        /// <summary>
        /// Represents the allies team (Soviets, USF, UKF).
        /// </summary>
        TEAM_ALLIES,

        /// <summary>
        /// Represents the axis team (OKW, Ostheer).
        /// </summary>
        TEAM_AXIS,

    }

    /// <summary>
    /// Represents a human or AI participant in a <see cref="Session"/>. Implements <see cref="IJsonObject"/>.
    /// </summary>
    public readonly struct SessionParticipant : IJsonObject {

        private readonly Company m_company;

        /// <summary>
        /// The <see cref="SteamUser"/> profile of the human user.
        /// </summary>
        [JsonIgnoreIfValue("")] public string UserDisplayname { get; }

        /// <summary>
        /// The <see cref="SteamUser"/> profile ID.
        /// </summary>
        [JsonIgnoreIfValue(0)] public ulong UserID { get; }

        /// <summary>
        /// Is the <see cref="SessionParticipant"/> instance a human player.
        /// </summary>
        public bool IsHumanParticipant { get; }

        /// <summary>
        /// The <see cref="Company"/> used by the <see cref="SessionParticipant"/>.
        /// </summary>
        public Company ParticipantCompany => this.m_company;

        /// <summary>
        /// The faction to be used by the <see cref="SessionParticipant"/>.
        /// </summary>
        [JsonIgnore] public Faction ParticipantFaction => this.ParticipantCompany.Army;

        /// <summary>
        /// The <see cref="AIDifficulty"/> to use ingame.
        /// </summary>
        [JsonEnum(typeof(AIDifficulty))] public AIDifficulty Difficulty { get; }

        /// <summary>
        /// The index of the team this <see cref="SessionParticipant"/> is on.
        /// </summary>
        [JsonEnum(typeof(SessionParticipantTeam))] public SessionParticipantTeam TeamIndex { get; }

        /// <summary>
        /// The index of the player. Only used for AI participants. Set to 0.
        /// </summary>
        public byte PlayerIndexOnTeam { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="user"></param>
        /// <param name="company"></param>
        /// <param name="tIndex"></param>
        /// <param name="pIndex"></param>
        /// <exception cref="ArgumentNullException"/>
        public SessionParticipant(SteamUser user, Company company, SessionParticipantTeam tIndex, byte pIndex) {

            this.UserDisplayname = user.Name;
            this.UserID = user.ID;
            this.IsHumanParticipant = true;
            this.m_company = company;
            this.Difficulty = AIDifficulty.Human;
            this.TeamIndex = tIndex;
            this.PlayerIndexOnTeam = pIndex;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="user"></param>
        /// <param name="company"></param>
        /// <param name="tIndex"></param>
        /// <param name="pIndex"></param>
        public SessionParticipant(string user, ulong id, Company company, SessionParticipantTeam tIndex, byte pIndex) {

            this.UserDisplayname = user;
            this.UserID = id;
            this.IsHumanParticipant = true;
            this.m_company = company;
            this.Difficulty = AIDifficulty.Human;
            this.TeamIndex = tIndex;
            this.PlayerIndexOnTeam = pIndex;

            if (this.ParticipantCompany != null) {
                this.TeamIndex = (this.ParticipantCompany.Army.IsAllied) ? SessionParticipantTeam.TEAM_ALLIES : SessionParticipantTeam.TEAM_AXIS;
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="difficulty"></param>
        /// <param name="company"></param>
        /// <param name="tIndex"></param>
        /// <param name="pIndex"></param>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentException"/>
        public SessionParticipant(AIDifficulty difficulty, Company company, SessionParticipantTeam tIndex, byte pIndex) {

            if (difficulty == AIDifficulty.Human)
                throw new ArgumentException("Attempted to create AI participant with human settings!");

            this.UserDisplayname = string.Empty;
            this.UserID = 0;
            this.IsHumanParticipant = false;
            this.m_company = company;
            this.Difficulty = difficulty;
            this.TeamIndex = tIndex;
            this.PlayerIndexOnTeam = pIndex;

        }

        public string ToJsonReference() => throw new NotSupportedException();

        public string GetName()
            => (this.IsHumanParticipant) ? this.UserDisplayname : this.Difficulty.GetIngameDisplayName();

        public ulong GetID()
            => this.IsHumanParticipant ? this.UserID : 0;

        public override string ToString()
            => $"{this.GetName()} [{this.ParticipantCompany.Name}]";

    }

}
