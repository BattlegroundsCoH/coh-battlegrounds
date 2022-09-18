using System;
using System.Collections.Generic;

using Battlegrounds.Game.DataSource;

namespace Battlegrounds.Modding;

/// <summary>
/// Readonly struct representing an option available in a <see cref="IWinconditionMod"/>.
/// </summary>
public readonly struct WinconditionOption : IGamemodeOption {

    /// <summary>
    /// The display title of the option.
    /// </summary>
    public UcsString Title { get; }

    /// <summary>
    /// The backing value of the option.
    /// </summary>
    public int Value { get; }

    /// <summary>
    /// Initialize a new <see cref="WinconditionOption"/> with a display name and backing value.
    /// </summary>
    /// <param name="title">The name to display when picking the option.</param>
    /// <param name="val">The integer backing value that is used to represent the option in code.</param>
    public WinconditionOption(string title, int val) {
        this.Title = UcsString.CreateLocString(title);
        this.Value = val;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="title"></param>
    /// <param name="val"></param>
    public WinconditionOption(UcsString title, int val) {
        this.Title = title;
        this.Value = val;
    }

    public void Deconstruct(out UcsString title, out int value) {
        title = this.Title;
        value = this.Value;
    }

    public override string ToString() => this.Title;

}

public readonly struct WinconditionSliderOption : IGamemodeAuxiliaryOption {

    public string Name { get; init; }

    public UcsString Title { get; init; }

    public UcsString Description { get; init; }

    public AuxiliaryOptionType OptionInputType => AuxiliaryOptionType.Slider;

    /// <summary>
    /// 
    /// </summary>
    public IGamemodeOption[] Options => Array.Empty<IGamemodeOption>();

    public int Minimum { get; init; }

    public int Maximum { get; init; }

    public int Step { get; init; }

    public int Default { get; init; }

    public UcsString Format { get; init; }

    /// <summary>
    /// Get a numeric value stored by the 
    /// </summary>
    /// <param name="num"></param>
    /// <returns></returns>
    public int GetNumber(string num) => num switch {
        "min" => this.Minimum,
        "max" => this.Maximum,
        "step" => this.Step,
        "def" => this.Default,
        _ => -1
    };

}

public readonly struct WinconditionDropdownOption : IGamemodeAuxiliaryOption {

    public string Name { get; init; }

    public UcsString Title { get; init; }

    public UcsString Description { get; init; }

    public UcsString Format { get; init; }

    public AuxiliaryOptionType OptionInputType => AuxiliaryOptionType.Dropdown;

    public int Default { get; init; }

    /// <summary>
    /// 
    /// </summary>
    public IGamemodeOption[] Options { get; init; }

    /// <summary>
    /// Get a numeric value stored by the 
    /// </summary>
    /// <param name="num"></param>
    /// <returns></returns>
    public int GetNumber(string num) => num switch {
        "def" => this.Default,
        _ => -1
    };

}

/// <summary>
/// Default Battlegrounds <see cref="IGamemode"/> implementation.
/// </summary>
public sealed class Wincondition : IGamemode {

    /// <summary>
    /// Get the name of the wincondition.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Get the associated wincondition <see cref="ModGuid"/>.
    /// </summary>
    public ModGuid Guid { get; }

    /// <summary>
    /// Get or initialise the base options of the wincondition.
    /// </summary>
    public IGamemodeOption[] Options { get; init; }

    /// <summary>
    /// Get or initialise the array of supporting options.
    /// </summary>
    public IGamemodeAuxiliaryOption[] AuxiliaryOptions { get; init; }

    /// <summary>
    /// Get or set the default base option index.
    /// </summary>
    public int DefaultOptionIndex { get; set; }

    /// <summary>
    /// Get or initialise the display name <see cref="UcsString"/> for the wincondition.
    /// </summary>
    public UcsString DisplayName { get; init; }

    /// <summary>
    /// Get or initialise the display description <see cref="UcsString"/> for the wincondition.
    /// </summary>
    public UcsString DisplayShortDescription { get; init; }

    /// <summary>
    /// Get or initialise if the wincondition requires fixed positions.
    /// </summary>
    public bool RequireFixed { get; init; }

    /// <summary>
    /// Get or initialise if the wincondition includes a planning phase.
    /// </summary>
    public bool HasPlanning { get; init; }

    /// <summary>
    /// Get or initialise the plannable entities for the wincondition.
    /// </summary>
    public Dictionary<string, object[]> PlannableEntities { get; init; }

    /// <summary>
    /// Get or initialise the display name of team 1.
    /// </summary>
    public string? TeamName1 { get; init; }

    /// <summary>
    /// Get or initialise the display name of team 2.
    /// </summary>
    public string? TeamName2 { get; init; }

    /// <summary>
    /// Get or initialise the include files
    /// </summary>
    public string[] IncludeFiles { get; init; }

    /// <summary>
    /// Initialise a new <see cref="Wincondition"/> instance.
    /// </summary>
    /// <param name="name">The name of the wincondition.</param>
    /// <param name="guid">The gamemode guid associated with the wincondition.</param>
    public Wincondition(string name, ModGuid guid) {
        this.Name = name;
        this.Guid = guid;
        this.DefaultOptionIndex = 0;
        this.DisplayName = UcsString.CreateLocString(name);
        this.Options = Array.Empty<IGamemodeOption>();
        this.AuxiliaryOptions = Array.Empty<IGamemodeAuxiliaryOption>();
        this.IncludeFiles = Array.Empty<string>();
        this.PlannableEntities = new();
    }

    public override string ToString() => this.DisplayName;

}
