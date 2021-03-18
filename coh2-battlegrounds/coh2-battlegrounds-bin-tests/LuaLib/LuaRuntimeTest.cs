using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Battlegrounds.Lua;
using Battlegrounds.Lua.Runtime;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace coh2_battlegrounds_bin_tests.LuaLib {

    [TestClass]    
    public class LuaRuntimeTest {

        LuaState lState;
        TextWriter writer;
        StringBuilder writerOutput;

        [TestInitialize]
        public void Setup() {
            writerOutput = new StringBuilder();
            lState = new LuaState("base") {
                Out = writer = new StringWriter(writerOutput),
            };
        }

        [TestMethod]
        public void HasBaseLoaded() {
            Assert.AreNotEqual(new LuaNil(), lState._G["print"]);
            var print = lState._G["print"];
            Assert.IsInstanceOfType(print, typeof(LuaClosure));
            Assert.IsTrue((print as LuaClosure).Function.IsCFunction);
        }

        [TestMethod]
        public void SimpleFunctionTest01() {

            string sourceText = @"
            function ds9()
                print(""Hello Space"")
            end
            ds9()
            ";

            var result = LuaVM.DoString(this.lState, sourceText);
            Assert.AreEqual(new LuaNil(), result);
            Assert.AreEqual($"Hello Space{writer.NewLine}", writerOutput.ToString());

        }

        [TestMethod]
        public void SimpleFunctionTest02() {

            string sourceText = @"
            function dumdum(a, b)
                print(a)
                print(b)
            end
            dumdum(42, 69)
            print(a)
            ";

            var result = LuaVM.DoString(this.lState, sourceText);
            Assert.AreEqual(new LuaNil(), result);
            Assert.AreEqual($"42{writer.NewLine}69{writer.NewLine}nil{writer.NewLine}", writerOutput.ToString());

        }

        [TestMethod]
        public void SimpleFunctionTest03() {

            string sourceText = @"
            function dumdum(a, b)
                return a +  b, a * b
            end
            a, b = dumdum(42, 69)
            print(a)
            print(b)";

            var result = LuaVM.DoString(this.lState, sourceText);
            Assert.AreEqual(new LuaNil(), result);
            Assert.AreEqual($"111{writer.NewLine}2898{writer.NewLine}", writerOutput.ToString());

        }

        [TestMethod]
        public void SimpleFunctionTest04() {

            string sourceText = @"
            function dumdum(a, b)
                return a +  b
            end
            a, b = dumdum(42, 69)
            print(a)
            print(b)";

            var result = LuaVM.DoString(this.lState, sourceText);
            Assert.AreEqual(new LuaNil(), result);
            Assert.AreEqual($"111{writer.NewLine}nil{writer.NewLine}", writerOutput.ToString());

        }

        [TestMethod]
        public void SimpleFunctionTest05() {

            string sourceText = @"
            local k = 2
            local function dumdum(a, b)
                local k = k + 1 -- Should not modify k in outer scope
                return a + b + k, a * b - k
            end
            a, b = dumdum(42, 69)
            c, d = dumdum(a + k, b / k)
            print(a .. "":"" .. b)
            print(c .. "":"" .. d)
            print(k)";

            // Run and check output
            var result = LuaVM.DoString(this.lState, sourceText);
            Assert.AreEqual(new LuaNil(), result);

            string[] lns = writerOutput.ToString().Split(writer.NewLine);
            Assert.AreEqual(4, lns.Length);
            Assert.AreEqual("114:2895", lns[0]);
            Assert.AreEqual("1566.5:167907", lns[1]);
            Assert.AreEqual("2", lns[2]);

            // Verify locals are done correctly
            Assert.AreEqual(new LuaNil(), this.lState._G["k"]);
            Assert.AreEqual(new LuaNil(), this.lState._G["dumdum"]);
            Assert.AreEqual(new LuaNumber(2), this.lState.Envionment["k"]);
            Assert.AreNotEqual(new LuaNil(), this.lState.Envionment["dumdum"]);

        }


        [TestMethod]
        public void SimpleFunctionTest06() {

            string sourceText = @"
            k = 0
            while k < 50 do
                print(k)
                k = k + 1
            end
            ";

            // Run and check output
            var result = LuaVM.DoString(this.lState, sourceText);
            Assert.AreEqual(new LuaNil(), result);

            string[] lns = writerOutput.ToString().Split(writer.NewLine);
            Assert.AreEqual(51, lns.Length);

            for (int i = 0; i < 50; i++) {
                Assert.AreEqual(i.ToString(), lns[i]);
            }

        }
    }

}
