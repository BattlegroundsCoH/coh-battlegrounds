using Battlegrounds.Errors.Common;
using Battlegrounds.Logging;
using Battlegrounds.Steam;

namespace Battlegrounds.Game;

/// <summary>
/// Enum representing the current game case to consider.
/// </summary>
public enum GameCase {

    /// <summary>
    /// Unpsecified (error) case.
    /// </summary>
    Unspecified,

    /// <summary>
    /// The current game case is CoH2 and actions should be performed in a CoH2 context.
    /// </summary>
    CompanyOfHeroes2,

    /// <summary>
    /// The current game case is CoH3 and actions should be performed in a CoH3 context.
    /// </summary>
    CompanyOfHeroes3,

    /// <summary>
    /// Both game cases should be considered.
    /// </summary>
    All = CompanyOfHeroes2 | CompanyOfHeroes3,

    /// <summary>
    /// Same as the 'All' case.
    /// </summary>
    Any = All

}

/// <summary>
/// Static utility class for simplifying work related to <see cref="GameCase"/> values.
/// </summary>
public static class GameCases {

    private static readonly Logger logger = Logger.CreateLogger();

    /// <summary>
    /// The Steam application Id for Company of Heroes 2.
    /// </summary>
    public const uint CoH2AppId = 231451;

    /// <summary>
    /// The Steam application Id for Company of Heroes 3.
    /// </summary>
    public const uint CoH3AppId = 1677280;

    /// <summary>
    /// Get the <see cref="GameCase"/> associated with the given <see cref="AppId"/>.
    /// </summary>
    /// <param name="appId">The application Id.</param>
    /// <returns>The <see cref="GameCase"/> associated with the <paramref name="appId"/>. <see cref="GameCase.Unspecified"/> is <paramref name="appId"/> is not valid.</returns>
    public static GameCase FromAppId(AppId appId) => appId.AppType switch {
        AppIdType.DLC => FromAppId(appId.ParentIdentifier ?? throw new ObjectPropertyNotDefinedException<AppId>(nameof(AppId.ParentIdentifier))),
        AppIdType.Game => FromAppId(appId.Identifier),
        _ => GameCase.Unspecified
    };

    /// <summary>
    /// Get the <see cref="GameCase"/> associated with the given app identifier.
    /// </summary>
    /// <param name="appId">The application Id.</param>
    /// <returns>The <see cref="GameCase"/> associated with the <paramref name="appId"/>. <see cref="GameCase.Unspecified"/> is <paramref name="appId"/> is not valid.</returns>
    public static GameCase FromAppId(uint appId) => appId switch {
        CoH2AppId => GameCase.CompanyOfHeroes2,
        CoH3AppId => GameCase.CompanyOfHeroes3,
        _ => GameCase.Unspecified
    };

}
