using System.Collections.Generic;
using System.Linq;

using Battlegrounds.Functional;
using Battlegrounds.Game.Database.Management.CoH2;
using Battlegrounds.Modding;

namespace Battlegrounds.Game.Database.Management.Common;

/// <summary>
/// Abstract class representing common features across gamemode lists.
/// </summary>
public abstract class CommonGamemodeList : IGamemodeList {

    /// <summary>
    /// Internal list of gamemodes
    /// </summary>
    protected readonly IList<IGamemode>? gamemodes;

    /// <summary>
    /// Create and load the <see cref="CoH2GamemodeList"/>.
    /// </summary>
    public CommonGamemodeList()
        => gamemodes = new List<IGamemode>();

    /// <inheritdoc/>
    public void AddWincondition(IGamemode gamemode) => gamemodes?.Add(gamemode);

    /// <inheritdoc/>
    public List<IGamemode> GetGamemodes(ModGuid modGuid, bool captureBaseGame = false)
        => gamemodes is not null ? gamemodes.Where(x => x.Guid == modGuid || (captureBaseGame && x.Guid == ModGuid.BaseGame)).ToList() : new();

    /// <inheritdoc/>
    public IGamemode? GetGamemodeByName(ModGuid modGuid, string gamemodeName)
        => gamemodes?.FirstOrDefault(x => x.Guid == modGuid && x.Name == gamemodeName);

    /// <inheritdoc/>
    public List<IGamemode> GetGamemodes(ModGuid modGuid, IEnumerable<string> gamemodeNames)
        => gamemodeNames.Select(x => GetGamemodeByName(modGuid, x)).NotNull().ToList();

}
