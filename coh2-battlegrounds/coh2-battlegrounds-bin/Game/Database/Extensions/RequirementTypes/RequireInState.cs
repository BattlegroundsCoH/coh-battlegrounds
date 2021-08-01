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
            this.InCombatSince = properties.GetValueOrDefault("in_combat_since", 0.0f);
            this.IsGarrisoned = properties.GetValueOrDefault("garrisoned", false);
            this.IsPinned = properties.GetValueOrDefault("pinned", false);
            this.IsSuppressed = properties.GetValueOrDefault("suppressed", false);
            this.IsNotMoving = properties.GetValueOrDefault("not_moving", false);
            this.IsNotRetreating = properties.GetValueOrDefault("not_retreating", false);
            this.IsCamouflaged = properties.GetValueOrDefault("camouflaged", false);
        }

        public override bool IsTrue(Squad squad) => false;

    }

}
