using YamlDotNet.Serialization;

namespace Battlegrounds.Core.Games.Blueprints;

public sealed class SquadBlueprint(PropertyBagGroupId id, string referenceId) : Blueprint(id, referenceId) {
    
    public sealed class Builder {
        
        public ulong Id {
            get => PropertyBagGroupId.Ppbgid;
            set => PropertyBagGroupId = new(value); 
        }
        [YamlIgnore]
        public PropertyBagGroupId PropertyBagGroupId { get; set; }
        public string ReferenceId { get; set; } = string.Empty;
        
        public Builder SetReferenceId(string referenceId) {
            ReferenceId = referenceId;
            return this;
        }

        public SquadBlueprint Build() => new SquadBlueprint(PropertyBagGroupId, ReferenceId);
    }

}
