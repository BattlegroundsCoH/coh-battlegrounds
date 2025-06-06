using Battlegrounds.Models.Companies;

namespace Battlegrounds.ViewModels.CompanyHelpers;

public sealed class FactionGroup(string id, ICollection<Company> companies) {
    
    public string Id { get; } = id;
    
    public string Faction => Id;

    public ICollection<Company> Companies { get; } = companies;
    
    public override string ToString() => Id;

}
