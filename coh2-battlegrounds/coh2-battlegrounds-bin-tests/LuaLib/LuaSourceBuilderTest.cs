using System;
using System.Collections.Generic;

using Battlegrounds.Lua;
using Battlegrounds.Lua.Generator;
using Battlegrounds.Lua.Generator.RuntimeServices;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace coh2_battlegrounds_bin_tests.LuaLib {

    [TestClass]
    public class LuaSourceBuilderTest {

        public class DummyObject {
            public string Name { get; init; }
            public int Number { get; init; }
            public DummyObject[] Children { get; init; }
            public Dictionary<string, float> Floats { get; init; }
        }

        public class AdditionalDummyObjectConverter : LuaConverter<AdditionalDummyObject> {
            public override void Write(LuaSourceBuilder luaSourceBuilder, AdditionalDummyObject value) {
                luaSourceBuilder.Writer.WriteKeyword(LuaKw.Function);
                luaSourceBuilder.Writer.WriteOperator('(');
                luaSourceBuilder.Writer.WriteVerbatim("ships");
                luaSourceBuilder.Writer.WriteOperator(')');
                luaSourceBuilder.Writer.EndLine(true);
                luaSourceBuilder.Writer.WriteKeyword(LuaKw.Return);
                luaSourceBuilder.Writer.WriteVerbatim($"ships[\"{value.This}-{value.That}\"]");
                luaSourceBuilder.Writer.DecreaseIndent();
                luaSourceBuilder.Writer.EndLine(false);
                luaSourceBuilder.Writer.WriteKeyword(LuaKw.End);
            }
        }

        [LuaConverter(typeof(AdditionalDummyObjectConverter))]
        public class AdditionalDummyObject {
            public string This { get; init; }
            public int That { get; init; }
        }

        private LuaState luaState;
        private LuaSourceBuilder sourceBuilder;
        private DummyObject dummyObject;

        [TestInitialize]
        public void Setup() {
            this.sourceBuilder = new();
            this.dummyObject = new() {
                Name = "Hello",
                Number = 42,
                Floats = new() {
                    ["a"] = 'A',
                    ["f"] = 'F',
                    ["247"] = 247.78f
                },
                /*Children = new DummyObject[] {
                    new DummyObject() { Name = "Child A", Number = 43 },
                    new DummyObject() { Name = "Child B", Number = 41 }
                },*/
            };
            this.luaState = new();
        }

        [TestMethod]
        public void CanWriteAssignment() {
            string source = this.sourceBuilder.Assignment("a", 5).GetSourceText();
            Assert.IsNotNull(source);
            Assert.AreEqual("a = 5;\r\n", source);
        }

        [TestMethod]
        public void CanWriteAssignmentWithoutSemicolon() {
            this.sourceBuilder.Options.WriteSemicolon = false;
            string source = this.sourceBuilder.Assignment("a", 5).GetSourceText();
            Assert.IsNotNull(source);
            Assert.AreEqual("a = 5\r\n", source);
        }

        [TestMethod]
        public void CanWriteStringAssignment() {
            string source = this.sourceBuilder.Assignment("a", "Hello").GetSourceText();
            Assert.IsNotNull(source);
            Assert.AreEqual("a = \"Hello\";\r\n", source);
        }

        [TestMethod]
        public void CanBuildTable() {

            string[] expected = {
                "g_table = {",
                "\tName = \"Hello\",",
                "\tNumber = 42,",
                "\tFloats = { [\"a\"] = 65, [\"f\"] = 70, [\"247\"] = 247.78 }",
                "};",
                ""
            };

            string source = this.sourceBuilder.Assignment("g_table", this.dummyObject).GetSourceText();
            Assert.IsNotNull(source);

            string[] sourceLines = source.Split(Environment.NewLine);
            Assert.AreEqual(expected.Length, sourceLines.Length);

            for (int i = 0; i < expected.Length; i++) {
                Assert.AreEqual(expected[i].Length, sourceLines[i].Length);
                Assert.AreEqual(expected[i], sourceLines[i]);
            }

        }

        [TestMethod]
        public void CanWriteFunction() {

            string[] expected = {
                "-- @Auto-Generated",
                "function test(a, b, c)",
                "\treturn a, b, c;",
                "end",
                ""
            };

            string source = this.sourceBuilder.Function("test", "a", "b", "c").Return().Variables("a", "b", "c").End().GetSourceText();
            Assert.IsNotNull(source);

            string[] sourceLines = source.Split(Environment.NewLine);
            Assert.AreEqual(expected.Length, sourceLines.Length);

            for (int i = 0; i < expected.Length; i++) {
                Assert.AreEqual(expected[i].Length, sourceLines[i].Length);
                Assert.AreEqual(expected[i], sourceLines[i]);
            }

        }

        [TestMethod]
        public void CanWriteFunctions() {

            string[] expected = {
                "-- @Auto-Generated",
                "function add(a, b)",
                "\treturn a + b;",
                "end",
                "",
                "-- @Auto-Generated",
                "function sub(a, b, c)",
                "\treturn add(a, b) - c;",
                "end",
                ""
            };

            string source = this.sourceBuilder.Function("add", "a", "b").Return().Arithmetic("a", '+', "b").End()
                .Function("sub", "a", "b", "c").Return().Call("add", "a", "b").Arithmetic('-', "c").End().GetSourceText();
            Assert.IsNotNull(source);

            string[] sourceLines = source.Split(Environment.NewLine);
            Assert.AreEqual(expected.Length, sourceLines.Length);

            for (int i = 0; i < expected.Length; i++) {
                Assert.AreEqual(expected[i].Length, sourceLines[i].Length);
                Assert.AreEqual(expected[i], sourceLines[i]);
            }

        }

        [TestMethod]
        public void CanWriteFunctionWithNumber() {

            string[] expected = {
                "-- @Auto-Generated",
                "function add(a, b)",
                "\treturn a + b;",
                "end",
                "",
                "-- @Auto-Generated",
                "function sub(a, b)",
                "\treturn add(a, b) / 1.5;",
                "end",
                ""
            };

            string source = this.sourceBuilder.Function("add", "a", "b").Return().Arithmetic("a", '+', "b").End()
                .Function("sub", "a", "b").Return().Call("add", "a", "b").Arithmetic('/', 1.5).End().GetSourceText();
            Assert.IsNotNull(source);

            string[] sourceLines = source.Split(Environment.NewLine);
            Assert.AreEqual(expected.Length, sourceLines.Length);

            for (int i = 0; i < expected.Length; i++) {
                Assert.AreEqual(expected[i].Length, sourceLines[i].Length);
                Assert.AreEqual(expected[i], sourceLines[i]);
            }

        }

        [TestMethod]
        public void CanWriteObjectUsingConverter() {

            AdditionalDummyObject obj = new() { That = 1701, This = "NCC" };

            string[] expected = {
                "Starship = function (ships)",
                "\treturn ships[\"NCC-1701\"];",
                "end",
                "",
                "",
            };

            string source = this.sourceBuilder.Assignment("Starship", obj).GetSourceText();
            Assert.IsNotNull(source);

            string[] sourceLines = source.Split(Environment.NewLine);
            Assert.AreEqual(expected.Length, sourceLines.Length);

            for (int i = 0; i < expected.Length; i++) {
                Assert.AreEqual(expected[i].Length, sourceLines[i].Length);
                Assert.AreEqual(expected[i], sourceLines[i]);
            }

        }

        [TestMethod]
        public void CanWriteDictionary() {

            // Dummy data
            var data = new Dictionary<string, object>() {
                ["Yellow"] = 555,
                ["Green"] = 888,
                ["Orange"] = 124
            };

            // Build
            this.sourceBuilder.Writer.WriteTableValue(this.sourceBuilder.BuildTableRaw(data));
            string source = this.sourceBuilder.GetSourceText();

            // Assert are equal
            Assert.AreEqual("{ Yellow = 555, Green = 888, Orange = 124 }", source);

        }

        [TestMethod]
        public void CanWriteDictionaryWithType() {

            // Dummy data
            var data = new Dictionary<string, int>() { // <-- It's literally just the type that was changed here
                ["Yellow"] = 555,
                ["Green"] = 888,
                ["Orange"] = 124
            };

            // Build
            this.sourceBuilder.Writer.WriteTableValue(this.sourceBuilder.BuildTableRaw(data));
            string source = this.sourceBuilder.GetSourceText();

            // Assert are equal
            Assert.AreEqual("{ Yellow = 555, Green = 888, Orange = 124 }", source);

        }

    }

}
