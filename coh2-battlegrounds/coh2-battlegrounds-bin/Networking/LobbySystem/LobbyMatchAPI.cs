using System;
using System.Collections.Generic;

using Battlegrounds.Networking.Server;

namespace Battlegrounds.Networking.LobbySystem;

/// <summary>
/// Represents a company file paired with the company owner.
/// </summary>
public readonly struct LobbyPlayerCompanyFile {

    /// <summary>
    /// The ID of the player owning the company.
    /// </summary>
    public readonly ulong playerID;

    /// <summary>
    /// The json data representing the company.
    /// </summary>
    public readonly string playerCompanyData;

    /// <summary>
    /// Initialize a new <see cref="LobbyPlayerCompanyFile"/> object.
    /// </summary>
    /// <param name="pid">The ID of the player owning the company.</param>
    /// <param name="company">The json data representing the company.</param>
    public LobbyPlayerCompanyFile(ulong pid, string company) {
        this.playerID = pid;
        this.playerCompanyData = company;
    }

}

/// <summary>
/// 
/// </summary>
/// <param name="playerCompanyFile"></param>
public delegate void PlayerCompanyCallback(LobbyPlayerCompanyFile playerCompanyFile);

public class LobbyMatchAPI {

    private readonly ServerAPI m_api;
    private readonly LobbyAPI m_lobby;
    private readonly int m_humans;

    private readonly HashSet<ulong> m_gamemodeReceived;
    private readonly HashSet<ulong> m_resultsReceived;

    public LobbyMatchAPI(LobbyAPI api) {
        
        // Set internal refs
        this.m_api = api.ServerHandle;
        this.m_lobby = api;
        this.m_humans = (int)this.m_lobby.GetPlayerCount();

        // Create hash sets
        this.m_gamemodeReceived = new();
        this.m_resultsReceived = new();

    }

    public LobbyPlayerCompanyFile GetPlayerCompany(ulong playerID)
            => new LobbyPlayerCompanyFile(playerID, this.m_lobby.Self.ID == playerID ? throw new NotImplementedException() : this.m_api.DownloadCompany(playerID));

    public bool HasAllPlayerCompanies() {

        bool All(LobbyAPIStructs.LobbyTeam team) {

            // Loop over team slots
            for (int i = 0; i < team.Slots.Length; i++) {

                // Get slot at index
                var slot = team.Slots[i];

                // If occupied but no company file available, return false.
                if (slot.State == 1 && !this.m_api.PlayerHasCompany(slot.Occupant.MemberID)) {
                    return false;
                }

            }

            // Return true --> All slots have a company or are unoccupied
            return true;

        }

        // Run 'All' on allies and axis team
        return All(this.m_lobby.Allies) && All(this.m_lobby.Axis);

    }

    public int CollectPlayerCompanies(PlayerCompanyCallback companyCallback) {

        int CollectTeam(LobbyAPIStructs.LobbyTeam team) {
            int count = 0;
            for (int i = 0; i < team.Slots.Length; i++) {

                var slot = team.Slots[i];
                if (slot.IsSelf())
                    continue;
                if (slot.IsAI())
                    continue;

                if (team.Slots[i].Occupant is LobbyAPIStructs.LobbyMember member) {
                    companyCallback?.Invoke(this.GetPlayerCompany(member.MemberID));
                    count++;
                }

            }
            return count;
        }

        return CollectTeam(this.m_lobby.Allies) + CollectTeam(this.m_lobby.Axis);

    }

}
