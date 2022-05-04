using System;
using System.Text.Json.Serialization;

namespace Battlegrounds.Networking.Server;

/// <summary>
/// Represents the results of a match that can be interpreted by the server.
/// </summary>
public struct ServerMatchResults {
    
    /// <summary>
    /// Get the length of the match.
    /// </summary>
    [JsonIgnore]
    public TimeSpan Length { get => TimeSpan.FromTicks(this.LengthTicks); set => this.LengthTicks = value.Ticks; }

    /// <summary>
    /// Get or set the length of the match in ticks.
    /// </summary>
    public long LengthTicks { get; set; }

    /// <summary>
    /// Get or set the amount of total kills in the match.
    /// </summary>
    public uint TotalKills { get; set; }

    /// <summary>
    /// Get or set the amount of total losses in the match.
    /// </summary>
    public uint TotalLosses { get; set; }

    /// <summary>
    /// Get or set the outcome of the match.
    /// </summary>
    public ServerMatchResultsOutcome Outcome { get; set; }

    /// <summary>
    /// Get or set the played gamemode in the match.
    /// </summary>
    public string Gamemode { get; set; }

    /// <summary>
    /// Get or set the played gamemode option in the match.
    /// </summary>
    public string Option { get; set; }

    /// <summary>
    /// Get or set the played map.
    /// </summary>
    public string Map { get; set; }

}
