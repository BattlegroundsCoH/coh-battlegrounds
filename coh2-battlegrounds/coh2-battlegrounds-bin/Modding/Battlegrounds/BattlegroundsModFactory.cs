using System;
using System.Collections.Generic;

using Battlegrounds.Functional;
using Battlegrounds.Game.DataSource;
using Battlegrounds.Modding.Content;

namespace Battlegrounds.Modding.Battlegrounds;

public sealed class BattlegroundsModFactory : IModFactory {

    public ModPackage Package { get; }

    private readonly UcsFile? m_gamemodelocale;

    public BattlegroundsModFactory(ModPackage package) {
        this.Package = package;
        this.m_gamemodelocale = package.GetLocale(ModType.Gamemode, BattlegroundsInstance.Localize.Language);
    }

    public IGamemode GetGamemode(Gamemode gamemode) {
        
        // Grab options
        var options = gamemode.Options switch {
            Gamemode.GamemodeOption[] y => y.Map(this.GetGamemodeOption),
            _ => Array.Empty<IGamemodeOption>()
        };

        // Grab aux options
        var axusOptions = gamemode.AdditionalOptions switch {
            Dictionary<string, Gamemode.GamemodeAdditionalOption> some => some.Map(this.GetAuxGamemodeOption),
            _ => Array.Empty<IGamemodeAuxiliaryOption>()
        };

        // Declare name and desc
        UcsString name = UcsString.None;
        UcsString desc = UcsString.None;

        // Try get name
        if (uint.TryParse(gamemode.Display, out uint nameKey)) {
            name = this.m_gamemodelocale?.GetRef(nameKey) ?? new();
        }

        // Try get description
        if (uint.TryParse(gamemode.DisplayDesc, out uint descKey)) {
            desc = this.m_gamemodelocale?.GetRef(descKey) ?? new();
        }

        // Return wincondition
        return new Wincondition(gamemode.ID, this.Package.GamemodeGUID) {
            Options = options,
            DefaultOptionIndex = gamemode.DefaultOption,
            AuxiliaryOptions = axusOptions,
            DisplayName = name,
            DisplayShortDescription = desc,
            HasPlanning = gamemode.Planning,
            RequireFixed = gamemode.FixedPosition,
            TeamName1 = gamemode.TeamNames?.GetOrDefault("1", string.Empty) ?? string.Empty,
            TeamName2 = gamemode.TeamNames?.GetOrDefault("2", string.Empty) ?? string.Empty,
            IncludeFiles = gamemode.Files
        };

    }

    private IGamemodeOption GetGamemodeOption(Gamemode.GamemodeOption option) {
        if (this.m_gamemodelocale is null)
            return new WinconditionOption(UcsString.CreateLocString(option.LocStr), option.Value);
        UcsString name = uint.TryParse(option.LocStr, out uint locKey) ? this.m_gamemodelocale.GetRef(locKey) : UcsString.CreateLocString(option.LocStr);
        return new WinconditionOption(name, option.Value);
    }

    private IGamemodeAuxiliaryOption GetAuxGamemodeOption(string name, Gamemode.GamemodeAdditionalOption option) {
        UcsString title;
        UcsString desc;
        UcsString format;
        if (this.m_gamemodelocale is not null) {
            title = uint.TryParse(option.Title, out uint locKey) ? this.m_gamemodelocale.GetRef(locKey) : UcsString.CreateLocString(option.Title);
            desc = uint.TryParse(option.Desc, out uint locKey2) ? this.m_gamemodelocale.GetRef(locKey2) : UcsString.CreateLocString(option.Desc);
            format = uint.TryParse(option.Value, out uint locKey3) ? this.m_gamemodelocale.GetRef(locKey3) : UcsString.CreateLocString(option.Value);
        } else {
            title = UcsString.CreateLocString(option.Title);
            desc = UcsString.CreateLocString(option.Desc);
            format = UcsString.CreateLocString(option.Value);
        }
        return option.Type switch {
            "Slider" => new WinconditionSliderOption() {
                Name = name,
                Title = title,
                Description = desc,
                Maximum = option.Max,
                Minimum = option.Min,
                Step = option.Step,
                Default = option.Default,
                Format = format,
            },
            "Dropdown" => new WinconditionDropdownOption() {
                Name = name,
                Title = title,
                Description = desc,
                Default = option.Default,
                Options = option.Options.Map(this.GetGamemodeOption)
            },
            "Checkbox" => new WinconditionDropdownOption() {
                Name = name,
                Title = title,
                Description = desc,
                Default = option.Default,
                Options = new IGamemodeOption[] {
                    new WinconditionOption(this.m_gamemodelocale is not null ? this.m_gamemodelocale.GetRef((uint)option.No) : UcsString.CreateLocString("No"), 0),
                    new WinconditionOption(this.m_gamemodelocale is not null ? this.m_gamemodelocale.GetRef((uint)option.Yes) : UcsString.CreateLocString("Yes"), 1),
                }
            },
            _ => new WinconditionDropdownOption() { }
        };
    }

    public IWinconditionMod GetWinconditionMod() 
        => new BattlegroundsWincondition(this.Package, this.Package.Gamemodes.Map(this.GetGamemode));

    public ITuningMod GetTuning()
        => new BattlegroundsTuning(this.Package);

}
