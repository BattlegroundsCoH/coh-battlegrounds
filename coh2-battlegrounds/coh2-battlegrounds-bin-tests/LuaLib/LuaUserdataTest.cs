using System.IO;
using System.Text;

using Battlegrounds.Lua;
using static Battlegrounds.Lua.LuaNil;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace coh2_battlegrounds_bin_tests.LuaLib {

    [TestClass]
    public class LuaUserdataTest {

        LuaState lState;
        TextWriter writer;
        StringBuilder writerOutput;

        [TestInitialize]
        public void Setup() {
            writerOutput = new StringBuilder();
            lState = new LuaState("base") {
                Out = writer = new StringWriter(writerOutput),
                StackTrace = true,
            };
        }

        public class PlayerTestClass {

            [LuaUserobjectProperty]
            public string Name { get; set; }

            private string m_faction = "German";

            [LuaUserobjectMethod(UseMarshalling = true)]
            public void SetFaction(string faction) => this.m_faction = faction;

            [LuaUserobjectMethod(UseMarshalling = true)]
            public string GetFaction() => this.m_faction;

            [LuaUserobjectMethod(UseMarshalling = true, CreateMetatable = true)]
            public static PlayerTestClass New() => new PlayerTestClass() { Name = "CoDiEx" };

        }

        [TestMethod]
        public void VerifyUserdata() {
            lState.RegisterUserdata(typeof(PlayerTestClass));
            Assert.IsTrue(lState.IsUserObject(typeof(PlayerTestClass)));
            Assert.IsTrue(lState._G[nameof(PlayerTestClass)] is not LuaNil);
            Assert.IsTrue(lState._G[nameof(PlayerTestClass)] is LuaTable);
        }

        [TestMethod]
        public void CanCreateUserObject() {
            lState.RegisterUserdata(typeof(PlayerTestClass));

            LuaUserObject userObject = lState.DoString("return PlayerTestClass.New()") as LuaUserObject;
            Assert.IsNotNull(userObject);
            Assert.IsInstanceOfType(userObject.Object, typeof(PlayerTestClass));

        }

        [TestMethod]
        public void CanUseUserObject() {

            // Register type
            lState.RegisterUserdata(typeof(PlayerTestClass));

            // Lua code to run
            string code = @"
            player = PlayerTestClass.New()
            PlayerTestClass.SetFaction(player, ""Soviet"")
            print(PlayerTestClass.GetFaction(player))
            ";

            // Run string
            Assert.AreEqual(Nil, lState.DoString(code));

            // Make assertions on output
            string[] lns = writerOutput.ToString().Split(writer.NewLine);
            Assert.AreEqual(2, lns.Length);
            Assert.AreEqual("Soviet", lns[0]);

        }

        [TestMethod]
        public void CanUseUserObjectDirectly() {

            // Register type
            lState.RegisterUserdata(typeof(PlayerTestClass));

            // Lua code to run
            string code = @"
            player = PlayerTestClass.New()
            player:SetFaction(""Soviet"")
            print(player:GetFaction())
            ";

            // Run string
            Assert.AreEqual(Nil, lState.DoString(code));

            // Make assertions on output
            string[] lns = writerOutput.ToString().Split(writer.NewLine);
            Assert.AreEqual(2, lns.Length);
            Assert.AreEqual("Soviet", lns[0]);

        }

        [TestMethod]
        public void CanAccessUserObjectProperty() {

            // Register type
            lState.RegisterUserdata(typeof(PlayerTestClass));

            // Lua code to run
            string code = @"
            player = PlayerTestClass.New()
            print(player.Name)
            ";

            // Run string
            Assert.AreEqual(Nil, lState.DoString(code));

            // Make assertions on output
            string[] lns = writerOutput.ToString().Split(writer.NewLine);
            Assert.AreEqual(2, lns.Length);
            Assert.AreEqual("CoDiEx", lns[0]);

        }

    }

}
