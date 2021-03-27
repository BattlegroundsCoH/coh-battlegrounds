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
            Assert.AreEqual(new LuaBool(true), (lState._G["settings"] as LuaTable)["a"]);
            Assert.AreEqual(new LuaString("Hello World"), (lState._G["settings"] as LuaTable)["b"]);
            Assert.AreEqual(new LuaNumber(1), (lState._G["settings"] as LuaTable)["c"]);
            Assert.AreEqual(new LuaNumber(2.3), (lState._G["settings"] as LuaTable)["d"]);
            Assert.AreEqual(new LuaBool(false), (lState._G["settings"] as LuaTable)["e"]);
            Assert.AreEqual(new LuaNil(), (lState._G["settings"] as LuaTable)["f"]);

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
            }";

            // Assert parsing and execution runs fine
            Assert.AreEqual(new LuaNil(), LuaVM.DoString(lState, sourcefile));

            // Assert table values
            Assert.IsNotNull(lState._G["test"]);

            var table = lState._G["test"] as LuaTable;
            Assert.AreEqual(new LuaNumber(0), table.ByKey<LuaTable>("a").ByKey<LuaNumber>("a"));
            Assert.AreEqual(new LuaNumber(4.5), table.ByKey<LuaTable>("a").ByKey<LuaNumber>("b"));
            Assert.AreEqual(new LuaString("nil"), table.ByKey<LuaString>("b"));

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
            Assert.AreEqual(new LuaNumber(0), LuaVM.DoRawString(lState, "test.a.a"));

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
            Assert.AreEqual(new LuaBool(false), LuaVM.DoRawString(lState, "test.b[\"Enterprise\"].a"));
            Assert.AreEqual(new LuaNumber(4.5), LuaVM.DoRawString(lState, "test.a[\"b\"]"));

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
            Assert.AreEqual(new LuaNumber(4.5), LuaVM.DoRawString(lState, "return test.a[\"b\"]"));
            Assert.AreEqual(new LuaBool(false), LuaVM.DoRawString(lState, "test.b[\"Enterprise\"].a"));
            Assert.AreEqual(new LuaBool(true), LuaVM.DoRawString(lState, "test.b[\"Shenzhou\"].a"));
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
            Assert.AreEqual(new LuaNumber(4.5), LuaVM.DoRawString(lState, "test.a[\"b\"]"));
            Assert.AreEqual(new LuaString("Enterprise"), LuaVM.DoRawString(lState, "test.b[1].a"));
            Assert.AreEqual(new LuaString("Shenzhou"), LuaVM.DoRawString(lState, "test.b[2].a"));
            Assert.AreEqual(lState.InitialGlobalSize + 1, lState._G.Size);

        }

        [TestMethod]
        public void LuaDataParseRunTest07() {

            string sourcefile = @"return 5 + 5";

            // Assert parsing and execution runs fine
            Assert.AreEqual(new LuaNumber(10), LuaVM.DoString(lState, sourcefile));

        }

        [TestMethod]
        public void LuaDataParseRunTest08() {

            string sourcefile = @"return 5 / 5";

            // Assert parsing and execution runs fine
            Assert.AreEqual(new LuaNumber(1), LuaVM.DoString(lState, sourcefile));

        }

        [TestMethod]
        public void LuaDataParseRunTest09() {

            string sourcefile = @"return { e = 5 / 5, b = 5 + 5 }";

            // Assert parsing and execution runs fine
            var val = LuaVM.DoString(lState, sourcefile);
            Assert.IsInstanceOfType(val, typeof(LuaTable));
            Assert.AreEqual(new LuaNumber(1), (val as LuaTable)["e"]);
            Assert.AreEqual(new LuaNumber(10), (val as LuaTable)["b"]);

        }

        [TestMethod]
        public void LuaDataParseRunTest10() {

            // Test string
            string sourcefile = @"return { ""a"", ""b"", ""c"" }";

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
            string sourcefile = @"return { "" a "", ""  b"", ""c  "" }";

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
            string sourceText = @"return -5";

            // Assertions
            var val = LuaVM.DoString(lState, sourceText);
            Assert.IsInstanceOfType(val, typeof(LuaNumber));
            Assert.AreEqual(new LuaNumber(-5), val);

        }

        [TestMethod]
        public void LuaDataParseRuntest13() {

            // Test string
            string sourceText = @"return -5 * 6";

            // Assertions
            var val = LuaVM.DoString(lState, sourceText);
            Assert.IsInstanceOfType(val, typeof(LuaNumber));
            Assert.AreEqual(new LuaNumber(-30), val);

        }

        [TestMethod]
        public void LuaDataParseRuntest14() {

            // Test string
            string sourceText = @"return -5 * 0.5";

            // Assertions
            var val = LuaVM.DoString(lState, sourceText);
            Assert.IsInstanceOfType(val, typeof(LuaNumber));
            Assert.AreEqual(new LuaNumber(-2.5), val);

        }

        [TestMethod]
        public void LuaDataParseRuntest15() {

            // Test string
            string sourceText = @"return { 0, 1, -5, -7 * 0.5, 5 }";

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
            string sourceText = @"return -""hello""";

            // Assertions
            var val = LuaVM.DoString(lState, sourceText);
            Assert.IsInstanceOfType(val, typeof(LuaNil));

            var ex = lState.GetError();
            Assert.IsNotNull(ex);
            Assert.AreEqual("Attempt to perform arithmetic on a string value.", ex.Message);

        }

        [TestMethod]
        public void LuaDataParseRuntest17() {

            // Various NOT parse-run tests
            Assert.AreEqual(new LuaBool(true), LuaVM.DoString(lState, "return not nil"));
            Assert.AreEqual(new LuaBool(true), LuaVM.DoString(lState, "return not false"));
            Assert.AreEqual(new LuaBool(false), LuaVM.DoString(lState, "return not true"));
            Assert.AreEqual(new LuaBool(false), LuaVM.DoString(lState, "return not \"Hello\""));

        }

        [TestMethod]
        public void LuaDataParseRuntest18() {

            // Various OR and AND parse-run tests (From the lua reference)
            Assert.AreEqual(new LuaNumber(10), LuaVM.DoString(lState, "return 10 or 20"));
            Assert.AreEqual(new LuaNumber(10), LuaVM.DoString(lState, "return 10 or error()"));
            Assert.AreEqual(new LuaString("a"), LuaVM.DoString(lState, "return nil or \"a\""));
            Assert.AreEqual(new LuaNil(), LuaVM.DoString(lState, "return nil and 10"));
            Assert.AreEqual(new LuaBool(false), LuaVM.DoString(lState, "return false and nil"));
            Assert.AreEqual(new LuaBool(false), LuaVM.DoString(lState, "return false and error()"));
            Assert.AreEqual(new LuaNil(), LuaVM.DoString(lState, "return false or nil"));
            Assert.AreEqual(new LuaNumber(20), LuaVM.DoString(lState, "return 10 and 20"));

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
            Assert.AreEqual("ds9", (top.Left as LuaIdentifierExpr).Identifier);
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
            Assert.AreEqual("ds9", (top.Left as LuaIdentifierExpr).Identifier);
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
            Assert.AreEqual("dumdum", (top.Left as LuaIdentifierExpr).Identifier);
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
            Assert.AreEqual("dumdum", (top.Left as LuaIdentifierExpr).Identifier);
            Assert.IsInstanceOfType(top.Right, typeof(LuaFuncExpr));
            Assert.AreEqual(2, (top.Right as LuaFuncExpr).Arguments.Arguments.Count);

            var body = (top.Right as LuaFuncExpr).Body;
            Assert.IsInstanceOfType(body.ScopeBody[0], typeof(LuaReturnStatement));

            var bottom = luaAST[1] as LuaBinaryExpr;
            Assert.IsInstanceOfType(bottom.Left, typeof(LuaExpressionList));
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
                k = k + 1
            end";

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

        [TestMethod]
        public void LuaFunctionalTest07() {

            string sourceText = @"
            for k = 1, 8, 2 do -- Numeric for statement
                print(k)
            end";

            // Parse and verify top-level
            var luaAST = LuaParser.ParseLuaSource(sourceText);
            Assert.IsInstanceOfType(luaAST[0], typeof(LuaNumericForStatement));

            // Verify AST
            var forStatement = luaAST[0] as LuaNumericForStatement;
            Assert.IsInstanceOfType(forStatement.Var, typeof(LuaAssignExpr));
            Assert.IsInstanceOfType(forStatement.Step, typeof(LuaExpr));
            Assert.IsInstanceOfType(forStatement.Limit, typeof(LuaExpr));
            Assert.IsInstanceOfType(forStatement.Body.ScopeBody[0], typeof(LuaExpr));

        }

        [TestMethod]
        public void LuaFunctionalTest08() {

            string sourceText = @"
            for k = 1, 8 do -- Numeric for statement
                print(k)
            end";

            // Parse and verify top-level
            var luaAST = LuaParser.ParseLuaSource(sourceText);
            Assert.IsInstanceOfType(luaAST[0], typeof(LuaNumericForStatement));

            // Verify AST
            var forStatement = luaAST[0] as LuaNumericForStatement;
            Assert.IsInstanceOfType(forStatement.Var, typeof(LuaAssignExpr));
            Assert.IsInstanceOfType(forStatement.Step, typeof(LuaNopExpr));
            Assert.IsInstanceOfType(forStatement.Limit, typeof(LuaExpr));
            Assert.IsInstanceOfType(forStatement.Body.ScopeBody[0], typeof(LuaExpr));

        }

        [TestMethod]
        public void LuaFunctionalTest09() {

            string sourceText = @"
            local t = { 1, 2, 4, 8, 16, 32, 64, 128, 256, 512, 1024, 2048, 4096 }
            for k, v in pairs(t) do -- Generic for statement
                print(""2^"" .. k  .. "" = "" .. v)
            end";

            // Parse and verify top-level
            var luaAST = LuaParser.ParseLuaSource(sourceText);
            Assert.IsInstanceOfType(luaAST[0], typeof(LuaAssignExpr));
            Assert.IsInstanceOfType(luaAST[1], typeof(LuaGenericForStatement));

            // Verify AST
            var forStatement = luaAST[1] as LuaGenericForStatement;
            Assert.AreEqual(forStatement.VarList.Variables.Count, 2);
            Assert.IsInstanceOfType(forStatement.Iterator, typeof(LuaCallExpr));
            Assert.AreEqual(1, forStatement.Body.ScopeBody.Count);

        }

        [TestMethod]
        public void LuaFunctionalTest10() {

            string sourceText = @"
            if false then
                print(""Hello World"")
            elseif (25 + 25 > 75) then
                print(""Crazy World"")
            else
                print(""This is branched Lua"")
            end";

            // Parse and verify top-level
            var luaAST = LuaParser.ParseLuaSource(sourceText);
            Assert.IsInstanceOfType(luaAST[0], typeof(LuaIfStatement));

            // Verify AST
            var ifStatement = luaAST[0] as LuaIfStatement;
            Assert.IsInstanceOfType(ifStatement.BranchFollow, typeof(LuaIfElseStatement));
            var ifElseStatement = ifStatement.BranchFollow as LuaIfElseStatement;
            Assert.IsInstanceOfType(ifElseStatement.BranchFollow, typeof(LuaElseStatement));

        }

        [TestMethod]
        public void LuaFunctionalTest11() {

            string sourceText = @"
            do
                print(""Hello"")
            end";

            // Parse and verify top-level
            var luaAST = LuaParser.ParseLuaSource(sourceText);
            Assert.IsInstanceOfType(luaAST[0], typeof(LuaDoStatement));
            Assert.IsInstanceOfType((luaAST[0] as LuaDoStatement).Body, typeof(LuaChunk));
            Assert.AreEqual((luaAST[0] as LuaDoStatement).Body.ScopeBody.Count, 1);

        }

        [TestMethod]
        public void LuaFunctionalTest12() {

            string sourceText = @"
            local i = 0
            repeat
                local k = i * 2
                print(k)
                i = i + 1
            until i + k >= 50
            print(i)";

            // Parse and verify top-level
            var luaAST = LuaParser.ParseLuaSource(sourceText);
            Assert.IsInstanceOfType(luaAST[0], typeof(LuaAssignExpr));
            Assert.IsInstanceOfType(luaAST[1], typeof(LuaRepeatStatement));
            Assert.IsInstanceOfType(luaAST[2], typeof(LuaCallExpr));

        }

        [TestMethod]
        public void LuaFunctionalTest13() {

            string sourceText =
              @"    for i=1, 10 do
                        if i <= 5 then
                            print(""Hello"")
                        else
                            break
                        end
                    end
                    print(""done"")";

            // Parse and verify top-level
            var luaAST = LuaParser.ParseLuaSource(sourceText);
            Assert.IsInstanceOfType(luaAST[0], typeof(LuaNumericForStatement));

            // Verify AST
            var forStatement = luaAST[0] as LuaNumericForStatement;
            Assert.IsInstanceOfType(forStatement.Var, typeof(LuaAssignExpr));
            Assert.IsInstanceOfType(forStatement.Step, typeof(LuaNopExpr));
            Assert.IsInstanceOfType(forStatement.Limit, typeof(LuaExpr));
            Assert.IsInstanceOfType(forStatement.Body.ScopeBody[0], typeof(LuaIfStatement));

        }

        [TestMethod]
        public void LuaFunctionalTest14() {

            string sourceText = "a.b()";

            // Parse and verify top-level
            var luaAST = LuaParser.ParseLuaSource(sourceText);
            Assert.IsInstanceOfType(luaAST[0], typeof(LuaCallExpr));
            Assert.IsInstanceOfType((luaAST[0] as LuaCallExpr).ToCall, typeof(LuaLookupExpr));

        }

        [TestMethod]
        public void LuaFunctionalTest15() {

            string sourceText = "a.b.c()";

            // Parse and verify top-level
            var luaAST = LuaParser.ParseLuaSource(sourceText);
            Assert.IsInstanceOfType(luaAST[0], typeof(LuaCallExpr));
            Assert.IsInstanceOfType((luaAST[0] as LuaCallExpr).ToCall, typeof(LuaLookupExpr));

        }

        [TestMethod]
        public void LuaFunctionalTest16() {

            string sourceText = "a:b()";

            // Parse and verify top-level
            var luaAST = LuaParser.ParseLuaSource(sourceText);
            Assert.IsInstanceOfType(luaAST[0], typeof(LuaSelfCallExpr));
            Assert.IsInstanceOfType((luaAST[0] as LuaSelfCallExpr).ToCall, typeof(LuaLookupExpr));

        }

        [TestMethod]
        public void LuaFunctionalTest17() {

            string sourceText = "a.b:c();";

            // Parse and verify top-level
            var luaAST = LuaParser.ParseLuaSource(sourceText);
            Assert.AreEqual(1, luaAST.Count);
            Assert.IsInstanceOfType(luaAST[0], typeof(LuaSelfCallExpr));
            Assert.IsInstanceOfType((luaAST[0] as LuaSelfCallExpr).ToCall, typeof(LuaLookupExpr));

        }

        [TestMethod]
        public void LuaFunctionalTest18() {

            string sourceText = @"
            test = { beta = 5 };
            function test:alpha(arg)
                print(arg / (self.beta * 2));
            end
            test:alpha(420);
            ";

            // Parse and verify top-level
            var luaAST = LuaParser.ParseLuaSource(sourceText);
            Assert.AreEqual(3, luaAST.Count);
            Assert.IsInstanceOfType(luaAST[0], typeof(LuaAssignExpr));
            Assert.IsInstanceOfType(luaAST[1], typeof(LuaAssignExpr));
            Assert.IsInstanceOfType(luaAST[2], typeof(LuaSelfCallExpr));

            // Verify test.alpha definition
            var decl = luaAST[1] as LuaAssignExpr;
            Assert.IsInstanceOfType(decl.Left, typeof(LuaLookupExpr));
            Assert.IsInstanceOfType(decl.Right, typeof(LuaFuncExpr));

        }

        [TestMethod]
        public void LuaFunctionalTest19() {

            // Parse and verify top-level
            var luaAST = LuaParser.ParseLuaSource("x, y, z = y, z, x;");
            Assert.AreEqual(1, luaAST.Count);
            Assert.IsInstanceOfType(luaAST[0], typeof(LuaAssignExpr));

            // Verify test.alpha definition
            var decl = luaAST[0] as LuaAssignExpr;
            Assert.IsInstanceOfType(decl.Left, typeof(LuaExpressionList));
            Assert.IsInstanceOfType(decl.Right, typeof(LuaExpressionList));

            // Assert orders
            LuaTestUtility.VerifyTupleOrder(decl.Left as LuaExpressionList, "x", "y", "z");
            LuaTestUtility.VerifyTupleOrder(decl.Right as LuaExpressionList, "y", "z", "x");

        }

        [TestMethod]
        public void LuaFunctionalTest20() {

            // Parse and verify top-level
            var luaAST = LuaParser.ParseLuaSource("x, y, z, w = y, z, w, x;");
            Assert.AreEqual(1, luaAST.Count);
            Assert.IsInstanceOfType(luaAST[0], typeof(LuaAssignExpr));

            // Verify test.alpha definition
            var decl = luaAST[0] as LuaAssignExpr;
            Assert.IsInstanceOfType(decl.Left, typeof(LuaExpressionList));
            Assert.IsInstanceOfType(decl.Right, typeof(LuaExpressionList));

            // Assert orders
            LuaTestUtility.VerifyTupleOrder(decl.Left as LuaExpressionList, "x", "y", "z", "w");
            LuaTestUtility.VerifyTupleOrder(decl.Right as LuaExpressionList, "y", "z", "w", "x");

        }

        [TestMethod]
        public void LuaFunctionalTest21() {

            // Src
            string src = @"
            function a()
                return true;
            end

            function b()
                return false;
            end

            function c()
                if a() then
                    if b() then 
                        return false;
                    else
                        return true;
                    end
                else
                    return false;
                end
            end

            ";

            // Parse and verify top-level
            var luaAST = LuaParser.ParseLuaSource(src);
            Assert.AreEqual(3, luaAST.Count);
            Assert.IsInstanceOfType(luaAST[0], typeof(LuaAssignExpr));
            Assert.IsInstanceOfType(luaAST[1], typeof(LuaAssignExpr));
            Assert.IsInstanceOfType(luaAST[2], typeof(LuaAssignExpr));

        }

        [TestMethod]
        public void LuaFunctionalTest22() {

            // Src
            string src = @"
            if true then
                if false then 
                    return false;
                else
                    return true;
                end
            else
                return false;
            end
            ";

            // Parse and verify top-level
            var luaAST = LuaParser.ParseLuaSource(src);
            Assert.AreEqual(1, luaAST.Count);
            Assert.IsInstanceOfType(luaAST[0], typeof(LuaIfStatement));
            Assert.IsInstanceOfType((luaAST[0] as LuaIfStatement).BranchFollow, typeof(LuaElseStatement));

            // Assert body is same pattern
            Assert.AreEqual(1, (luaAST[0] as LuaIfStatement).Body.ScopeBody.Count);
            var body = (luaAST[0] as LuaIfStatement).Body.ScopeBody[0];
            Assert.IsInstanceOfType(body, typeof(LuaIfStatement));
            Assert.IsInstanceOfType((body as LuaIfStatement).BranchFollow, typeof(LuaElseStatement));

        }

        [TestMethod]
        public void LuaFunctionalTest23() {

            // Src
            string src = @"
            if true then
                if false then 
                    return false;
                else
                    return true;
                end
                print(5);
            else
                return false;
            end
            ";

            // Parse and verify top-level
            var luaAST = LuaParser.ParseLuaSource(src);
            Assert.AreEqual(1, luaAST.Count);
            Assert.IsInstanceOfType(luaAST[0], typeof(LuaIfStatement));
            Assert.IsInstanceOfType((luaAST[0] as LuaIfStatement).BranchFollow, typeof(LuaElseStatement));

            // Assert body is same pattern
            Assert.AreEqual(2, (luaAST[0] as LuaIfStatement).Body.ScopeBody.Count);
            var body = (luaAST[0] as LuaIfStatement).Body.ScopeBody[0];
            Assert.IsInstanceOfType(body, typeof(LuaIfStatement));
            Assert.IsInstanceOfType((body as LuaIfStatement).BranchFollow, typeof(LuaElseStatement));

        }

    }

}
