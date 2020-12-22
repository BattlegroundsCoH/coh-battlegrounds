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

    }

}
