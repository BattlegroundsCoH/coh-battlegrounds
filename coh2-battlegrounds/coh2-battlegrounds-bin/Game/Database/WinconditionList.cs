using System.Collections.Generic;
using System.Linq;

using Battlegrounds.Functional;
using Battlegrounds.Game.Database.Management;
using Battlegrounds.Modding;

namespace Battlegrounds.Game.Database;

/// <summary>
/// 
/// </summary>
public sealed class WinconditionList : IWinconditionList {

    private readonly List<IGamemode>? winconditions;

    /// <summary>
    /// 
    /// </summary>
    public WinconditionList() { 
        winconditions = new List<IGamemode>();
    }

    /// <inheritdoc/>
    public void AddWincondition(IGamemode wincondition) => winconditions?.Add(wincondition);

    /// <inheritdoc/>
    public List<IGamemode> GetGamemodes(ModGuid modGuid, bool captureBaseGame = false)
        => winconditions is not null ? winconditions.Where(x => x.Guid == modGuid || (captureBaseGame && x.Guid == ModGuid.BaseGame)).ToList() : new();

    /// <inheritdoc/>
    public IGamemode? GetGamemodeByName(ModGuid modGuid, string gamemodeName)
        => winconditions?.FirstOrDefault(x => x.Guid == modGuid && x.Name == gamemodeName);

    /// <inheritdoc/>
    public List<IGamemode> GetGamemodes(ModGuid modGuid, IEnumerable<string> gamemodeNames)
        => gamemodeNames.Select(x => GetGamemodeByName(modGuid, x)).NotNull().ToList();

}
