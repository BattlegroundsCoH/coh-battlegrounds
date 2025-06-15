using Battlegrounds.Modding;

namespace Battlegrounds.Lobby.Playing;

/// <summary>
/// A delegate that handles the cancellation of the game preparation process.
/// </summary>
/// <param name="model">The play model that is being prepared.</param>
public delegate void PrepareCancelHandler(object model);

/// <summary>
/// A delegate that handles the event when game preparation is completed.
/// </summary>
/// <param name="model">The play model that was prepared.</param>
public delegate void PrepareOverHandler(IPlayModel model);

/// <summary>
/// A delegate that handles the event when the play sequence is over.
/// </summary>
/// <param name="model">The play model that was in play.</param>
public delegate void PlayOverHandler(IPlayModel model);

/// <summary>
/// A delegate that handles the startup sequence of a game.
/// </summary>
public delegate void GameStartupHandler();

/// <summary>
/// Defines the operations for a play model.
/// </summary>
public interface IPlayModel {

    /// <summary>
    /// Prepares the play model for gameplay using a mod package. 
    /// Calls handlers when the preparation is either completed or cancelled.
    /// </summary>
    /// <param name="modPackage">The mod package to use for preparation.</param>
    /// <param name="prepareOver">Handler to call when the preparation is over.</param>
    /// <param name="prepareCancel">Handler to call when the preparation is cancelled.</param>
    void Prepare(IModPackage modPackage, PrepareOverHandler prepareOver, PrepareCancelHandler prepareCancel);

    /// <summary>
    /// Starts the gameplay for the play model.
    /// Calls handlers when the game startup is complete and when the match is over.
    /// </summary>
    /// <param name="startupHandler">Handler to call on game startup. Null if there is no specific startup action.</param>
    /// <param name="matchOver">Handler to call when the match is over.</param>
    void Play(GameStartupHandler? startupHandler, PlayOverHandler matchOver);

}
