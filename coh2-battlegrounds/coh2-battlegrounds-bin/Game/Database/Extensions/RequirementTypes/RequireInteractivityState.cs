using System.Collections.Generic;

using Battlegrounds.Game.Gameplay;

namespace Battlegrounds.Game.Database.Extensions.RequirementTypes;

public class RequireInteractivityState : RequirementExtension {

    public RequireInteractivityState(string ui, RequirementReason reason, Dictionary<string, object> properties) : base(ui, reason) { }

    public override bool IsTrue(Squad squad) => false;

}

