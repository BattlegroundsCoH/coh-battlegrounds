using YamlDotNet.Serialization;

namespace Battlegrounds.Core.Games.Blueprints;

public abstract class BlueprintBaseBuilder<T> where T : IBlueprint {

    public ulong Id {
        get => PropertyBagGroupId.Ppbgid;
        set => PropertyBagGroupId = new(value);
    }

    [YamlIgnore]
    public PropertyBagGroupId PropertyBagGroupId { get; set; }
    public string ReferenceId { get; set; } = string.Empty;

    public BlueprintBaseBuilder<T> SetReferenceId(string referenceId) {
        ReferenceId = referenceId;
        return this;
    }

    public abstract T Build();

}
