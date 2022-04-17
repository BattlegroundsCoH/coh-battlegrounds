using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

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
    public SteamUser User => this.m_user ?? throw new Exception("No local steam user found!");

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
    public static SteamUser? FromLocalInstall() {

        // Get install path
        string steaminstall = Pathfinder.GetOrFindSteamPath().Replace("Steam.exe", "config\\loginusers.vdf");

        // Make sure the file exists
        if (File.Exists(steaminstall)) {

            // Read all contents
            string contents = File.ReadAllText(steaminstall);

            // Run regex match
            var idCollection = Regex.Matches(contents, @"\""(?<id>\d+)\""\s*\{(?<body>(\s|\w|\d|\"")*)\}");

            // Loop through matches
            foreach (Match idMatch in idCollection) {

                // Read ID
                ulong id = ulong.Parse(idMatch.Groups["id"].Value);

                // Read name and recent
                Match name = Regex.Match(idMatch.Groups["body"].Value, @"\""PersonaName\""\s*\""(?<name>(\s|\w|\d)*)\""");
                Match recent = Regex.Match(idMatch.Groups["body"].Value, @"\""(MostRecent|mostrecent)\""\s*\""(?<recent>1|0)\""");

                // If recent, use that (Most likely the one running).
                if (recent.Groups["recent"].Value.CompareTo("1") == 0) {
                    return new SteamUser(id) { Name = name.Groups["name"].Value };
                }

            }

        }

        // Return null user
        return null;

    }

}
