using System.Diagnostics;

using Battlegrounds.Online.Services;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace coh2_battlegrounds_bin_tests {

    [TestClass]
    public class SyncTest {

        [TestMethod]
        public void TestIsAccurateTime() {
            var timer = Stopwatch.StartNew();
            bool timedOut = SyncService.WaitUntil(() => false, 100, 10);
            timer.Stop();
            Assert.IsTrue(timedOut);
            Assert.IsTrue(timer.ElapsedMilliseconds > 900 && timer.ElapsedMilliseconds <= 1100); // Allow for an error margin of 10%
        }

        [TestMethod]
        public void TestWillEnd() {
            var timer = Stopwatch.StartNew();
            bool timedOut = SyncService.WaitUntil(() => timer.ElapsedMilliseconds >= 750, 100, 10);
            timer.Stop();
            Assert.IsFalse(timedOut);
            Assert.IsTrue(timer.ElapsedMilliseconds > 700 && timer.ElapsedMilliseconds <= 850);
        }

        [TestMethod]
        public void TestWillTriggerThenIfTimedOut() {
            bool changed = false;
            var timer = Stopwatch.StartNew();
            bool timedOut = SyncService.WaitUntil(() => false, 100, 10).Then(() => changed = true);
            timer.Stop();
            Assert.IsTrue(timedOut);
            Assert.IsTrue(changed);
            Assert.IsTrue(timer.ElapsedMilliseconds > 900 && timer.ElapsedMilliseconds <= 1100); // Allow for an error margin of 10%
        }

        [TestMethod]
        public void TestWillNotTriggerThenIfNotTimedOut() {
            bool changed = false;
            var timer = Stopwatch.StartNew();
            bool timedOut = SyncService.WaitUntil(() => timer.ElapsedMilliseconds >= 750, 100, 10).Then(() => changed = true);
            timer.Stop();
            Assert.IsFalse(timedOut);
            Assert.IsFalse(changed);
            Assert.IsTrue(timer.ElapsedMilliseconds > 700 && timer.ElapsedMilliseconds <= 850);
        }

    }

}
