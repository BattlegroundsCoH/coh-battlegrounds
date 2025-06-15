using Battlegrounds.Models.Companies;

namespace Battlegrounds.ViewModels.CompanyHelpers;

public sealed class GameGroup(string id, ICollection<Company> companies) {

    public string Id { get; } = id;

    public string GameType => Id;

    public string CompanyCount => companies.Count switch {
        0 => "(No companies)",
        1 => "(1 company)",
        _ => $"({companies.Count} companies)"
    };

    public ICollection<Company> Companies { get; } = [.. companies.OrderBy(x => x.Faction)];

}
