using Battlegrounds.Core.Games.Blueprints;

namespace Battlegrounds.Core.Services;

public interface IBlueprintService {

    IBlueprint? GetBlueprintById(string game, PropertyBagGroupId propertyBagGroupId);

    TBlueprint? GetBlueprintById<TBlueprint>(string game, PropertyBagGroupId propertyBagGroupId) where TBlueprint : class, IBlueprint;

    Task<bool> LoadBlueprintsFromStream(Stream? stream);

}
