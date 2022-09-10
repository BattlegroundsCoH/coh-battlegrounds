using System.Collections.Generic;

using Battlegrounds.Functional;
using Battlegrounds.Game.Gameplay;

namespace Battlegrounds.Game.Database.Extensions.RequirementTypes;

public class RequireInTerritory : RequirementExtension {

    public bool InSupply { get; }

    public bool IsSecured { get; }

    public bool NotInTransition { get; }

    public RequireInTerritory(string ui, RequirementReason reason, Dictionary<string, object> properties) : base(ui, reason) {
        this.InSupply = properties.GetCastValueOrDefault("in_supply", false);
        this.IsSecured = properties.GetCastValueOrDefault("is_secured", false);
        this.NotInTransition = properties.GetCastValueOrDefault("not_in_transition", false);
    }

    public override bool IsTrue(Squad squad) => false;

}
