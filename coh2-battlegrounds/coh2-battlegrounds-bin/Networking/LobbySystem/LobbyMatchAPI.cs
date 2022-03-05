using System;
using System.Diagnostics;
using System.Text;

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

    public LobbyMatchAPI(LobbyAPI api) {
        
        // Set internal refs
        this.m_api = api.ServerHandle;
        this.m_lobby = api;

    }

    public LobbyPlayerCompanyFile GetPlayerCompany(ulong playerID) {
        string companyFile = string.Empty;
        this.m_api.DownloadCompany(playerID, (status, data) => {
            if (status is DownloadResult.DOWNLOAD_SUCCESS) {
                companyFile = Encoding.UTF8.GetString(data);
            } else {
                Trace.WriteLine($"Failed to get company of player {playerID} ({status}).", nameof(LobbyMatchAPI));
            }
        }); // .DownloadCompany is a blocking call!
        return new(playerID, companyFile);
    }

    public bool HasAllPlayerCompanies() {

        bool All(LobbyAPIStructs.LobbyTeam team) {

            // Loop over team slots
            for (int i = 0; i < team.Slots.Length; i++) {

                // Get slot at index
                var slot = team.Slots[i];

                // Flag if consider
                var considerable = slot.IsOccupied && !slot.IsSelf() && !slot.IsAI();

                // If occupied but no company file available, return false.
                if (considerable) {

                    // Grab occupant
                    var occupant = slot.Occupant;
                    if (occupant is null) {
                        return false;
                    }

                    // Flag
                    bool hasCompany = this.m_api.PlayerHasCompany(occupant.MemberID);
                    Trace.WriteLine($"Companny Status of {occupant.MemberID} = {hasCompany}", nameof(LobbyMatchAPI));
                    if (!hasCompany) {
                        return false;
                    }

                }

            }

            // Return true --> All slots have a company or are unoccupied
            return true;

        }

        // Get
        var allies = All(this.m_lobby.Allies);
        var axis = All(this.m_lobby.Axis);

        // Log
        Trace.WriteLine($"Has players uploaded company status [Allies = {allies}; Axis = {axis}]", nameof(LobbyMatchAPI));

        // Run 'All' on allies and axis team
        return allies && axis;

    }

    public int CollectPlayerCompanies(PlayerCompanyCallback companyCallback) {

        int CollectTeam(LobbyAPIStructs.LobbyTeam team) {
            int count = 0;
            for (int i = 0; i < team.Slots.Length; i++) {

                var slot = team.Slots[i];
                if (slot.IsSelf() || slot.IsAI())
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
