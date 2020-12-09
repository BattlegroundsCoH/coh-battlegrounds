using System;
using System.IO;
using System.Linq;
using Battlegrounds.Compiler;
using Battlegrounds.Compiler.Source;
using Battlegrounds.Game.Gameplay;
using Battlegrounds.Modding;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace coh2_battlegrounds_bin_tests {
    
    [TestClass]
    public class LocalSourceTest {

        Wincondition gamemode;
        LocalSource ls;
        string sessionFile = "E:\\coh2_battlegrounds\\coh2-battlegrounds-mod\\wincondition_mod\\auxiliary_scripts\\session.scar";

        [TestInitialize]
        public void Setup() {
            if (!File.Exists(sessionFile)) {
                Assert.Inconclusive("Incorrect testing environment!");
            }
            this.gamemode = new Wincondition("Battlegrounds Wincondition", new Guid("6a0a13b89555402ca75b85dc30f5cb04"));
            this.ls = new LocalSource(@"E:\coh2_battlegrounds\coh2-battlegrounds-mod\wincondition_mod"); // This may cause test on your local setup to fail!
        }

        [TestCleanup]
        public void Cleanup() {
            if (Directory.Exists("~tmp")) {
                Directory.Delete("~tmp", true);
            }
        }

        [TestMethod]
        public void CanGetInfo() {
            var info = this.ls.GetInfoFile(this.gamemode);
            Assert.AreEqual(info.path, $"info\\{ModGuid.FromGuid(this.gamemode.Guid)}.info");
        }

        [TestMethod]
        public void CanGetScarFiles() {
            var files = this.ls.GetScarFiles();
            Assert.IsTrue(files.Length > 0);
            Assert.IsTrue(files.Any(x => x.path.Contains("coh2_battlegrounds.scar")));
            Assert.IsTrue(files.Any(x => x.path.Contains("auxiliary_scripts\\shared_handler.scar")));
            Assert.IsTrue(files.Any(x => x.path.Contains("auxiliary_scripts\\client_overrideui.scar")));
            Assert.IsTrue(files.Any(x => x.path.Contains("auxiliary_scripts\\client_companyui.scar")));
            Assert.IsTrue(files.Any(x => x.path.Contains("auxiliary_scripts\\api_ui.scar")));
            Assert.IsTrue(files.Any(x => x.path.Contains("auxiliary_scripts\\shared_sessionloader.scar")));
            Assert.IsTrue(files.Any(x => x.path.Contains("auxiliary_scripts\\shared_units.scar")));
            Assert.IsTrue(files.Any(x => x.path.Contains("auxiliary_scripts\\shared_util.scar")));
            Assert.IsTrue(files.Any(x => x.path.Contains("ui_api\\button.scar")));
            Assert.IsTrue(!files.Any(x => x.path.Contains("auxiliary_scripts\\session.scar")));
        }

        [TestMethod]
        public void CanGetEnglishLocaleFile() {
            var files = this.ls.GetLocaleFiles();
            Assert.IsTrue(files.Length > 0);
            Assert.IsTrue(files.Any(x => x.path.Contains("english.ucs"))); // only really interested in the english file to verify it works (Add more tests for other locale files)
        }

        [TestMethod]
        public void CanGetWinFiles() {
            var files = this.ls.GetWinFiles();
            Assert.IsTrue(files.Length > 0);
            Assert.IsTrue(files.Any(x => x.path.Contains("coh2_battlegrounds.win")));
        }

        [TestMethod]
        public void CanGetModGraphic() {
            var graphic = this.ls.GetModGraphic();
            Assert.IsTrue(graphic.path.EndsWith("info\\coh2_battlegrounds_wincondition_preview.dds"));
            Assert.IsTrue(graphic.contents.Length > 0);
        }

        [TestMethod]
        public void CanGetModUI() {
            var files = this.ls.GetUIFiles(this.gamemode);
            Assert.IsTrue(files.Length > 0);
            Assert.IsTrue(files.Any(x => x.path.EndsWith(".gfx")));
            Assert.IsTrue(files.Any(x => x.path.EndsWith(".dds")));
        }

        [TestMethod]
        public void CanCompileFromLocalSource() {
            Assert.IsTrue(WinconditionCompiler.CompileToSga("~tmp\\bld\\", sessionFile, this.gamemode, this.ls));
            string path = "~tmp\\bld\\ArchiveDefinition.txt";
            Assert.IsTrue(File.Exists(path));
            Assert.IsTrue(File.ReadAllText(path).Length > 0);
        }

    }

}
