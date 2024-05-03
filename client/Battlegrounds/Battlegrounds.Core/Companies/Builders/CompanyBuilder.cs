using Battlegrounds.Core.Companies.Templates;
using Battlegrounds.Core.Games.Factions;

namespace Battlegrounds.Core.Companies.Builders;

public sealed class CompanyBuilder : ICompanyBuilder {

    public Guid Id { get; set; } = Guid.NewGuid();

    public string Name { get; set; } = "Untitled Company";

    public List<ISquad> Squads { get; set; } = [];

    public IFaction Faction { get; set; } = CoH3Faction.British;

    public ICompanyTemplate Template { get; set; } = new CompanyTemplate(string.Empty, string.Empty, 0, 0, 0, 0);

    public CompanyBuilder() { }

    public CompanyBuilder(ICompany source) {
        this.Id = source.Id;
        this.Name = source.Name;
        this.Squads = new List<ISquad>(source.Squads);
    }

    public CompanyBuilder WithId(Guid id) {
        Id = id;
        return this;
    }

    public CompanyBuilder WithName(string name) {
        Name = name;
        return this;
    }

    public CompanyBuilder WithFaction(IFaction faction) {
        Faction = faction;
        return this;
    }

    public ICompany Build() {
        return new Company(Id, Name, Faction, Template, Squads, [], []);
    }

    public ICompanyBuilder AddSquad(ISquad squad) {
        int i = Squads.FindIndex(x => x.SquadId == squad.SquadId);
        if (i != -1) {
            Squads[i] = squad;
        } else {
            Squads.Add(squad);
        }
        return this;
    }

    public ICompanyBuilder AddSquad(ISquadBuilder squadBuilder) => 
        AddSquad(squadBuilder
            .Build((ushort)((Squads.Count > 0 ? Squads.Max(x => x.SquadId) : 0) + 1)));

    public CompanyBuilder WithSquad(Action<SquadBuilder> builder) {
        builder(new SquadBuilder(0, this));
        return this;
    }

    public CompanyBuilder WithTemplate(ICompanyTemplate template) {
        this.Template = template;
        return this;
    }

}
