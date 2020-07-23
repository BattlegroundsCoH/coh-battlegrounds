using System;
using Battlegrounds.Game.Gameplay;
using Battlegrounds.Json;
using Battlegrounds.Steam;

namespace Battlegrounds.Game.Battlegrounds {
    
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
    
        /// <summary>
        /// The <see cref="SteamUser"/> profile of the human user.
        /// </summary>
        [JsonIgnoreIfValue("")] public string UserDisplayname { get; }

        /// <summary>
        /// Is the <see cref="SessionParticipant"/> instance a human player.
        /// </summary>
        public bool IsHumanParticipant { get; }

        /// <summary>
        /// The <see cref="Company"/> used by the <see cref="SessionParticipant"/>.
        /// </summary>
        public Company ParticipantCompany { get; }

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
            this.IsHumanParticipant = true;
            this.ParticipantCompany = company;
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
        public SessionParticipant(string user, Company company, SessionParticipantTeam tIndex, byte pIndex) {

            this.UserDisplayname = user;
            this.IsHumanParticipant = true;
            this.ParticipantCompany = company;
            this.Difficulty = AIDifficulty.Human;
            this.TeamIndex = tIndex;
            this.PlayerIndexOnTeam = pIndex;

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
            this.IsHumanParticipant = false;
            this.ParticipantCompany = company;
            this.Difficulty = difficulty;
            this.TeamIndex = tIndex;
            this.PlayerIndexOnTeam = pIndex;
        }

        public string ToJsonReference() => throw new NotSupportedException();

        public string GetName()
            => (this.IsHumanParticipant) ? this.UserDisplayname : this.Difficulty.GetIngameDisplayName();

        public override string ToString() 
            => $"{this.GetName()} [{this.ParticipantCompany.Name}]";

    }

}
