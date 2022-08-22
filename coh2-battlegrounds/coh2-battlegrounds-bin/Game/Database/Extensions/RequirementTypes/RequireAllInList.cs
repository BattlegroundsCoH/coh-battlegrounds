using System;
using System.Collections.Generic;
using System.Linq;

using Battlegrounds.Game.Gameplay;

namespace Battlegrounds.Game.Database.Extensions.RequirementTypes;

public class RequireAllInList : RequirementExtension, IRequirementList {

    public RequirementExtension[] Requirements { get; }

    public RequireAllInList(string ui, RequirementReason reason, Dictionary<string, object> properties) : base(ui, reason) {
        if (properties.TryGetValue("requirements", out object? reqs) && reqs is List<object> table) {
            List<RequirementExtension> all = new();
            foreach (Dictionary<string, object> entry in table) {
                all.Add(RequirementExtensionReader.CreateRequirement(entry));
            }
            this.Requirements = all.ToArray();
        } else {
            this.Requirements = Array.Empty<RequirementExtension>();
        }
    }

    public override bool IsTrue(Squad squad) => this.Requirements.All(x => x.IsTrue(squad));

}

