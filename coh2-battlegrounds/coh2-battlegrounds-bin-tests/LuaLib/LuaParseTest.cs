﻿using Battlegrounds.Lua;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace coh2_battlegrounds_bin_tests.LuaLib {
    
    [TestClass]
    public class LuaParseTest {

        private LuaState lState;

        [TestInitialize]
        public void Init() => lState = new LuaState();

        [TestMethod]
        public void LuaDataParseRunTest01() {
            string sourcefile = @"
settings = {
    a = true,
    b = ""Hello World"",
    c = 1,
    d = 2.3,
    e = false,
    f = nil,
}
";

            // Assert parsing and execution runs fine
            Assert.AreEqual(new LuaNil(), lState.DoString(sourcefile));

            // Assert table values
            Assert.IsNotNull(lState._G["settings"]);
            Assert.AreEqual(new LuaString("Hello World"), (lState._G["settings"] as LuaTable)["b"]);

        }

        [TestMethod]
        public void LuaDataParseRunTest02() {

            string sourcefile = @"
test = {
    a = {
        a = 0,
        b = 4.5
    },
    b = ""nil"",
}
";

            // Assert parsing and execution runs fine
            Assert.AreEqual(new LuaNil(), lState.DoString(sourcefile));

            // Assert table values
            Assert.IsNotNull(lState._G["test"]);
            Assert.AreEqual(new LuaNumber(0), lState._G.ByKey<LuaTable>("test").ByKey<LuaTable>("a").ByKey<LuaNumber>("a"));

        }

        [TestMethod]
        public void LuaDataParseRunTest03() {

            string sourcefile = @"
test = {
    a = {
        a = 0,
        b = 4.5
    },
    b = ""nil"",
}
";

            // Assert parsing and execution runs fine
            Assert.AreEqual(new LuaNil(), lState.DoString(sourcefile));

            // Assert table values
            Assert.IsNotNull(lState._G["test"]);
            Assert.AreEqual(new LuaNumber(0), lState.DoString("test.a.a"));

        }

        [TestMethod]
        public void LuaDataParseRunTest04() {

            string sourcefile = @"
test = {
    a = {
        a = 0,
        b = 4.5
    },
    b = {
        [""Enterprise""] = {
            a = false
        }
    }
}
";

            // Assert parsing and execution runs fine
            Assert.AreEqual(new LuaNil(), lState.DoString(sourcefile));

            // Assert table values
            Assert.IsNotNull(lState._G["test"]);
            Assert.AreEqual(new LuaBool(false), lState.DoString("test.b[\"Enterprise\"].a"));
            Assert.AreEqual(new LuaNumber(4.5), lState.DoString("test.a[\"b\"]"));

        }

    }

}