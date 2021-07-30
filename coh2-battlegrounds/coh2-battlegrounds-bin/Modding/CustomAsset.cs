namespace Battlegrounds.Modding {

    /// <summary>
    /// Class representing a custom-defined asset pack.
    /// </summary>
    public class CustomAsset : IGameMod {
        
        public ModGuid Guid { get; }

        public string Name { get; }

        public ModPackage Package { get; }

        public ModType GameModeType => ModType.Asset;

        public CustomAsset(ModPackage package) {
            this.Name = package.PackageName;
            this.Guid = package.AssetGUID;
            this.Package = package;
        }

    }

}
