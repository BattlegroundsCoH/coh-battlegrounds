using System.Collections.Generic;

using Battlegrounds.Functional;
using Battlegrounds.Game.Gameplay;

namespace Battlegrounds.Game.Database.Extensions.RequirementTypes {

    public class RequireSlotItem : RequirementExtension {

        public bool DisplayRequirement { get; }

        public int Min { get; }

        public int Max { get; }

        public string Item { get; }

        public RequireSlotItem(string ui, RequirementReason reason, Dictionary<string, object> properties) : base(ui, reason) {
            this.DisplayRequirement = properties.GetValueOrDefault("display_requirement", false);
            this.Min = (int)properties.GetValueOrDefault("min_owned", 0.0f);
            this.Max = (int)properties.GetValueOrDefault("max_owned", 0.0f);
            this.Item = properties.GetValueOrDefault("slot_item", string.Empty);
        }

        public override bool IsTrue(Squad squad) => false;

    }

}
