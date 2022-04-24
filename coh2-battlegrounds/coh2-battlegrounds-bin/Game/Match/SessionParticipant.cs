using System;
using System.Text.Json.Serialization;

using Battlegrounds.Game.Gameplay;
using Battlegrounds.Steam;
using Battlegrounds.Game.DataCompany;

namespace Battlegrounds.Game.Match {

    /// <summary>
    /// Represents a human or AI participant in a <see cref="Session"/>. Implements <see cref="IJsonObject"/>.
    /// </summary>
    public readonly struct SessionParticipant : ISessionParticipant { // Maybe change to class, now that we implement an interface

        private readonly Company? m_company;

        /// <summary>
        /// The <see cref="SteamUser"/> profile of the human user.
        /// </summary>
        public string UserDisplayname { get; }

        /// <summary>
        /// The <see cref="SteamUser"/> profile ID.
        /// </summary>
        public ulong UserID { get; }

        /// <summary>
        /// Is the <see cref="SessionParticipant"/> instance a human player.
        /// </summary>
        public bool IsHuman { get; }

        /// <summary>
        /// The <see cref="Company"/> used by the <see cref="SessionParticipant"/>.
        /// </summary>
        public Company SelectedCompany => this.m_company ?? throw new Exception("Selected company is undefined.");

        /// <summary>
        /// The faction to be used by the <see cref="SessionParticipant"/>.
        /// </summary>
        [JsonIgnore] public Faction ParticipantFaction => this.SelectedCompany?.Army ?? throw new Exception("Company undefined and faction is, therefore, unknown.");

        /// <summary>
        /// The <see cref="AIDifficulty"/> to use ingame.
        /// </summary>
        public AIDifficulty Difficulty { get; }

        /// <summary>
        /// The index of the team this <see cref="SessionParticipant"/> is on.
        /// </summary>
        public ParticipantTeam TeamIndex { get; }

        /// <summary>
        /// The index of the player. Only used for AI participants. Set to 0.
        /// </summary>
        public byte PlayerIndexOnTeam { get; }

        /// <summary>
        /// Get the index uniquely assigned to the player for ingame action identification.
        /// </summary>
        public byte PlayerIngameIndex { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="user"></param>
        /// <param name="company"></param>
        /// <param name="tIndex"></param>
        /// <param name="pIndex"></param>
        public SessionParticipant(string user, ulong id, Company? company, ParticipantTeam tIndex, byte pTeamIndex, byte pIndex) {

            this.UserDisplayname = user;
            this.UserID = id;
            this.IsHuman = true;
            this.m_company = company;
            this.Difficulty = AIDifficulty.Human;
            this.TeamIndex = tIndex;
            this.PlayerIndexOnTeam = pTeamIndex;
            this.PlayerIngameIndex = pIndex;

            if (this.SelectedCompany != null) {
                this.TeamIndex = (this.SelectedCompany.Army.IsAllied) ? ParticipantTeam.TEAM_ALLIES : ParticipantTeam.TEAM_AXIS;
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="difficulty"></param>
        /// <param name="company"></param>
        /// <param name="tIndex"></param>
        /// <param name="pIndex"></param>
        /// <exception cref="ArgumentException"/>
        public SessionParticipant(AIDifficulty difficulty, Company? company, ParticipantTeam tIndex, byte pTeamIndex, byte pIndex) {

            if (difficulty == AIDifficulty.Human)
                throw new ArgumentException("Attempted to create AI participant with human settings!", nameof(difficulty));

            this.UserDisplayname = string.Empty;
            this.UserID = 0;
            this.IsHuman = false;
            this.m_company = company;
            this.Difficulty = difficulty;
            this.TeamIndex = tIndex;
            this.PlayerIndexOnTeam = pTeamIndex;
            this.PlayerIngameIndex = pIndex;

        }

        public string GetName()
            => (this.IsHuman) ? this.UserDisplayname : this.Difficulty.GetIngameDisplayName();

        public ulong GetID()
            => this.IsHuman ? this.UserID : 0;

        public override string ToString()
            => $"{this.GetName()} [{this.SelectedCompany.Name}]";

    }

}
