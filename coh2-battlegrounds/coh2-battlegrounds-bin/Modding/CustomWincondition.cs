using System;

namespace Battlegrounds.Modding;

public class CustomWincondition : IWinconditionMod {

    public WinconditionOption[] Options { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public int DefaultOptionIndex { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    public ModGuid Guid => throw new NotImplementedException();

    public string Name => throw new NotImplementedException();

    public IGamemode[] Gamemodes { get; }

    public ModPackage Package { get; }

    public ModType GameModeType => ModType.Gamemode;

    public CustomWincondition(ModPackage package) {
        this.Package = package;
        this.Gamemodes = Array.Empty<IGamemode>();
    }

}
