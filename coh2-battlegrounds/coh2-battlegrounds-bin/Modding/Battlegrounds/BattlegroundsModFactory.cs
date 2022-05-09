using System;

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
            Gamemode.GamemodeOption[] y => y.Map(z => this.GetGamemodeOption(z)),
            _ => Array.Empty<IGamemodeOption>()
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
            DisplayName = name,
            DisplayShortDescription = desc
        };

    }

    private IGamemodeOption GetGamemodeOption(Gamemode.GamemodeOption option) {
        if (this.m_gamemodelocale is null)
            return new WinconditionOption(UcsString.CreateLocString(option.LocStr), option.Value);
        UcsString name = uint.TryParse(option.LocStr, out uint locKey) ? this.m_gamemodelocale.GetRef(locKey) : UcsString.CreateLocString(option.LocStr);
        return new WinconditionOption(name, option.Value);
    }

    public IWinconditionMod GetWinconditionMod() 
        => new BattlegroundsWincondition(this.Package, this.Package.Gamemodes.Map(this.GetGamemode));

    public ITuningMod GetTuning()
        => new BattlegroundsTuning(this.Package);

}
