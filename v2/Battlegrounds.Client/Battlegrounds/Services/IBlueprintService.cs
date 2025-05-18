using Battlegrounds.Models.Blueprints;
using Battlegrounds.Models.Playing;

namespace Battlegrounds.Services;

public interface IBlueprintService {
    
    bool IsLoaded { get; }

    T2 GetBlueprint<T1, T2>(string blueprintId) 
        where T1 : Game 
        where T2 : Blueprint;

    void LoadBlueprints();

}
