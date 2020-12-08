using Battlegrounds.Modding;

namespace Battlegrounds.Compiler.Source {
    
    public interface IWinconditionSource {

        WinconoditionSourceFile[] GetScarFiles();

        WinconoditionSourceFile[] GetWinFiles();

        WinconoditionSourceFile[] GetLocaleFiles();

        WinconoditionSourceFile[] GetUIFiles(IWinconditionMod mod);

        WinconoditionSourceFile GetModGraphic();

        WinconoditionSourceFile GetInfoFile(IWinconditionMod mod);

    }

}
