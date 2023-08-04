using System;
using System.Text.Json.Serialization;

using Battlegrounds.Game.Gameplay;
using Battlegrounds.Steam;
using Battlegrounds.Game.DataCompany;
using Battlegrounds.AI;

namespace Battlegrounds.Game.Match;

/// <summary>
/// Represents a human or AI participant in a <see cref="Session"/>.
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
    public Company? SelectedCompany => this.m_company;

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
    /// The index of the player. Mainly used for AI participants.
    /// </summary>
    /// <remarks>
    /// Is used when fixed positions are required.
    /// </remarks>
    public byte PlayerIndexOnTeam { get; }

    /// <summary>
    /// Get the index uniquely assigned to the player for ingame action identification.
    /// </summary>
    public byte PlayerIngameIndex { get; }

    /// <summary>
    /// Initialise a new <see cref="SessionParticipant"/> instance.
    /// </summary>
    /// <param name="user">The displayname of the player</param>
    /// <param name="id">The unique Steam index of the player</param>
    /// <param name="company">The company the participant should use</param>
    /// <param name="tIndex">The team the player is on</param>
    /// <param name="pTeamIndex">The player's index on the team.</param>
    /// <param name="pIndex">The player's global index</param>
    public SessionParticipant(string user, ulong id, Company? company, ParticipantTeam tIndex, byte pTeamIndex, byte pIndex) {

        this.UserDisplayname = user;
        this.UserID = id;
        this.IsHuman = true;
        this.m_company = company;
        this.Difficulty = AIDifficulty.Human;
        this.TeamIndex = tIndex;
        this.PlayerIndexOnTeam = pTeamIndex;
        this.PlayerIngameIndex = pIndex;

        if (this.SelectedCompany is not null) {
            this.TeamIndex = (this.SelectedCompany.Army.IsAllied) ? ParticipantTeam.TEAM_ALLIES : ParticipantTeam.TEAM_AXIS;
        }

    }

    /// <summary>
    /// Initialise a new <see cref="SessionParticipant"/> instance.
    /// </summary>
    /// <param name="difficulty">The difficulty of the player. Should never be human.</param>
    /// <param name="company">The company the participant should use</param>
    /// <param name="tIndex">The team the player is on</param>
    /// <param name="pTeamIndex">The player's index on the team.</param>
    /// <param name="pIndex">The player's global index</param>
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

    /// <summary>
    /// Get the name of the participant
    /// </summary>
    /// <returns>The name of the participant</returns>
    public string GetName()
        => (this.IsHuman) ? this.UserDisplayname : this.Difficulty.GetIngameDisplayName();

    /// <summary>
    /// Get the unique Steam id of the participant.
    /// </summary>
    /// <returns>The unique Steam id of the participant</returns>
    public ulong GetId()
        => this.IsHuman ? this.UserID : 0;

    /// <inheritdoc/>
    public override string ToString()
        => $"{this.GetName()} [{this.SelectedCompany?.Name}]";

}
