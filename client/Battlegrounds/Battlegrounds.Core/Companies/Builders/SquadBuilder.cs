using Battlegrounds.Core.Games.Blueprints;

namespace Battlegrounds.Core.Companies.Builders;

public sealed class SquadBuilder(ushort? squadIndex, ICompanyBuilder? builder) : ISquadBuilder {

    private readonly ushort? _squadIndex = squadIndex;
    private readonly ICompanyBuilder? _companyBuilder = builder;

    public SquadBlueprint? Blueprint { get; set; } = null;

    public string? Name { get; set; } = null;

    public float Experience { get; set; } = 0f;

    public HashSet<UpgradeBlueprint> Upgrades { get; set; } = [];

    public List<IBlueprint> Items { get; set; } = [];

    public SquadBuilder(ISquad squad, ICompanyBuilder? builder = null) : this(squad.SquadId, builder) {
        this.Blueprint = squad.Blueprint;
        this.Name = squad.Name;
        this.Experience = squad.Experience;
        this.Upgrades = new HashSet<UpgradeBlueprint>(squad.Upgrades);
        this.Items = new List<IBlueprint>(squad.Items);
    }

    public ICompanyBuilder AddToCompany()
        => _companyBuilder?.AddSquad(this) ?? throw new InvalidOperationException("Cannot add squad to unspecified company");

    public ICompanyBuilder AddToCompany(int count) {
        ICompanyBuilder companyBuilder = _companyBuilder ?? throw new InvalidOperationException("Cannot add squad to unspecified company");
        for (int i = 0; i < count; i++) {
            companyBuilder = companyBuilder.AddSquad(this);
        }
        return companyBuilder;
    }

    public ISquad Build() {
        if (!_squadIndex.HasValue) {
            throw new InvalidOperationException("Cannot build a squad when no valid squad index is provided");
        }
        return Build(_squadIndex.Value);
    }

    public ISquad Build(ushort squadIndex) {
        return new CompanySquad(
            squadIndex, 
            Blueprint ?? throw new InvalidDataException("Blueprint expected while constructing company squad"), 
            Name, 
            Experience, 
            Upgrades, 
            Items);
    }

    public SquadBuilder WithBlueprint(SquadBlueprint blueprint) {
        Blueprint = blueprint;
        return this;
    }

    public SquadBuilder WithName(string? name) {
        Name = name;
        return this;
    }

    public SquadBuilder WithExperience(float experience) {
        Experience = experience;
        return this;
    }

    public SquadBuilder WithUpgrade(UpgradeBlueprint upgrade) {
        Upgrades.Add(upgrade);
        return this;
    }

    public SquadBuilder WithItem(IBlueprint itemId) {
        Items.Add(itemId);
        return this;
    }

}
