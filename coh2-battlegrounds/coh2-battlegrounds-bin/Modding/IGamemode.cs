using System.Collections.Generic;

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
/// Possible input types for an <see cref="IGamemodeAuxiliaryOption"/>.
/// </summary>
public enum AuxiliaryOptionType {

    /// <summary>
    /// Dropdown of multiple options.
    /// </summary>
    Dropdown,

    /// <summary>
    /// Slider between a min and max value.
    /// </summary>
    Slider

}

/// <summary>
/// Interface representing an auxiliary option for a <see cref="IGamemode"/>.
/// </summary>
public interface IGamemodeAuxiliaryOption {

    /// <summary>
    /// Get the name of the option
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Get the title of the option.
    /// </summary>
    UcsString Title { get; }

    /// <summary>
    /// Get the description of the opton.
    /// </summary>
    UcsString Description { get; }

    /// <summary>
    /// Get the format to display a value in.
    /// </summary>
    /// <remarks>
    /// Not valid for <see cref="AuxiliaryOptionType.Dropdown"/>.
    /// </remarks>
    UcsString Format { get; }

    /// <summary>
    /// Get the input type.
    /// </summary>
    AuxiliaryOptionType OptionInputType { get; }

    /// <summary>
    /// Get the options for dropdown input.
    /// </summary>
    IGamemodeOption[] Options { get; }

    /// <summary>
    /// Get a numeric values for control input.
    /// </summary>
    /// <param name="num">The name of the desired number</param>
    /// <returns>The stored integer</returns>
    int GetNumber(string num);

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
    /// Get the array of supporting options that are also available in the <see cref="IGamemode"/>.
    /// </summary>
    IGamemodeAuxiliaryOption[] AuxiliaryOptions { get; }

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

    /// <summary>
    /// Get if the <see cref="IGamemode"/> requires fixed positions to be played.
    /// </summary>
    bool RequireFixed { get; }

    /// <summary>
    /// Get if the <see cref="IGamemode"/> includes a planning phase.
    /// </summary>
    bool HasPlanning { get; }

    /// <summary>
    /// Get a dictionary of all available planning entities.
    /// </summary>
    Dictionary<string, object[]> PlannableEntities { get; }

    /// <summary>
    /// Get the name of the first team.
    /// </summary>
    string? TeamName1 { get; }

    /// <summary>
    /// Get the name of the second team.
    /// </summary>
    string? TeamName2 { get; }

}
