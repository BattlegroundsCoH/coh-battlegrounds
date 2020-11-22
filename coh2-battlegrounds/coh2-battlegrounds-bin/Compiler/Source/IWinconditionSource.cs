namespace Battlegrounds.Compiler.Source {
    
    public interface IWinconditionSource {

        WinconoditionSourceFile[] GetScarFiles();

        WinconoditionSourceFile[] GetWinFiles();

        WinconoditionSourceFile[] GetLocaleFiles();

        WinconoditionSourceFile GetInfoFile();

    }

}
