using Battlegrounds.Game.DataSource;

namespace Battlegrounds.Modding;

/// <summary>
/// Interface representing an option selectable in an <see cref="IGamemode"/>.
/// </summary>
public interface IGamemodeOption {

    /// <summary>
    /// The display title of the option.
    /// </summary>
    UcsString Title { get; }

    /// <summary>
    /// The backing value of the option.
    /// </summary>
    int Value { get; }

}

/// <summary>
/// Interface representing a gamemode that can by played.
/// </summary>
public interface IGamemode {

    /// <summary>
    /// Get the identifier name of the gamemode.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Get the GUID associated with this gamemode.
    /// </summary>
    ModGuid Guid { get; }

    /// <summary>
    /// Get the array of <see cref="IGamemodeOption"/> that are available to the <see cref="IGamemode"/>.
    /// </summary>
    IGamemodeOption[] Options { get; }

    /// <summary>
    /// Get the index of the default option.
    /// </summary>
    int DefaultOptionIndex { get; }

    /// <summary>
    /// Get the name that is displayed when selecting the gamemode.
    /// </summary>
    UcsString DisplayName { get; }

    /// <summary>
    /// Get the short display description of the gamemode.
    /// </summary>
    UcsString DisplayShortDescription { get; }

}
