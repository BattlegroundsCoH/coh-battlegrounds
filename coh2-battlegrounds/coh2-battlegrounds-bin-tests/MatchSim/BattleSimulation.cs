using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Battlegrounds.Game.Gameplay;
using Battlegrounds.Game.Match;
using Battlegrounds.Game.Match.Analyze;
using Battlegrounds.Game.Match.Data;
using Battlegrounds.Game.Match.Data.Events;
using Battlegrounds.Game.Match.Play;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace coh2_battlegrounds_bin_tests.MatchSim {
    
    [TestClass]
    public class BattleSimulation {

        ISession session;
        BattleSimulatorStrategy playStrategy;
        IAnalyzeStrategy analyzeStrategy;
        Player[] players;

        private Player SOVIET => players[0];
        private Player GERMAN => players[1];

        [TestInitialize]
        public void Setup() {
            session = new NullSession();
            playStrategy = new BattleSimulatorStrategy(session);
            analyzeStrategy = new SingleplayerMatchAnalyzer();
            players = new Player[] {
                new Player (0, 0, "Player 1", Faction.Soviet, string.Empty),
                new Player (1, 1, "Player 2", Faction.Wehrmacht, string.Empty),
            };
        }

        [TestMethod]
        public void CanDeployAndKillWithoutError() {
            
            // Create events
            playStrategy.BattleEvent(TimeSpan.FromSeconds(1), new DeployEvent(0, new string[] { "0", }, SOVIET));
            playStrategy.BattleEvent(TimeSpan.FromSeconds(3.8), new KillEvent(1, new string[] { "0", }, SOVIET));
            
            // Get and initialize simulated data
            SimulatedMatchData data = playStrategy.GetResults() as SimulatedMatchData;
            data.CreatePlayer(SOVIET);
            data.CreatePlayer(GERMAN);

            // Verify simulated data is registered
            Assert.IsNotNull(data);
            Assert.AreEqual(2, data.Count());
            Assert.AreEqual(TimeSpan.FromSeconds(3.8), data.Length);
            
            // Put through analysis
            analyzeStrategy.OnPrepare(null, data);
            analyzeStrategy.OnAnalyze(null);
            
            // Compile analysis
            var result = analyzeStrategy.OnCleanup(null);
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(EventAnalysis));
            Assert.AreEqual(true, result.CompileResults());
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

            // Get and initialize simulated data
            SimulatedMatchData data = playStrategy.GetResults() as SimulatedMatchData;
            data.CreatePlayer(SOVIET);
            data.CreatePlayer(GERMAN);

            // Verify simulated data is registered
            Assert.IsNotNull(data);
            Assert.AreEqual(2, data.Count());
            Assert.AreEqual(TimeSpan.FromSeconds(3.8), data.Length);

            // Put through analysis
            analyzeStrategy.OnPrepare(null, data);
            analyzeStrategy.OnAnalyze(null);

            // Compile analysis
            var result = analyzeStrategy.OnCleanup(null);
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(EventAnalysis));
            Assert.AreEqual(true, result.CompileResults());
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
    }

}
