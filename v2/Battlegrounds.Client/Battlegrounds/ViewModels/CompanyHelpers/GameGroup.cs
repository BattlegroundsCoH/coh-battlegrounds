using Battlegrounds.Models.Companies;

namespace Battlegrounds.ViewModels.CompanyHelpers;

public sealed class GameGroup(string id, ICollection<Company> companies) {

    public string Id { get; } = id;

    public string GameType => Id;

    public ICollection<Company> Companies { get; } = companies;

    public ICollection<FactionGroup> Factions { get; } = 
        [..from company in companies
        group company by company.Faction into factionGroup
        select new FactionGroup(factionGroup.Key, [.. factionGroup])];

}
