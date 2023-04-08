using Battlegrounds.Modding.Content;

namespace Battlegrounds.Modding;

/// <summary>
/// Interface for a factory that generates specific mod instances for a <see cref="ModPackage"/>.
/// </summary>
public interface IModFactory {

    /// <summary>
    /// Get the mod package the factory creates mod instances for.
    /// </summary>
    IModPackage Package { get; }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="gamemode"></param>
    /// <returns></returns>
    IGamemode GetGamemode(Gamemode gamemode);

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    IWinconditionMod GetWinconditionMod();

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    ITuningMod GetTuning();

}
