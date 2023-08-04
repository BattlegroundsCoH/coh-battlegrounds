using System.Collections.Generic;

using Battlegrounds.Functional;
using Battlegrounds.Game.Gameplay;

namespace Battlegrounds.Game.Blueprints.Extensions.RequirementTypes;

public class RequireInState : RequirementExtension
{

    public float InCombatSince { get; }

    public bool IsGarrisoned { get; }

    public bool IsPinned { get; }

    public bool IsSuppressed { get; }

    public bool IsNotMoving { get; }

    public bool IsNotRetreating { get; }

    public bool IsCamouflaged { get; }

    public RequireInState(string ui, RequirementReason reason, Dictionary<string, object> properties) : base(ui, reason)
    {
        InCombatSince = properties.GetCastValueOrDefault("in_combat_since", 0.0f);
        IsGarrisoned = properties.GetCastValueOrDefault("garrisoned", false);
        IsPinned = properties.GetCastValueOrDefault("pinned", false);
        IsSuppressed = properties.GetCastValueOrDefault("suppressed", false);
        IsNotMoving = properties.GetCastValueOrDefault("not_moving", false);
        IsNotRetreating = properties.GetCastValueOrDefault("not_retreating", false);
        IsCamouflaged = properties.GetCastValueOrDefault("camouflaged", false);
    }

    public override bool IsTrue(Squad squad) => false;

}

