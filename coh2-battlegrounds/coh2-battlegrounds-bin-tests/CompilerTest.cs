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
    public class CompilerTest { // Integration testing

        public const bool WRITE_FILES = true; // Set to false to disable file output (Will output to BG dir)

        ISessionCompiler sessionCompiler;
        ICompanyCompiler companyCompiler;
        ModPackage package;

        SessionInfo info; // We can then modify this if needed...

        LuaState luaState;

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
                EnableSupply = true,
                EnableDayNightCycle = true,
                SelectedScenario = new() { RelativeFilename = "2p_angoville_farms", MaxPlayers = 2 },
                SelectedTuningMod = ModManager.GetMod<ITuningMod>(this.package.TuningGUID),
                DefaultDifficulty = AIDifficulty.AI_Hard,
                Allies = new SessionParticipant[] { new("Red Army", 77789995, this.sovietCompany, ParticipantTeam.TEAM_ALLIES, 0, 0) },
                Axis = new SessionParticipant[] { new("Wehrmacht", 77789996, this.germanCompany, ParticipantTeam.TEAM_AXIS, 0, 1) },
            };

            // Create directory with compilte test files
            if (!Directory.Exists("compile_tests")) {
                Directory.CreateDirectory("compile_tests");
            }

            // Create lua state
            luaState = new();
            luaState._G["bg_db"] = new LuaTable(luaState);

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

        [TestMethod]
        public void CanCompileBasicSessionAndSupplyNoVerify() {

            // Create session data
            var session = Session.CreateSession(info);
            Assert.IsNotNull(session);

            // Compile the session
            string sessionFile = this.sessionCompiler.CompileSession(session);
            Assert.IsTrue(sessionFile.Length > 0);
            if (WRITE_FILES) {
                File.WriteAllText($"compile_tests\\{GetTestName()}_session.scar", sessionFile);
            }

            // Compile the session
            string sessionSupplyFile = this.sessionCompiler.CompileSupplyData(session);
            Assert.IsTrue(sessionSupplyFile.Length > 0);
            if (WRITE_FILES) {
                File.WriteAllText($"compile_tests\\{GetTestName()}_session_supply.scar", sessionSupplyFile);
            }

        }

        /* // Currently disabled --> Lua Runtime issue
        [TestMethod]
        public void CanInterpretBasicSession() {

            // Create session data
            var session = Session.CreateSession(info);
            Assert.IsNotNull(session);

            // Compile the session
            string sessionFile = this.sessionCompiler.CompileSession(session);
            Assert.IsTrue(sessionFile.Length > 0);
            if (WRITE_FILES) {
                File.WriteAllText($"compile_tests\\{GetTestName()}_session.scar", sessionFile);
            }

            // Do the stuff
            LuaVM.DoString(luaState, sessionFile, $"{GetTestName()}_session.scar");

            // Assert table exists
            Assert.IsTrue(luaState._G["bg_settings"] is not LuaNil);
            Assert.IsTrue(luaState._G["bg_companies"] is not LuaNil);
            Assert.IsTrue(luaState._G["bg_db.towing_upgrade"] is not LuaNil);

        }*/

    }

}
