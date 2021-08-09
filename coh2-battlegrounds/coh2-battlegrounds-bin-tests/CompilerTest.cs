using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;

using Battlegrounds;
using Battlegrounds.Compiler;
using Battlegrounds.Game;
using Battlegrounds.Game.Database;
using Battlegrounds.Game.Database.Management;
using Battlegrounds.Game.DataCompany;
using Battlegrounds.Game.Match;
using Battlegrounds.Lua;
using Battlegrounds.Modding;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace coh2_battlegrounds_bin_tests {

    [TestClass]
    public class CompilerTest {

        public const bool WRITE_FILES = true;

        ISessionCompiler sessionCompiler;
        ICompanyCompiler companyCompiler;
        ModPackage package;
        LuaState luaState;

        SessionInfo info; // We can then modify this if needed...

        Company sovietCompany;
        Company germanCompany;

        [TestInitialize]
        public void Initialise() {

            // Tell BG to load itself
            if (DatabaseManager.DatabaseLoaded is false) {
                bool canContinue = false;
                Environment.CurrentDirectory = @"E:\coh2_battlegrounds\coh2-battlegrounds\coh2-battlegrounds\bin\Debug\net5.0-windows";
                BattlegroundsInstance.LoadInstance();
                DatabaseManager.LoadAllDatabases((a, b) => canContinue = true);
                while (!canContinue) {
                    Thread.Sleep(1);
                }
            }

            // Get package
            package = ModManager.GetPackage("mod_bg");

            // Create specific compiler instances
            this.companyCompiler = new CompanyCompiler(); // BG SPECIFIC TESTING
            this.sessionCompiler = new SessionCompiler(); // BG SPECIFIC TESTING
            this.sessionCompiler.SetCompanyCompiler(companyCompiler);

            // Get companies
            this.sovietCompany = CompanyTestBuilder.CreateAdvancedSovietCompany();
            this.germanCompany = CompanyTestBuilder.CreateAdvancedGermanCompany();

            // Get session info
            this.info = new SessionInfo() {
                FillAI = false,
                SelectedGamemode = WinconditionList.GetGamemodeByName(this.package.GamemodeGUID, "bg_vp"),
                SelectedGamemodeOption = 1,
                IsOptionValue = false,
                SelectedScenario = new() { RelativeFilename = "2p_angoville_farms", MaxPlayers = 2 },
                SelectedTuningMod = ModManager.GetMod<ITuningMod>(this.package.TuningGUID),
                DefaultDifficulty = AIDifficulty.AI_Hard,
                Allies = new SessionParticipant[] { new("Red Army", 77789995, this.sovietCompany, SessionParticipantTeam.TEAM_ALLIES, 0) },
                Axis = new SessionParticipant[] { new("Wehrmacht", 77789996, this.germanCompany, SessionParticipantTeam.TEAM_AXIS, 0) },
            };

            if (!Directory.Exists("compile_tests")) {
                Directory.CreateDirectory("compile_tests");
            }

        }

        [TestCleanup]
        public void Cleanup() {

        }

        private static string GetTestName([CallerMemberName] string s = "") => s;

        [TestMethod]
        public void CanCompileBasicSessionNoVerify() {

            // Create session data
            var session = Session.CreateSession(info);
            Assert.IsNotNull(session);

            // Compile the session
            string sessionFile = this.sessionCompiler.CompileSession(session);
            Assert.IsTrue(sessionFile.Length > 0);
            if (WRITE_FILES) {
                File.WriteAllText($"compile_tests\\{GetTestName()}_session.scar", sessionFile);
            }

        }

    }

}
