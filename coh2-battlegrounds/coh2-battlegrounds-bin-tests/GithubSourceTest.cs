using System;
using System.IO;
using System.Linq;
using Battlegrounds.Compiler;
using Battlegrounds.Compiler.Source;
using Battlegrounds.Game.Gameplay;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace coh2_battlegrounds_bin_tests {
    
    [TestClass]
    public class GithubSourceTest {

        const string DEV_BRANCH = "scar-dev-branch";
        const string RELEASE_BRANCH = "scar-release-branch";

        Wincondition gamemode;
        GithubSource ws_dev;
        GithubSource ws_rel;
        string sessionFile = "E:\\coh2_battlegrounds\\coh2-battlegrounds-mod\\wincondition_mod\\auxiliary_scripts\\session.scar";

        [TestInitialize]
        public void Setup() {
            if (!File.Exists(sessionFile)) {
                Assert.Inconclusive("Incorrect testing environment!");
            }
            this.gamemode = new Wincondition("Battlegrounds Wincondition", new Guid("6a0a13b89555402ca75b85dc30f5cb04"));
            this.ws_dev = new GithubSource(DEV_BRANCH);
            this.ws_rel = new GithubSource(RELEASE_BRANCH);
        }

        [TestCleanup]
        public void Cleanup() {
            if (Directory.Exists("~tmp")) {
                Directory.Delete("~tmp", true);
            }
        }

        IWinconditionSource GetBranchSource(string branch) => branch.CompareTo(DEV_BRANCH) == 0 ? this.ws_dev : this.ws_rel;

        [TestMethod]
        public void ReplacesCorrectly() {
            const string given = "https://raw.githubusercontent.com/JustCodiex/coh2-battlegrounds/scar-release-branch/coh2-battlegrounds-mod/wincondition_mod/coh2_battlegrounds.scar";
            const string expected = "https://raw.githubusercontent.com/JustCodiex/coh2-battlegrounds/scar-dev-branch/coh2-battlegrounds-mod/wincondition_mod/coh2_battlegrounds.scar";
            Assert.AreEqual(expected, ws_dev.CorrectBranch(given));
        }

        [TestMethod]
        public void IsCorrectReleaseLength() {
            const string ln = "https://raw.githubusercontent.com/JustCodiex/coh2-battlegrounds/scar-release-branch/coh2-battlegrounds-mod/wincondition_mod/";
            Assert.AreEqual(ln.Length, ws_rel.GetCut());
        }

        [TestMethod]
        public void IsCorrectDevLength() {
            const string ln = "https://raw.githubusercontent.com/JustCodiex/coh2-battlegrounds/scar-dev-branch/coh2-battlegrounds-mod/wincondition_mod/";
            Assert.AreEqual(ln.Length, ws_dev.GetCut());
        }

        [DataTestMethod]
        [DataRow(DEV_BRANCH)]
        [DataRow(RELEASE_BRANCH)]
        public void CanGetInfo(string branch) {
            var info = this.GetBranchSource(branch).GetInfoFile(this.gamemode);
            Assert.AreEqual(info.path, $"info\\{this.gamemode.Guid}.info");
            Assert.AreNotEqual(0, info.contents.Length);
        }

        [DataTestMethod]
        [DataRow(DEV_BRANCH)]
        [DataRow(RELEASE_BRANCH)]
        public void CanGetScarFiles(string branch) {
            var files = this.GetBranchSource(branch).GetScarFiles();
            Assert.IsTrue(files.Length > 0);
            Assert.IsTrue(files.Any(x => x.path.EndsWith("coh2_battlegrounds.scar") && x.contents.Length > 0));
            Assert.IsTrue(files.Any(x => x.path.EndsWith("auxiliary_scripts\\shared_handler.scar") && x.contents.Length > 0));
            Assert.IsTrue(files.Any(x => x.path.EndsWith("auxiliary_scripts\\client_overrideui.scar") && x.contents.Length > 0));
            Assert.IsTrue(files.Any(x => x.path.EndsWith("auxiliary_scripts\\client_companyui.scar") && x.contents.Length > 0));
            Assert.IsTrue(files.Any(x => x.path.EndsWith("auxiliary_scripts\\api_ui.scar") && x.contents.Length > 0));
            Assert.IsTrue(files.Any(x => x.path.EndsWith("auxiliary_scripts\\shared_sessionloader.scar") && x.contents.Length > 0));
            Assert.IsTrue(files.Any(x => x.path.EndsWith("auxiliary_scripts\\shared_units.scar") && x.contents.Length > 0));
            Assert.IsTrue(files.Any(x => x.path.EndsWith("auxiliary_scripts\\shared_util.scar") && x.contents.Length > 0));
            Assert.IsTrue(files.Any(x => x.path.EndsWith("ui_api\\button.scar") && x.contents.Length > 0));
        }

        [DataTestMethod]
        [DataRow(DEV_BRANCH)]
        [DataRow(RELEASE_BRANCH)]
        public void CanGetEnglishLocaleFile(string branch) {
            var files = this.GetBranchSource(branch).GetLocaleFiles();
            Assert.IsTrue(files.Length > 0);
            Assert.IsTrue(files.Any(x => x.path.CompareTo("locale\\english\\english.ucs") == 0 && x.contents.Length > 2 && x.contents[0] == 0xff && x.contents[1] == 0xfe));
        }

        [DataTestMethod]
        [DataRow(DEV_BRANCH)]
        [DataRow(RELEASE_BRANCH)]
        public void CanGetWinFiles(string branch) {
            var files = this.GetBranchSource(branch).GetWinFiles();
            Assert.IsTrue(files.Length > 0);
            Assert.IsTrue(files.Any(x => x.path.Contains("coh2_battlegrounds.win") && x.contents.Length > 0));
        }


        [DataTestMethod]
        [DataRow(DEV_BRANCH)]
        [DataRow(RELEASE_BRANCH)]
        public void CanGetModGraphic(string branch) {
            var graphic = this.GetBranchSource(branch).GetModGraphic();
            Assert.IsTrue(graphic.path.EndsWith("info\\coh2_battlegrounds_wincondition_preview.dds"));
            Assert.IsTrue(graphic.contents.Length > 0);
        }

        [DataTestMethod]
        [DataRow(DEV_BRANCH)]
        [DataRow(RELEASE_BRANCH)]
        public void CanGetModUI(string branch) {
            var files = this.GetBranchSource(branch).GetUIFiles(this.gamemode);
            Assert.IsTrue(files.Length > 0);
            Assert.IsTrue(files.Any(x => x.path.EndsWith(".gfx") && x.contents.Length > 0));
            Assert.IsTrue(files.Any(x => x.path.EndsWith(".dds") && x.contents.Length > 0));
        }

        [DataTestMethod]
        [DataRow(DEV_BRANCH)]
        [DataRow(RELEASE_BRANCH)]
        public void CanCompileFromLocalSource(string branch) {
            Assert.IsTrue(WinconditionCompiler.CompileToSga("~tmp\\bld\\", sessionFile, this.gamemode, this.GetBranchSource(branch)));
            string path = "~tmp\\bld\\ArchiveDefinition.txt";
            Assert.IsTrue(File.Exists(path));
            Assert.IsTrue(File.ReadAllText(path).Length > 0);
        }

    }

}
