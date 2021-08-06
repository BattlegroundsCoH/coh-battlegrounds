using System.Linq;

using Battlegrounds.Game.DataSource;

namespace Battlegrounds.Modding {

    /// <summary>
    /// Sealed class representing a wincondition specifically tied to the Battlegrounds mod.
    /// </summary>
    public sealed class BattlegroundsWincondition : IWinconditionMod {

        public ModGuid Guid { get; }

        public string Name { get; }

        public IGamemode[] Gamemodes { get; }

        public ModPackage Package { get; }

        public ModType GameModeType => ModType.Gamemode;

        public BattlegroundsWincondition(ModPackage package) {

            // Set basic properties
            this.Guid = package.GamemodeGUID;
            this.Name = package.PackageName;

            // Get locale
            var locale = package.GetLocale(ModType.Gamemode, BattlegroundsInstance.Localize.Language);

            // Set package
            this.Package = package;

            // Loop over gamemodes
            this.Gamemodes = this.Package.Gamemodes.Select(x => {
                IGamemodeOption[] options = (x.Options?.Length ?? 0) is 0 ? null : x.Options.Select(x => GetGamemodeOption(x, locale)).ToArray();
                UcsString name = UcsString.None;
                UcsString desc = UcsString.None;
                if (uint.TryParse(x.Display, out uint nameKey)) {
                    name = locale.GetRef(nameKey);
                }
                if (uint.TryParse(x.DisplayDesc, out uint descKey)) {
                    desc = locale.GetRef(descKey);
                }
                return new Wincondition(x.ID, this.Guid) {
                    Options = options,
                    DefaultOptionIndex = x.DefaultOption,
                    DisplayName = name,
                    DisplayShortDescription = desc
                };
            }).ToArray();

        }

        private static IGamemodeOption GetGamemodeOption(ModPackage.Gamemode.GamemodeOption option, UcsFile loc) {
            UcsString name = uint.TryParse(option.LocStr, out uint locKey) ? loc.GetRef(locKey) : UcsString.CreateLocString(option.LocStr);
            return new WinconditionOption(name, option.Value);
        }

    }

}
