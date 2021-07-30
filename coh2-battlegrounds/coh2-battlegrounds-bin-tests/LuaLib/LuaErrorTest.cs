using Battlegrounds.Lua.Debugging;
using Battlegrounds.Lua.Parsing;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace coh2_battlegrounds_bin_tests.LuaLib {

    [TestClass]
    public class LuaErrorTest {

        [TestMethod]
        [TestCategory("Syntax Error - if ... end")]
        public void LuaSyntaxError01() {

            // Verify correct
            LuaSyntaxError lse = LuaParser.ParseLuaSourceSafe(out _, "if true then print(5) end");
            Assert.IsNull(lse);

            // Verify if
            lse = LuaParser.ParseLuaSourceSafe(out _, "if true print(5) end");
            Assert.IsNotNull(lse);
            Assert.AreEqual("'then' expected before 'print'", lse.Message);

            // Verify if
            lse = LuaParser.ParseLuaSourceSafe(out _, "if true print(5)");
            Assert.IsNotNull(lse);
            Assert.AreEqual("'then' expected before 'print'", lse.Message);

            // Verify if
            lse = LuaParser.ParseLuaSourceSafe(out _, "if 5 + 5 print(5)");
            Assert.IsNotNull(lse);
            Assert.AreEqual("'then' expected before 'print'", lse.Message);

            // Verify if
            lse = LuaParser.ParseLuaSourceSafe(out _, "if true then print(5)");
            Assert.IsNotNull(lse);
            Assert.AreEqual("'end' expected (to close 'if' at line 1) near <eof>", lse.Message);

        }

        [TestMethod]
        [TestCategory("Syntax Error - while")]
        public void LuaSyntaxError02() {

            // Verify correct
            LuaSyntaxError lse = LuaParser.ParseLuaSourceSafe(out _, "while true do print(true) end");
            Assert.IsNull(lse);

            // Verify while
            lse = LuaParser.ParseLuaSourceSafe(out _, "while true print(true) end");
            Assert.IsNotNull(lse);
            Assert.AreEqual("'do' expected before 'print'", lse.Message);

            // Verify while
            lse = LuaParser.ParseLuaSourceSafe(out _, "while 5+5 print(true) end");
            Assert.IsNotNull(lse);
            Assert.AreEqual("'do' expected before 'print'", lse.Message);

            // Verify while
            lse = LuaParser.ParseLuaSourceSafe(out _, "while true do print(true)");
            Assert.IsNotNull(lse);
            Assert.AreEqual("'end' expected (to close 'while' at line 1) near <eof>", lse.Message);

        }

        [TestMethod]
        [TestCategory("Syntax Error - for (numeric)")]
        public void LuaSyntaxError03() {

            // Verify correct
            LuaSyntaxError lse = LuaParser.ParseLuaSourceSafe(out _, "for i=1, 10, 1 do print(i) end");
            Assert.IsNull(lse);

            // Verify for
            lse = LuaParser.ParseLuaSourceSafe(out _, "for i=1, 10, 1 print(i) end");
            Assert.IsNotNull(lse);
            Assert.AreEqual("'do' expected near 'print'", lse.Message);

            // Verify for
            lse = LuaParser.ParseLuaSourceSafe(out _, "for i=1, 10, 1, print(i) end");
            Assert.IsNotNull(lse);
            Assert.AreEqual("'do' expected near ','", lse.Message);

            // Verify for
            lse = LuaParser.ParseLuaSourceSafe(out _, "for i=1, 10, 1 do print(i)");
            Assert.IsNotNull(lse);
            Assert.AreEqual("'end' expected (to close 'for' at line 1) near <eof>", lse.Message);

            // Verify for
            lse = LuaParser.ParseLuaSourceSafe(out _, "for i=1, 10 do print(i)");
            Assert.IsNotNull(lse);
            Assert.AreEqual("'end' expected (to close 'for' at line 1) near <eof>", lse.Message);

        }

        [TestMethod]
        [TestCategory("Syntax Error - for (generic)")]
        public void LuaSyntaxError04() {

        }

        [TestMethod]
        [TestCategory("Syntax Error - Basic syntax")]
        public void LuaSyntaxError05() {

            // Verify invalid
            LuaSyntaxError lse = LuaParser.ParseLuaSourceToChunk(out _, "5+5");
            Assert.IsNotNull(lse);
            Assert.AreEqual("unexpected number '5' near '+'", lse.Message);

            // Verify invalid
            lse = LuaParser.ParseLuaSourceToChunk(out _, "if true then 5*5 end");
            Assert.IsNotNull(lse);
            Assert.AreEqual("unexpected number '5' near '*'", lse.Message);

        }

    }

}
