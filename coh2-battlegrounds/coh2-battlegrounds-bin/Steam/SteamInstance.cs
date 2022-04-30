using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Battlegrounds.Steam;

/// <summary>
/// Class representation of a running Steam instance. Cannot be inheritted. Inherits from <see cref="IJsonObject"/>.
/// </summary>
public sealed class SteamInstance {

    // The local user to use
    private SteamUser? m_user;
    private bool m_isVerified; // TODO: Make a verify function that verifies the user registered here is the one logged in (check registry keys)

    /// <summary>
    /// Get the active <see cref="SteamUser"/> that is using this instance.
    /// </summary>
    public SteamUser User {
        get => this.m_user ?? throw new Exception("No local steam user found!");
        set => this.m_user = value;
    }

    /// <summary>
    /// Get if any <see cref="SteamUser"/> has been set.
    /// </summary>
    [JsonIgnore]
    [MemberNotNullWhen(true, nameof(User))]
    public bool HasUser => this.m_user is not null;

    /// <summary>
    /// Get if the current user has been verified as the logged in user.
    /// </summary>
    [JsonIgnore]
    public bool HasVerifiedUser => this.m_isVerified;

    /// <summary>
    /// Initialise a new <see cref="SteamInstance"/> with no user.
    /// </summary>
    public SteamInstance() {
        this.m_user = null;
        this.m_isVerified = false;
    }

    /// <summary>
    /// Initialise a new <see cref="SteamInstance"/> with a specified user.
    /// </summary>
    /// <param name="User"></param>
    [JsonConstructor]
    public SteamInstance(SteamUser User) {
        this.m_user = User;
        this.m_isVerified = false;
        Trace.WriteLine($"Found steam user '{User.Name}' ({User.ID}) as local user in local data (Unverified).", nameof(SteamInstance));
    }

    /// <summary>
    /// Get the local steam user.
    /// </summary>
    /// <returns>Will return <see langword="true"/> if a Steam user could be found. Otherwise <see langword="false"/>.</returns>
    public bool GetSteamUser() {
        SteamUser? user = FromLocalInstall();
        if (user is not null) {
            this.m_user = user;
            this.m_isVerified = true;
            return true;
        } else {
            return false;
        }
    }

    /// <summary>
    /// Verify that there's any steam instance running.
    /// </summary>
    /// <returns>Will return <see langword="true"/> if a Steam instance could be found. Otherwise <see langword="false"/>.</returns>
    public static bool IsSteamRunning => Process.GetProcessesByName("Steam").Length > 0;

    /// <summary>
    /// Fetch the <see cref="SteamUser"/> from the local instance of the Steam client.
    /// </summary>
    /// <returns>The <see cref="SteamUser"/> using the machine or null if it was not possible.</returns>
    public static SteamUser? FromLocalInstall(string? steamInstall = "") {

        // Get install path
        steamInstall = (string.IsNullOrEmpty(steamInstall) ? Pathfinder.GetOrFindSteamPath() : steamInstall).Replace("Steam.exe", "config\\loginusers.vdf");
        Trace.WriteLine($"Fetching local user data: {steamInstall}", nameof(SteamUser));

        // Get VDF
        var vdf = Vdf.FromFile(steamInstall);
        if (vdf is null) {
            Trace.WriteLine($"Failed to parse local user data in {steamInstall}", nameof(SteamUser));
            return null;
        }

        // Grab users
        var users = vdf.Table("users");

        // Loop over each
        foreach (var (k,v) in users) {
            if (v is Dictionary<string, object> entries) {
                if (entries.ContainsKey("MostRecent") && entries["MostRecent"] is "1") {
                    ulong id = ulong.Parse(k);
                    string name = (string)entries["PersonaName"];
                    return new SteamUser(id) { Name = name };
                }
            }
        }

        // Return null user
        return null;

    }

}
