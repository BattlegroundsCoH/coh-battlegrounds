using System;
using System.Collections.Generic;
using System.Linq;

using Battlegrounds.Game.Gameplay;

namespace Battlegrounds.Game.Blueprints.Extensions.RequirementTypes;

public class RequireNot : RequirementExtension, IRequirementList
{

    public RequirementExtension[] Requirements { get; }

    public RequireNot(string ui, RequirementReason reason, Dictionary<string, object> properties) : base(ui, reason)
    {
        if (properties.TryGetValue("requirement_table", out object? reqs) && reqs is List<object> table)
        {
            List<RequirementExtension> all = new();
            foreach (Dictionary<string, object> entry in table)
            {
                all.Add(RequirementExtensionReader.CreateRequirement(entry));
            }
            Requirements = all.ToArray();
        }
        else
        {
            Requirements = Array.Empty<RequirementExtension>();
        }
    }

    public override bool IsTrue(Squad squad) => !Requirements.All(x => x.IsTrue(squad));

}

