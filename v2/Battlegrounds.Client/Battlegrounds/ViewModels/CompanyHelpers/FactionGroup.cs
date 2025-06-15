using Battlegrounds.Models.Companies;

namespace Battlegrounds.ViewModels.CompanyHelpers;

[Obsolete("This class will not be used but is here for reference.")]
public sealed class FactionGroup(string id, string game, ICollection<Company> companies) {
    
    public string Id { get; } = id;
    
    public string GameType { get; } = game;

    public string Faction => Id;

    public ICollection<Company> Companies { get; } = companies;
    
    public override string ToString() => Id;

}
