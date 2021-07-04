using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Battlegrounds.Game.DataSource.Replay;
using Battlegrounds.Game.Match;
using Battlegrounds.Game.Match.Analyze;
using Battlegrounds.Game.Match.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace coh2_battlegrounds_bin_tests.MatchSim {
    
    [TestClass]
    public class ReplayData {

        ISession session;
        IAnalyzeStrategy analyzeStrategy;

        [TestInitialize]
        public void Setup() {
            this.session = new NullSession();
            this.analyzeStrategy = new SingleplayerMatchAnalyzer();
        }

        private static ReplayMatchData GetIdCheckPlayback() {
            var assstream = Assembly.GetExecutingAssembly().GetManifestResourceStream("coh2_battlegrounds_bin_tests.MatchSim.Data.idchk.rec");
            Assert.IsNotNull(assstream);
            var resstream = new BinaryReader(assstream);
            ReplayFile replayFile = new ReplayFile();
            Assert.AreEqual(true, replayFile.LoadReplay(resstream));
            Assert.AreEqual(true, replayFile.IsParsed);
            var data = new ReplayMatchData(new NullSession());
            data.SetReplayFile(replayFile);
            Assert.AreEqual(true, data.ParseMatchData());
            return data;
        }

        [TestMethod]
        public void IsAnalyzable() {
            var matchdata = GetIdCheckPlayback();
            analyzeStrategy.OnPrepare(null, matchdata);
            analyzeStrategy.OnAnalyze(null);            ;
            Assert.IsTrue(analyzeStrategy.OnCleanup(null) is not NullAnalysis);
        }

    }

}
