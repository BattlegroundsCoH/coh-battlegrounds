using Battlegrounds.Modding.Content;

namespace Battlegrounds.Modding;

public interface IModFactory {

    /// <summary>
    /// 
    /// </summary>
    ModPackage Package { get; }

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
