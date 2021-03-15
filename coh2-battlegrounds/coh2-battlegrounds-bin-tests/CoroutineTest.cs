using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Battlegrounds.Util.Coroutines;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace coh2_battlegrounds_bin_tests {

    [TestClass]
    public class CoroutineTest {
    
        [TestMethod]
        public void TestCoroutine01() {

            List<string> test = new List<string>();

            IEnumerator TestFunc() {
                test.Add("Hello");
                yield return null;
                test.Add("World");
            }

            Coroutine.StartCoroutine(TestFunc());
            Assert.IsTrue(test.Count != 2);

            int cntr = 0;
            while(test.Count != 2 && cntr < 10) {
                Thread.Sleep(100);
                cntr++;
            }

            Assert.IsTrue(cntr < 10);
            Assert.AreEqual("Hello", test[0]);
            Assert.AreEqual("World", test[1]);

        }

        [TestMethod]
        public void TestCoroutine02() {

            List<string> test = new List<string>();

            IEnumerator TestFunc() {
                test.Add("Hello");
                yield return new WaitTimespan(TimeSpan.FromSeconds(1.2));
                test.Add("World");
                yield return new WaitTimespan(TimeSpan.FromSeconds(0.8));
                test.Add("!");
            }

            Coroutine.StartCoroutine(TestFunc());
            Assert.IsTrue(test.Count != 3);

            int cntr = 0;
            while (test.Count != 3 && cntr < 40) {
                Thread.Sleep(100);
                cntr++;
            }

            Assert.IsTrue(cntr < 40);
            Assert.AreEqual("Hello", test[0]);
            Assert.AreEqual("World", test[1]);
            Assert.AreEqual("!", test[2]);

        }

    }

}
