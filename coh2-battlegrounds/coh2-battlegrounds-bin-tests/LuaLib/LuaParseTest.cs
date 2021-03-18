using Battlegrounds.Lua;
using Battlegrounds.Lua.Parsing;
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
            Assert.AreEqual(new LuaNil(), LuaVM.DoString(lState, sourcefile));

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
            Assert.AreEqual(new LuaNil(), LuaVM.DoString(lState, sourcefile));

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
            Assert.AreEqual(new LuaNil(), LuaVM.DoString(lState, sourcefile));

            // Assert table values
            Assert.IsNotNull(lState._G["test"]);
            Assert.AreEqual(new LuaNumber(0), LuaVM.DoString(lState, "test.a.a"));

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
            Assert.AreEqual(new LuaNil(), LuaVM.DoString(lState, sourcefile));

            // Assert table values
            Assert.IsNotNull(lState._G["test"]);
            Assert.AreEqual(new LuaBool(false), LuaVM.DoString(lState, "test.b[\"Enterprise\"].a"));
            Assert.AreEqual(new LuaNumber(4.5), LuaVM.DoString(lState, "test.a[\"b\"]"));

        }

        [TestMethod]
        public void LuaDataParseRunTest05() {

            string sourcefile = @"
test = {
    a = {
        a = 0,
        b = 4.5
    },
    b = {
        [""Enterprise""] = {
            a = false
        },
        [""Shenzhou""] = {
            a = true
        }
    }
}
";

            // Assert parsing and execution runs fine
            Assert.AreEqual(new LuaNil(), LuaVM.DoString(lState, sourcefile));

            // Assert table values
            Assert.IsNotNull(lState._G["test"]);
            Assert.AreEqual(new LuaNumber(4.5), LuaVM.DoString(lState, "test.a[\"b\"]"));
            Assert.AreEqual(new LuaBool(false), LuaVM.DoString(lState, "test.b[\"Enterprise\"].a"));
            Assert.AreEqual(new LuaBool(true), LuaVM.DoString(lState, "test.b[\"Shenzhou\"].a"));
            Assert.AreEqual(lState.InitialGlobalSize + 1, lState._G.Size);

        }

        [TestMethod]
        public void LuaDataParseRunTest06() {

            string sourcefile = @"
test = {
    a = {
        a = 0,
        b = 4.5
    },
    b = {
        {
            a = ""Enterprise""
        },
        {
            a = ""Shenzhou""
        }
    }
}
";

            // Assert parsing and execution runs fine
            Assert.AreEqual(new LuaNil(), LuaVM.DoString(lState, sourcefile));

            // Assert table values
            Assert.IsNotNull(lState._G["test"]);
            Assert.AreEqual(new LuaNumber(4.5), LuaVM.DoString(lState, "test.a[\"b\"]"));
            Assert.AreEqual(new LuaString("Enterprise"), LuaVM.DoString(lState, "test.b[1].a"));
            Assert.AreEqual(new LuaString("Shenzhou"), LuaVM.DoString(lState, "test.b[2].a"));
            Assert.AreEqual(lState.InitialGlobalSize + 1, lState._G.Size);

        }

        [TestMethod]
        public void LuaDataParseRunTest07() {

            string sourcefile = @"5 + 5";

            // Assert parsing and execution runs fine
            Assert.AreEqual(new LuaNumber(10), LuaVM.DoString(lState, sourcefile));

        }

        [TestMethod]
        public void LuaDataParseRunTest08() {

            string sourcefile = @"5 / 5";

            // Assert parsing and execution runs fine
            Assert.AreEqual(new LuaNumber(1), LuaVM.DoString(lState, sourcefile));

        }

        [TestMethod]
        public void LuaDataParseRunTest09() {

            string sourcefile = @"{ e = 5 / 5, b = 5 + 5 }";

            // Assert parsing and execution runs fine
            var val = LuaVM.DoString(lState, sourcefile);
            Assert.IsInstanceOfType(val, typeof(LuaTable));
            Assert.AreEqual(new LuaNumber(1), (val as LuaTable)["e"]);
            Assert.AreEqual(new LuaNumber(10), (val as LuaTable)["b"]);


        }

        [TestMethod]
        public void LuaDataParseRunTest10() {

            // Test string
            string sourcefile = @"{ ""a"", ""b"", ""c"" }";

            // Assert parsing and execution runs fine
            var val = LuaVM.DoString(lState, sourcefile);
            Assert.IsInstanceOfType(val, typeof(LuaTable));
            Assert.AreEqual(new LuaString("a"), (val as LuaTable).RawIndex<LuaString>(0));
            Assert.AreEqual(new LuaString("b"), (val as LuaTable).RawIndex<LuaString>(1));
            Assert.AreEqual(new LuaString("c"), (val as LuaTable).RawIndex<LuaString>(2));

        }

        [TestMethod]
        public void LuaDataParseRunTest11() {

            // Test string
            string sourcefile = @"{ "" a "", ""  b"", ""c  "" }";

            // Assert parsing and execution runs fine
            var val = LuaVM.DoString(lState, sourcefile);
            Assert.IsInstanceOfType(val, typeof(LuaTable));
            Assert.AreEqual(new LuaString(" a "), (val as LuaTable).RawIndex<LuaString>(0));
            Assert.AreEqual(new LuaString("  b"), (val as LuaTable).RawIndex<LuaString>(1));
            Assert.AreEqual(new LuaString("c  "), (val as LuaTable).RawIndex<LuaString>(2));

        }

        [TestMethod]
        public void LuaDataParseRuntest12() {

            // Test string
            string sourceText = @"-5";

            // Assertions
            var val = LuaVM.DoString(lState, sourceText);
            Assert.IsInstanceOfType(val, typeof(LuaNumber));
            Assert.AreEqual(new LuaNumber(-5), val);

        }

        [TestMethod]
        public void LuaDataParseRuntest13() {

            // Test string
            string sourceText = @"-5 * 6";

            // Assertions
            var val = LuaVM.DoString(lState, sourceText);
            Assert.IsInstanceOfType(val, typeof(LuaNumber));
            Assert.AreEqual(new LuaNumber(-30), val);

        }

        [TestMethod]
        public void LuaDataParseRuntest14() {

            // Test string
            string sourceText = @"-5 * 0.5";

            // Assertions
            var val = LuaVM.DoString(lState, sourceText);
            Assert.IsInstanceOfType(val, typeof(LuaNumber));
            Assert.AreEqual(new LuaNumber(-2.5), val);

        }

        [TestMethod]
        public void LuaDataParseRuntest15() {

            // Test string
            string sourceText = @"{ 0, 1, -5, -7 * 0.5, 5 }";

            // Assertions
            var val = LuaVM.DoString(lState, sourceText);
            Assert.IsInstanceOfType(val, typeof(LuaTable));

            // Get table
            var tab = val as LuaTable;
            Assert.AreEqual(new LuaNumber(0), tab[1]);
            Assert.AreEqual(new LuaNumber(1), tab[2]);
            Assert.AreEqual(new LuaNumber(-5), tab[3]);
            Assert.AreEqual(new LuaNumber(-3.5), tab[4]);
            Assert.AreEqual(new LuaNumber(5), tab[5]);

        }

        [TestMethod]
        public void LuaDataParseRuntest16() {

            // Test string
            string sourceText = @"-""hello""";

            // Assertions
            var val = LuaVM.DoString(lState, sourceText);
            Assert.IsInstanceOfType(val, typeof(LuaNil));

            var ex = lState.GetError();
            Assert.IsNotNull(ex);
            Assert.AreEqual("Attempt to perform arithmetic on a string value.", ex.Message);

        }

        [TestMethod]
        public void LuaFunctionalTest01() {

            // src, the below is equivalent to [[ ds9 = function() print("Hello Space") end ]]
            string sourceText = @"
            function ds9()
                print(""Hello Space"")
            end
            ";

            var luaAST = LuaParser.ParseLuaSource(sourceText);
            Assert.IsInstanceOfType(luaAST[0], typeof(LuaBinaryExpr));

            var top = luaAST[0] as LuaBinaryExpr;
            Assert.AreEqual(new LuaIdentifierExpr("ds9"), top.Left);
            Assert.IsInstanceOfType(top.Right, typeof(LuaFuncExpr));

        }

        [TestMethod]
        public void LuaFunctionalTest02() {

            string sourceText = @"
            ds9 = function()
                print(""Hello Space"")
            end
            ";

            var luaAST = LuaParser.ParseLuaSource(sourceText);
            Assert.IsInstanceOfType(luaAST[0], typeof(LuaBinaryExpr));

            var top = luaAST[0] as LuaBinaryExpr;
            Assert.AreEqual(new LuaIdentifierExpr("ds9"), top.Left);
            Assert.IsInstanceOfType(top.Right, typeof(LuaFuncExpr));

        }

        [TestMethod]
        public void LuaFunctionalTest03() {

            string sourceText = @"
            function dumdum(a, b)
                print(a)
                print(b)
            end
            dumdum(42, 69)
            ";

            var luaAST = LuaParser.ParseLuaSource(sourceText);
            Assert.IsInstanceOfType(luaAST[0], typeof(LuaBinaryExpr));
            Assert.IsInstanceOfType(luaAST[1], typeof(LuaCallExpr));

            var top = luaAST[0] as LuaBinaryExpr;
            Assert.AreEqual(new LuaIdentifierExpr("dumdum"), top.Left);
            Assert.IsInstanceOfType(top.Right, typeof(LuaFuncExpr));
            Assert.AreEqual(2, (top.Right as LuaFuncExpr).Arguments.Arguments.Count);

            var bottom = luaAST[1] as LuaCallExpr;
            Assert.AreEqual(2, bottom.Arguments.Arguments.Count);

        }

        [TestMethod]
        public void LuaFunctionalTest04() {

            string sourceText = @"
            function dumdum(a, b)
                return a +  b, a * b
            end
            a, b = dumdum(42, 69)
            print(a)
            print(b)
            ";

            var luaAST = LuaParser.ParseLuaSource(sourceText);
            Assert.IsInstanceOfType(luaAST[0], typeof(LuaBinaryExpr));
            Assert.IsInstanceOfType(luaAST[1], typeof(LuaBinaryExpr));

            var top = luaAST[0] as LuaBinaryExpr;
            Assert.AreEqual(new LuaIdentifierExpr("dumdum"), top.Left);
            Assert.IsInstanceOfType(top.Right, typeof(LuaFuncExpr));
            Assert.AreEqual(2, (top.Right as LuaFuncExpr).Arguments.Arguments.Count);

            var body = (top.Right as LuaFuncExpr).Body;
            Assert.IsInstanceOfType(body.ScopeBody[0], typeof(LuaReturnStatement));

            var bottom = luaAST[1] as LuaBinaryExpr;
            Assert.IsInstanceOfType(bottom.Left, typeof(LuaTupleExpr));
            Assert.IsInstanceOfType(bottom.Right, typeof(LuaCallExpr));

        }

        [TestMethod]
        public void LuaFunctionalTest05() {

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

            var luaAST = LuaParser.ParseLuaSource(sourceText);
            Assert.IsInstanceOfType(luaAST[0], typeof(LuaAssignExpr));
            Assert.IsTrue(luaAST[0] is LuaAssignExpr { Local: true });

            Assert.IsInstanceOfType(luaAST[1], typeof(LuaAssignExpr));
            Assert.IsTrue(luaAST[1] is LuaAssignExpr { Local: true });

        }

        [TestMethod]
        public void LuaFunctionalTest06() {

            string sourceText = @"
            local k = 0
            while k < 50 do
                print(k)
            end
            ";

            // Parse and verify top-level
            var luaAST = LuaParser.ParseLuaSource(sourceText);
            Assert.IsInstanceOfType(luaAST[0], typeof(LuaAssignExpr));
            Assert.IsTrue(luaAST[0] is LuaAssignExpr { Local: true });
            Assert.IsInstanceOfType(luaAST[1], typeof(LuaWhileStatement));

            // Verify AST
            var whileStatement = luaAST[1] as LuaWhileStatement;
            Assert.IsInstanceOfType(whileStatement.Condition, typeof(LuaBinaryExpr));
            Assert.IsInstanceOfType(whileStatement.Body.ScopeBody[0], typeof(LuaCallExpr));

        }


    }

}
