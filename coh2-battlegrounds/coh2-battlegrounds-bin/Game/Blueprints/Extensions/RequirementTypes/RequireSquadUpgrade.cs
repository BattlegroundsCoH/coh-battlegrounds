using System.Collections.Generic;
using System.Linq;

using Battlegrounds.Game.Gameplay;

namespace Battlegrounds.Game.Blueprints.Extensions.RequirementTypes;

public class RequireSquadUpgrade : RequirementExtension
{

    public string Upgrade { get; }

    public bool IsPresent { get; }

    public RequireSquadUpgrade(string ui, RequirementReason reason, Dictionary<string, object> properties) : base(ui, reason)
    {
        Upgrade = (string)properties.GetValueOrDefault("upgrade_name", string.Empty);
        IsPresent = (bool)properties.GetValueOrDefault("is_present", false);
    }

    public override bool IsTrue(Squad squad)
        => squad.Upgrades.Any(x => x.Name == Upgrade) == IsPresent;


}

