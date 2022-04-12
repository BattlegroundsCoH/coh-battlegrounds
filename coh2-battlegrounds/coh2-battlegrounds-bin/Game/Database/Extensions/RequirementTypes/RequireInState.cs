using System.Collections.Generic;

using Battlegrounds.Functional;
using Battlegrounds.Game.Gameplay;

namespace Battlegrounds.Game.Database.Extensions.RequirementTypes {

    public class RequireInState : RequirementExtension {

        public float InCombatSince { get; }

        public bool IsGarrisoned { get; }

        public bool IsPinned { get; }

        public bool IsSuppressed { get; }

        public bool IsNotMoving { get; }

        public bool IsNotRetreating { get; }

        public bool IsCamouflaged { get; }

        public RequireInState(string ui, RequirementReason reason, Dictionary<string, object> properties) : base(ui, reason) {
            this.InCombatSince = properties.GetCastValueOrDefault("in_combat_since", 0.0f);
            this.IsGarrisoned = properties.GetCastValueOrDefault("garrisoned", false);
            this.IsPinned = properties.GetCastValueOrDefault("pinned", false);
            this.IsSuppressed = properties.GetCastValueOrDefault("suppressed", false);
            this.IsNotMoving = properties.GetCastValueOrDefault("not_moving", false);
            this.IsNotRetreating = properties.GetCastValueOrDefault("not_retreating", false);
            this.IsCamouflaged = properties.GetCastValueOrDefault("camouflaged", false);
        }

        public override bool IsTrue(Squad squad) => false;

    }

}
