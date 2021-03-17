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
        TextReader reader;
        StringBuilder writerOutput;

        [TestInitialize]
        public void Setup() {
            writerOutput = new StringBuilder();
            lState = new LuaState("base") {
                Out = writer = new StringWriter(writerOutput),
                In = reader = new StreamReader(new MemoryStream())
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

    }

}
