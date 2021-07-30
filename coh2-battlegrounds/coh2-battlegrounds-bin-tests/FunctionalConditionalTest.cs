using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Battlegrounds.Functional;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace coh2_battlegrounds_bin_tests {

    [TestClass]
    public class FunctionalConditionalTest {

        [TestMethod]
        public void IsTrueTest1() {
            bool b = true.IfTrue().ToBool();
            Assert.IsTrue(b);
        }

        [TestMethod]
        public void IsTrueTest2() {
            bool b = true.IfTrue().Then(x => !x);
            Assert.IsFalse(b);
        }

        [TestMethod]
        public void IsTrueTest3() {
            bool b = true.IfTrue().Else(x => !x);
            Assert.IsTrue(b);
        }

        [TestMethod]
        public void IsFalseTest1() {
            bool b = false.IfFalse().ToBool();
            Assert.IsTrue(b);
        }

        [TestMethod]
        public void ChainTest1() {
            string s = "Soviet"
                .IfTrue(x => x.CompareTo("German") == 0)
                .ElseIf("American", x => x.CompareTo("German") == 0)
                .ElseIf("German", x => x.CompareTo("German") == 0) // Expected as returning subject
                .Else(x => x);
            Assert.AreEqual("German", s);
        }

        [TestMethod]
        public void ChainTest2() {
            string s = "Soviet"
                .IfTrue(x => x.CompareTo("German") == 0)
                .ElseIf("American", x => x.CompareTo("American") == 0) // Expected as returning subject
                .ElseIf("German", x => x.CompareTo("German") == 0)
                .Else(x => x);
            Assert.AreEqual("American", s);
        }

    }

}
