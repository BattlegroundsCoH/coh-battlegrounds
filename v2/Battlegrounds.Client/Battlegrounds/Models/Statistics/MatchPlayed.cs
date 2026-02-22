namespace Battlegrounds.Models.Statistics;

public sealed class MatchPlayed {

    public string MatchId { get; set; } = string.Empty;

    public DateTime DatePlayed { get; set; }

    public TimeSpan Duration { get; set; }

    public string PlayerCompanyId { get; set; } = string.Empty;

    public int CompanyVersion { get; set; }

    public string PlayerFaction { get; set; } = string.Empty;

    public string PlayedMap { get; set; } = string.Empty;

    public bool IsVictory { get; set; }

    public bool IsSinglePlayer { get; set; }

    public int TotalLosses { get; set; }

    public int TotalKills { get; set; }

    public string ClientVersion { get; set; } = string.Empty;

}
