using System.Collections.Generic;

using Battlegrounds.Functional;
using Battlegrounds.Game.Gameplay;

namespace Battlegrounds.Game.Blueprints.Extensions.RequirementTypes;

public class RequireSlotItem : RequirementExtension
{

    public bool DisplayRequirement { get; }

    public int Min { get; }

    public int Max { get; }

    public string Item { get; }

    public RequireSlotItem(string ui, RequirementReason reason, Dictionary<string, object> properties) : base(ui, reason)
    {
        DisplayRequirement = properties.GetCastValueOrDefault("display_requirement", false);
        Min = (int)properties.GetCastValueOrDefault("min_owned", 0.0f);
        Max = (int)properties.GetCastValueOrDefault("max_owned", 0.0f);
        Item = properties.GetCastValueOrDefault("slot_item", string.Empty);
    }

    public override bool IsTrue(Squad squad) => false;

}

