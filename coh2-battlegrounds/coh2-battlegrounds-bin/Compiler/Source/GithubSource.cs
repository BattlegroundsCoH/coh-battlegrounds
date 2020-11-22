using System.Linq;
using System.Text;
using Battlegrounds.Online;

namespace Battlegrounds.Compiler.Source {

    public class GithubSource : IWinconditionSource {

        // URLs to the most up-to-date scar files
        private static string[] ScarFiles = new string[] {
            "https://raw.githubusercontent.com/JustCodiex/coh2-battlegrounds/scar-release-branch/coh2-battlegrounds-mod/wincondition_mod/auxiliary_scripts/shared_util.scar",
            "https://raw.githubusercontent.com/JustCodiex/coh2-battlegrounds/scar-release-branch/coh2-battlegrounds-mod/wincondition_mod/auxiliary_scripts/shared_units.scar",
            "https://raw.githubusercontent.com/JustCodiex/coh2-battlegrounds/scar-release-branch/coh2-battlegrounds-mod/wincondition_mod/auxiliary_scripts/shared_handler.scar",
            "https://raw.githubusercontent.com/JustCodiex/coh2-battlegrounds/scar-release-branch/coh2-battlegrounds-mod/wincondition_mod/auxiliary_scripts/shared_sessionloader.scar",
            "https://raw.githubusercontent.com/JustCodiex/coh2-battlegrounds/scar-release-branch/coh2-battlegrounds-mod/wincondition_mod/auxiliary_scripts/shared_ai.scar",
            "https://raw.githubusercontent.com/JustCodiex/coh2-battlegrounds/scar-release-branch/coh2-battlegrounds-mod/wincondition_mod/auxiliary_scripts/shared_lookups.scar",
            "https://raw.githubusercontent.com/JustCodiex/coh2-battlegrounds/scar-release-branch/coh2-battlegrounds-mod/wincondition_mod/auxiliary_scripts/client_companyui.scar",
            "https://raw.githubusercontent.com/JustCodiex/coh2-battlegrounds/scar-release-branch/coh2-battlegrounds-mod/wincondition_mod/auxiliary_scripts/client_overrideui.scar",
            "https://raw.githubusercontent.com/JustCodiex/coh2-battlegrounds/scar-release-branch/coh2-battlegrounds-mod/wincondition_mod/auxiliary_scripts/api_ui.scar",
            "https://raw.githubusercontent.com/JustCodiex/coh2-battlegrounds/scar-release-branch/coh2-battlegrounds-mod/wincondition_mod/ui_api/button.scar",
            "https://raw.githubusercontent.com/JustCodiex/coh2-battlegrounds/scar-release-branch/coh2-battlegrounds-mod/wincondition_mod/ui_api/class.scar",
            "https://raw.githubusercontent.com/JustCodiex/coh2-battlegrounds/scar-release-branch/coh2-battlegrounds-mod/wincondition_mod/ui_api/color.scar",
            "https://raw.githubusercontent.com/JustCodiex/coh2-battlegrounds/scar-release-branch/coh2-battlegrounds-mod/wincondition_mod/ui_api/control.scar",
            "https://raw.githubusercontent.com/JustCodiex/coh2-battlegrounds/scar-release-branch/coh2-battlegrounds-mod/wincondition_mod/ui_api/icon.scar",
            "https://raw.githubusercontent.com/JustCodiex/coh2-battlegrounds/scar-release-branch/coh2-battlegrounds-mod/wincondition_mod/ui_api/label.scar",
            "https://raw.githubusercontent.com/JustCodiex/coh2-battlegrounds/scar-release-branch/coh2-battlegrounds-mod/wincondition_mod/ui_api/panel.scar",
            "https://raw.githubusercontent.com/JustCodiex/coh2-battlegrounds/scar-release-branch/coh2-battlegrounds-mod/wincondition_mod/ui_api/rootpanel.scar",
            "https://raw.githubusercontent.com/JustCodiex/coh2-battlegrounds/scar-release-branch/coh2-battlegrounds-mod/wincondition_mod/ui_api/statusindicator.scar",
            "https://raw.githubusercontent.com/JustCodiex/coh2-battlegrounds/scar-release-branch/coh2-battlegrounds-mod/wincondition_mod/coh2_battlegrounds.scar",
        };

        // URLs to the most up-to-date win files
        private static string[] WinFiles = new string[] {
            "https://raw.githubusercontent.com/JustCodiex/coh2-battlegrounds/scar-release-branch/coh2-battlegrounds-mod/wincondition_mod/coh2_battlegrounds.win",
        };

        // URLs to the most up-to-date info file
        private static string InfoFile = "https://raw.githubusercontent.com/JustCodiex/coh2-battlegrounds/scar-release-branch/coh2-battlegrounds-mod/wincondition_mod/coh2_battlegrounds_wincondition%20Intermediate%20Cache/Intermediate%20Files/info/6a0a13b89555402ca75b85dc30f5cb04.info";

        // URLs to the most up-to-date locale files
        private static string[] LocaleFiles = new string[] {
            "https://raw.githubusercontent.com/JustCodiex/coh2-battlegrounds/scar-release-branch/coh2-battlegrounds-mod/wincondition_mod/locale/english/english.ucs",
        };

        private string m_branch;

        public GithubSource(string branchname) {
            this.m_branch = branchname;
        }

        public WinconoditionSourceFile GetInfoFile() 
            => new WinconoditionSourceFile(InfoFile[this.GetCut()..], Encoding.UTF8.GetBytes(SourceDownloader.DownloadSourceCode(CorrectBranch(InfoFile))));

        public WinconoditionSourceFile[] GetLocaleFiles() 
            => LocaleFiles.Select(
                x => new WinconoditionSourceFile(x[this.GetCut()..], // get path
                    (byte[])new byte[] { 0xff, 0xfe } // create byte array with required start bytes
                    .Union(Encoding.Unicode.GetBytes(SourceDownloader.DownloadSourceCode(CorrectBranch(x)))))) // and union with the unicode representation of the external source file
            .ToArray(); // convert all the selected files to an array

        public WinconoditionSourceFile[] GetScarFiles()
            => ScarFiles.Select(
                x => new WinconoditionSourceFile(x[this.GetCut()..], Encoding.UTF8.GetBytes(SourceDownloader.DownloadSourceCode(CorrectBranch(x))))).ToArray();

        public WinconoditionSourceFile[] GetWinFiles()
            => WinFiles.Select(
                x => new WinconoditionSourceFile(x[this.GetCut()..], Encoding.UTF8.GetBytes(SourceDownloader.DownloadSourceCode(CorrectBranch(x))))).ToArray();

        private string CorrectBranch(string input) => input.Replace("scar-release-branch", this.m_branch);

        private int GetCut() => this.CorrectBranch("https://raw.githubusercontent.com/JustCodiex/coh2-battlegrounds/scar-release-branch/coh2-battlegrounds-mod/wincondition_mod/").Length;

    }

}
