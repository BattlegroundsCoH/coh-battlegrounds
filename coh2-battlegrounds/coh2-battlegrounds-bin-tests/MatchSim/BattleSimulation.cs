using System;
using System.Linq;
using System.Threading;

using Battlegrounds;
using Battlegrounds.Functional;
using Battlegrounds.Game.Database;
using Battlegrounds.Game.Database.Management;
using Battlegrounds.Game.DataCompany;
using Battlegrounds.Game.Gameplay;
using Battlegrounds.Game.Match;
using Battlegrounds.Game.Match.Analyze;
using Battlegrounds.Game.Match.Data;
using Battlegrounds.Game.Match.Data.Events;
using Battlegrounds.Game.Match.Finalizer;
using Battlegrounds.Game.Match.Play;
using Battlegrounds.Modding;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace coh2_battlegrounds_bin_tests.MatchSim {

    [TestClass]
    public class BattleSimulation {

        ISession session;

        BattleSimulatorStrategy playStrategy;
        IAnalyzeStrategy analyzeStrategy;
        IFinalizeStrategy finalizeStrategy;
        Player[] players;
        Company company;

        private Player SOVIET => players[0];

        private Player GERMAN => players[1];

        [TestInitialize]
        public void Setup() {
            if (DatabaseManager.DatabaseLoaded is false) {
                bool canContinue = false;
                Environment.CurrentDirectory = @"E:\coh2_battlegrounds\coh2-battlegrounds\coh2-battlegrounds\bin\Debug\net5.0-windows";
                BattlegroundsInstance.LoadInstance();
                DatabaseManager.LoadAllDatabases((a, b) => canContinue = true);
                while (!canContinue) {
                    Thread.Sleep(1);
                }
            }
            var s = new NullSession(true);
            s.CreateCompany(0, Faction.Soviet, "Allies",
                x => x.AddUnit(y => y.SetBlueprint(BlueprintManager.FromBlueprintName<SquadBlueprint>("conscript_squad_bg")))
                .AddUnit(y => y.SetBlueprint(BlueprintManager.FromBlueprintName<SquadBlueprint>("conscript_squad_bg"))));
            s.CreateCompany(1, Faction.Wehrmacht, "Axis",
                x => x.AddUnit(y => y.SetBlueprint(BlueprintManager.FromBlueprintName<SquadBlueprint>("panzer_iv_squad_bg"))));
            session = s;
            playStrategy = new BattleSimulatorStrategy(session);
            analyzeStrategy = new SingleplayerMatchAnalyzer();
            finalizeStrategy = new SingleplayerFinalizer() { CompanyHandler = x => x.IfTrue(x => x.Army == Faction.Soviet).Then(() => company = x) };
            players = new Player[] {
                new Player (0, 0, 0, "Player 1", Faction.Soviet, string.Empty),
                new Player (1, 1, 1, "Player 2", Faction.Wehrmacht, string.Empty),
            };
        }

        private (SimulatedMatchData, IAnalyzedMatch) AnalyseAndAssert(int eventCount = -1, TimeSpan time = default) {

            // Get and initialize simulated data
            SimulatedMatchData data = playStrategy.GetResults() as SimulatedMatchData;
            data.CreatePlayer(SOVIET);
            data.CreatePlayer(GERMAN);

            // Verify simulated data is registered
            Assert.IsNotNull(data);
            if (eventCount is not -1) {
                Assert.AreEqual(eventCount, data.Count());
            }
            if (time != default) {
                Assert.AreEqual(time, data.Length);
            }

            // Put through analysis
            analyzeStrategy.OnPrepare(null, data);
            analyzeStrategy.OnAnalyze(null);

            // Compile analysis
            var result = analyzeStrategy.OnCleanup(null);
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(EventAnalysis));
            Assert.AreEqual(true, result.CompileResults());

            // Return results
            return (data, result);

        }

        [TestMethod]
        public void CanDeployAndKillWithoutError() {

            // Create events
            playStrategy.BattleEvent(TimeSpan.FromSeconds(1), new DeployEvent(0, new string[] { "0", }, SOVIET));
            playStrategy.BattleEvent(TimeSpan.FromSeconds(3.8), new KillEvent(1, new string[] { "0", }, SOVIET));

            // Analyse and assert
            var (data, result) = AnalyseAndAssert(2, TimeSpan.FromSeconds(3.8));
            Assert.AreEqual(1, result.Units.Count);

            // Verify Analysis
            UnitStatus status = result.Units[0];
            Assert.AreEqual(0, status.UnitID);
            Assert.AreEqual(true, status.IsDead);
            Assert.AreEqual(false, status.IsDeployed);
            Assert.AreEqual(false, status.IsWithdrawn);
            Assert.AreEqual(TimeSpan.FromSeconds(1), status.FirstSeen);
            Assert.AreEqual(TimeSpan.FromSeconds(3.8), status.LastSeen);
            Assert.AreEqual(TimeSpan.FromSeconds(3.8 - 1.0), status.CombatTime);

        }

        [TestMethod]
        public void CanDeployAndWithdrawWithoutError() {

            // Create events
            playStrategy.BattleEvent(TimeSpan.FromSeconds(1), new DeployEvent(0, new string[] { "0", }, SOVIET));
            playStrategy.BattleEvent(TimeSpan.FromSeconds(3.8), new RetreatEvent(1, new string[] { "0", "1", "100" }, SOVIET));

            // Analyse and assert
            var (data, result) = AnalyseAndAssert(2, TimeSpan.FromSeconds(3.8));
            Assert.AreEqual(1, result.Units.Count);

            // Verify Analysis
            UnitStatus status = result.Units[0];
            Assert.AreEqual(0, status.UnitID);
            Assert.AreEqual(false, status.IsDead);
            Assert.AreEqual(false, status.IsDeployed);
            Assert.AreEqual(true, status.IsWithdrawn);
            Assert.AreEqual(1, status.VetChange);
            Assert.AreEqual(100, status.VetExperience);
            Assert.AreEqual(TimeSpan.FromSeconds(1), status.FirstSeen);
            Assert.AreEqual(TimeSpan.FromSeconds(3.8), status.LastSeen);
            Assert.AreEqual(TimeSpan.FromSeconds(3.8 - 1.0), status.CombatTime);

        }

        [TestMethod]
        public void CanPickupItem() {

            // Define item to pick up
            const string slot_item = "ppsh41_assault_package_bg";
            var slot_item_bp = BlueprintManager.FromBlueprintName<SlotItemBlueprint>(slot_item);

            // Analyse and assert
            playStrategy.BattleEvent(TimeSpan.FromSeconds(1), new DeployEvent(0, new string[] { "0", }, SOVIET));
            playStrategy.BattleEvent(TimeSpan.FromSeconds(5), new PickupEvent(1, new string[] { "0", slot_item }, SOVIET));

            // Get and initialize simulated data
            var (data, result) = AnalyseAndAssert(2);

            // Assert unit
            var status = result.Units[0];
            Assert.AreEqual(1, status.CapturedSlotItems.Count);
            Assert.AreEqual(slot_item_bp, status.CapturedSlotItems[0]);

            // Test finalise
            this.finalizeStrategy.Finalize(result);
            this.finalizeStrategy.Synchronize(null);
            Assert.IsNotNull(this.company);

            // Test if company unit got item
            var squad = this.company.GetSquadByIndex(0);
            Assert.AreEqual(1, squad.SlotItems.Count);
            Assert.IsTrue(squad.SlotItems.Contains(slot_item_bp));

        }

        [TestMethod]
        public void CanCaptureTank() {

            // Analyse and assert
            playStrategy.BattleEvent(TimeSpan.FromSeconds(1.5), new DeployEvent(0, new string[] { "0", }, GERMAN));
            playStrategy.BattleEvent(TimeSpan.FromSeconds(2), new DeployEvent(1, new string[] { "0", }, SOVIET));
            playStrategy.BattleEvent(TimeSpan.FromSeconds(8), new KillEvent(2, new string[] { "0", }, GERMAN));
            playStrategy.BattleEvent(TimeSpan.FromSeconds(10), new CaptureEvent(3, new string[] { "panzer_iv_sdkfz_161_bg", "1", "EBP" }, SOVIET));

            // Get and initialize simulated data
            var (data, result) = AnalyseAndAssert(4);

            // Test finalise
            this.finalizeStrategy.Finalize(result);
            this.finalizeStrategy.Synchronize(null);
            Assert.IsNotNull(this.company);

            // Assert is found in company items
            Assert.IsTrue(this.company.Inventory.Contains(BlueprintManager.FromBlueprintName<EntityBlueprint>("panzer_iv_sdkfz_161_bg")));

        }

        [TestMethod]
        public void CanCaptureTeamWeapon() {

            // Analyse and assert
            playStrategy.BattleEvent(TimeSpan.FromSeconds(2), new DeployEvent(0, new string[] { "0", }, SOVIET));
            playStrategy.BattleEvent(TimeSpan.FromSeconds(10), new CaptureEvent(1, new string[] { "mg42_hmg_bg", "2", "EBP" }, SOVIET));

            // Get and initialize simulated data
            var (data, result) = AnalyseAndAssert(2);

            // Test finalise
            this.finalizeStrategy.Finalize(result);
            this.finalizeStrategy.Synchronize(null);
            Assert.IsNotNull(this.company);

            // Assert is found in company items
            Assert.IsTrue(this.company.Inventory.Contains(BlueprintManager.FromBlueprintName<EntityBlueprint>("mg42_hmg_bg")));

        }

    }

}
