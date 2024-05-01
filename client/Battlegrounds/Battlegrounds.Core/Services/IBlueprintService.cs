namespace Battlegrounds.Core.Services;

public interface IBlueprintService {

    Task<bool> LoadBlueprintsFromStream(Stream? stream);

}
