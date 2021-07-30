using System.Linq;

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

            // Set package
            this.Package = package;

            // Loop over gamemodes
            this.Gamemodes = this.Package.Gamemodes.Select(x => {
                IGamemodeOption[] options = (x.Options?.Length ?? 0) is 0 ? null : x.Options.Select(x => new WinconditionOption(x.LocStr, x.Value)).Cast<IGamemodeOption>().ToArray();
                return new Wincondition(x.ID, this.Guid) {
                    Options = options,
                    DefaultOptionIndex = x.DefaultOption,
                    DisplayName = x.Display,
                    DisplayShortDescription = x.DisplayDesc
                };
            }).ToArray();

        }

    }

}
