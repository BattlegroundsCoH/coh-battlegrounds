using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Battlegrounds.Game.DataSource.Replay;
using Battlegrounds.Game.Match;
using Battlegrounds.Game.Match.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace coh2_battlegrounds_bin_tests.MatchSim {
    
    [TestClass]
    public class PlaybackData {

        ISession session;

        [TestInitialize]
        public void Setup() {
            this.session = new NullSession();
        }

        private static ReplayFile Load3rdm() {
            var assstream = Assembly.GetExecutingAssembly().GetManifestResourceStream("coh2_battlegrounds_bin_tests.MatchSim.Data.3rdm.rec");
            Assert.IsNotNull(assstream);
            var resstream = new BinaryReader(assstream);
            ReplayFile replayFile = new ReplayFile();
            Assert.AreEqual(true, replayFile.LoadReplay(resstream));
            return replayFile;
        }

        [TestMethod]
        public void CanLoad3rdm() => Assert.IsNotNull(Load3rdm());

        [TestMethod]
        public void CanParse3rdm() {

            // Create replay
            ReplayMatchData rmd = new ReplayMatchData(session);
            rmd.SetReplayFile(Load3rdm());

            // Try parse
            bool isParsing = rmd.ParseMatchData();

            // Assert we can parse the match data
            Assert.AreEqual(true, isParsing);

        }

    }

}
