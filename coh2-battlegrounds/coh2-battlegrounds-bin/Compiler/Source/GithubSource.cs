﻿using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Battlegrounds.Functional;
using Battlegrounds.Modding;
using Battlegrounds.Online;
using Battlegrounds.ErrorHandling;

namespace Battlegrounds.Compiler.Source {

    public class GithubSource : IWinconditionSource {

        // URLs to the most up-to-date scar files
        private static string[] ScarFiles = new string[] {
            @"https://raw.githubusercontent.com/JustCodiex/coh2-battlegrounds/scar-release-branch/coh2-battlegrounds-mod/wincondition_mod/auxiliary_scripts/shared_util.scar",
            @"https://raw.githubusercontent.com/JustCodiex/coh2-battlegrounds/scar-release-branch/coh2-battlegrounds-mod/wincondition_mod/auxiliary_scripts/shared_units.scar",
            @"https://raw.githubusercontent.com/JustCodiex/coh2-battlegrounds/scar-release-branch/coh2-battlegrounds-mod/wincondition_mod/auxiliary_scripts/shared_handler.scar",
            @"https://raw.githubusercontent.com/JustCodiex/coh2-battlegrounds/scar-release-branch/coh2-battlegrounds-mod/wincondition_mod/auxiliary_scripts/shared_sessionloader.scar",
            @"https://raw.githubusercontent.com/JustCodiex/coh2-battlegrounds/scar-release-branch/coh2-battlegrounds-mod/wincondition_mod/auxiliary_scripts/shared_ai.scar",
            @"https://raw.githubusercontent.com/JustCodiex/coh2-battlegrounds/scar-release-branch/coh2-battlegrounds-mod/wincondition_mod/auxiliary_scripts/shared_lookups.scar",
            @"https://raw.githubusercontent.com/JustCodiex/coh2-battlegrounds/scar-release-branch/coh2-battlegrounds-mod/wincondition_mod/auxiliary_scripts/client_companyui.scar",
            @"https://raw.githubusercontent.com/JustCodiex/coh2-battlegrounds/scar-release-branch/coh2-battlegrounds-mod/wincondition_mod/auxiliary_scripts/client_overrideui.scar",
            @"https://raw.githubusercontent.com/JustCodiex/coh2-battlegrounds/scar-release-branch/coh2-battlegrounds-mod/wincondition_mod/auxiliary_scripts/api_ui.scar",
            @"https://raw.githubusercontent.com/JustCodiex/coh2-battlegrounds/scar-release-branch/coh2-battlegrounds-mod/wincondition_mod/ui_api/button.scar",
            @"https://raw.githubusercontent.com/JustCodiex/coh2-battlegrounds/scar-release-branch/coh2-battlegrounds-mod/wincondition_mod/ui_api/class.scar",
            @"https://raw.githubusercontent.com/JustCodiex/coh2-battlegrounds/scar-release-branch/coh2-battlegrounds-mod/wincondition_mod/ui_api/color.scar",
            @"https://raw.githubusercontent.com/JustCodiex/coh2-battlegrounds/scar-release-branch/coh2-battlegrounds-mod/wincondition_mod/ui_api/control.scar",
            @"https://raw.githubusercontent.com/JustCodiex/coh2-battlegrounds/scar-release-branch/coh2-battlegrounds-mod/wincondition_mod/ui_api/icon.scar",
            @"https://raw.githubusercontent.com/JustCodiex/coh2-battlegrounds/scar-release-branch/coh2-battlegrounds-mod/wincondition_mod/ui_api/label.scar",
            @"https://raw.githubusercontent.com/JustCodiex/coh2-battlegrounds/scar-release-branch/coh2-battlegrounds-mod/wincondition_mod/ui_api/panel.scar",
            @"https://raw.githubusercontent.com/JustCodiex/coh2-battlegrounds/scar-release-branch/coh2-battlegrounds-mod/wincondition_mod/ui_api/rootpanel.scar",
            @"https://raw.githubusercontent.com/JustCodiex/coh2-battlegrounds/scar-release-branch/coh2-battlegrounds-mod/wincondition_mod/ui_api/statusindicator.scar",
            @"https://raw.githubusercontent.com/JustCodiex/coh2-battlegrounds/scar-release-branch/coh2-battlegrounds-mod/wincondition_mod/coh2_battlegrounds.scar",
        };

        // URLs to the most up-to-date win files
        private static string[] WinFiles = new string[] {
            @"https://raw.githubusercontent.com/JustCodiex/coh2-battlegrounds/scar-release-branch/coh2-battlegrounds-mod/wincondition_mod/coh2_battlegrounds.win",
        };

        // URLs to the most up-to-date info file
        private static string InfoFile = @"https://raw.githubusercontent.com/JustCodiex/coh2-battlegrounds/scar-release-branch/coh2-battlegrounds-mod/wincondition_mod/coh2_battlegrounds_wincondition%20Intermediate%20Cache/Intermediate%20Files/info/6a0a13b89555402ca75b85dc30f5cb04.info";

        // URLs to the most up-to-date locale files
        private static string[] LocaleFiles = new string[] {
            @"https://raw.githubusercontent.com/JustCodiex/coh2-battlegrounds/scar-release-branch/coh2-battlegrounds-mod/wincondition_mod/locale/english/english.ucs",
        };

        private string m_branch;

        public GithubSource(string branchname) {
            this.m_branch = branchname;
        }

        public WinconoditionSourceFile GetInfoFile(IWinconditionMod mod) 
            => new WinconoditionSourceFile($"info\\{ModGuid.FromGuid(mod.Guid)}.info", Encoding.UTF8.GetBytes(SourceDownloader.DownloadSourceCode(CorrectBranch(InfoFile))));

        public WinconoditionSourceFile[] GetLocaleFiles() {
            var files = new List<WinconoditionSourceFile>();
            LocaleFiles.ForEach(x => {
                string correct = this.CorrectBranch(x);
                byte[] byteContent = (new byte[] { 0xff, 0xfe }).Union(Encoding.Unicode.GetBytes(SourceDownloader.DownloadSourceCode(correct))).ToArray();
                files.Add(new WinconoditionSourceFile(correct[this.GetCut()..].Replace("/", "\\"), byteContent));
            });
            return files.ToArray();
        }

        public WinconoditionSourceFile[] GetScarFiles() {
            var files = new List<WinconoditionSourceFile>();
            ScarFiles.ForEach(x => {
                string correct = this.CorrectBranch(x);
                files.Add(new WinconoditionSourceFile(correct[this.GetCut()..].Replace("/", "\\"), Encoding.UTF8.GetBytes(SourceDownloader.DownloadSourceCode(correct))));
            });
            return files.ToArray();
        }

        public WinconoditionSourceFile[] GetWinFiles() {
            var files = new List<WinconoditionSourceFile>();
            WinFiles.ForEach(x => {
                string correct = this.CorrectBranch(x);
                files.Add(new WinconoditionSourceFile(correct[this.GetCut()..].Replace("/", "\\"), Encoding.UTF8.GetBytes(SourceDownloader.DownloadSourceCode(correct))));
            });
            return files.ToArray();
        }

        public WinconoditionSourceFile[] GetUIFiles(IWinconditionMod mod) {
            try {
                string[] local = Directory.GetFiles("bin\\gfx\\");
                if (local.Length > 0) {
                    var files = new List<WinconoditionSourceFile>();
                    for (int i = 0; i < local.Length; i++) {
                        byte[] contents = File.ReadAllBytes(local[i]);
                        if (local[i].EndsWith(".gfx")) {
                            files.Add(new WinconoditionSourceFile($"data\\ui\\Bin\\{ModGuid.FromGuid(mod.Guid)}.gfx", contents));
                        } else if (local[i].EndsWith(".dds")) {
                            files.Add(new WinconoditionSourceFile($"data\\ui\\Assets\\Textures\\{Path.GetFileName(local[i])}", contents));
                        } // else ... ignore
                    }
                    return files.ToArray();
                } else {
                    throw new EnvironmentException($"Failed to find .gfx and .dds files that are part of the shipped build!");
                }
            } catch(DirectoryNotFoundException e) {
                throw new EnvironmentException(e.Message);
            }
        }

        public WinconoditionSourceFile GetModGraphic() {
            const string path = "bin\\coh2_battlegrounds_wincondition_preview.dds";
            if (File.Exists(path)) {
                byte[] ddsBytes = File.ReadAllBytes(path);
                return new WinconoditionSourceFile($"info\\coh2_battlegrounds_wincondition_preview.dds", ddsBytes);
            } else {
                throw new EnvironmentException($"Failed to find shipped file \"{path}\"");
            }
        }

        public string CorrectBranch(string input) 
            => input.Replace("scar-release-branch", this.m_branch);

        public int GetCut() 
            => this.CorrectBranch(@"https://raw.githubusercontent.com/JustCodiex/coh2-battlegrounds/scar-release-branch/coh2-battlegrounds-mod/wincondition_mod/").Length;

        public override string ToString() => "Github";

    }

}