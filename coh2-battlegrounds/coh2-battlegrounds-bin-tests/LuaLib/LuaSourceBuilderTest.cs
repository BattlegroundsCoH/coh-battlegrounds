using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Battlegrounds.Lua;
using Battlegrounds.Lua.Generator;

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

        LuaState luaState;
        LuaSourceBuilder sourceBuilder;
        DummyObject dummyObject;

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
            string source = this.sourceBuilder.WriteAssignment("a", 5).GetSourceTest();
            Assert.IsNotNull(source);
            Assert.AreEqual("a = 5;\r\n", source);
        }

        [TestMethod]
        public void CanWriteAssignmentWithoutSemicolon() {
            this.sourceBuilder.Options.WriteSemicolon = false;
            string source = this.sourceBuilder.WriteAssignment("a", 5).GetSourceTest();
            Assert.IsNotNull(source);
            Assert.AreEqual("a = 5\r\n", source);
        }

        [TestMethod]
        public void CanWriteStringAssignment() {
            string source = this.sourceBuilder.WriteAssignment("a", "Hello").GetSourceTest();
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

            string source = this.sourceBuilder.WriteAssignment("g_table", this.dummyObject).GetSourceTest();
            Assert.IsNotNull(source);

            string[] sourceLines = source.Split(Environment.NewLine);
            Assert.AreEqual(expected.Length, sourceLines.Length);

            for (int i = 0; i < expected.Length; i++) {
                Assert.AreEqual(expected[i].Length, sourceLines[i].Length);
                Assert.AreEqual(expected[i], sourceLines[i]);
            }

        }

    }

}
