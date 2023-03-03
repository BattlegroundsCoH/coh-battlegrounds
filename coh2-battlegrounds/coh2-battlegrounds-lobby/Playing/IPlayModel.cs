using Battlegrounds.Modding;

namespace Battlegrounds.Lobby.Playing;

/// <summary>
/// 
/// </summary>
/// <param name="model"></param>
public delegate void PrepareCancelHandler(object model);

/// <summary>
/// 
/// </summary>
/// <param name="model"></param>
public delegate void PrepareOverHandler(IPlayModel model);

/// <summary>
/// 
/// </summary>
/// <param name="model"></param>
public delegate void PlayOverHandler(IPlayModel model);

/// <summary>
/// 
/// </summary>
public delegate void GameStartupHandler();

/// <summary>
/// 
/// </summary>
public interface IPlayModel {

    /// <summary>
    /// 
    /// </summary>
    /// <param name="modPackage"></param>
    /// <param name="prepareOver"></param>
    /// <param name="prepareCancel"></param>
    void Prepare(ModPackage modPackage, PrepareOverHandler prepareOver, PrepareCancelHandler prepareCancel);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="startupHandler"></param>
    /// <param name="matchOver"></param>
    void Play(GameStartupHandler? startupHandler, PlayOverHandler matchOver);

}
