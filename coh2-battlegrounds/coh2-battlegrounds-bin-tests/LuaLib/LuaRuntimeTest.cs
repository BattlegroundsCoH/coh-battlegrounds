using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Battlegrounds.Lua;
using Battlegrounds.Lua.Runtime;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using static Battlegrounds.Lua.LuaNil;

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
                StackTrace = true,
            };
        }

        [TestMethod]
        public void HasBaseLoaded() {
            Assert.AreNotEqual(Nil, lState._G["print"]);
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
            ds9()";

            var result = LuaVM.DoString(this.lState, sourceText);
            Assert.AreEqual(Nil, result);
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
            print(a)";

            var result = LuaVM.DoString(this.lState, sourceText);
            Assert.AreEqual(Nil, result);
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
            Assert.AreEqual(Nil, result);
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
            Assert.AreEqual(Nil, result);
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
            Assert.AreEqual(Nil, result);

            string[] lns = writerOutput.ToString().Split(writer.NewLine);
            Assert.AreEqual(4, lns.Length);
            Assert.AreEqual("114:2895", lns[0]);
            Assert.AreEqual("1566.5:167907", lns[1]);
            Assert.AreEqual("2", lns[2]);

            // Verify locals are done correctly
            Assert.AreEqual(Nil, this.lState._G["k"]);
            Assert.AreEqual(Nil, this.lState._G["dumdum"]);
            //Assert.AreEqual(0, this.lState.Envionment.Size); // Environment should now be closed --> We sould not be able to access k or dumdum

        }

        [TestMethod]
        public void SimpleFunctionTest06() {

            string sourceText = @"
            k = 0
            while k < 50 do
                print(k)
                k = k + 1
            end";

            // Run and check output
            var result = LuaVM.DoString(this.lState, sourceText);
            Assert.AreEqual(Nil, result);

            string[] lns = writerOutput.ToString().Split(writer.NewLine);
            Assert.AreEqual(51, lns.Length);

            for (int i = 0; i < 50; i++) {
                Assert.AreEqual(i.ToString(), lns[i]);
            }

        }

        [TestMethod]
        public void SimpleFunctionTest07() {

            string sourceText = @"
            for k = 1, 8, 2 do -- Numeric for statement
                print(k)
            end";

            // Run and check output
            var result = LuaVM.DoString(this.lState, sourceText);
            Assert.AreEqual(Nil, result);

            string[] lns = writerOutput.ToString().Split(writer.NewLine);
            Assert.AreEqual(5, lns.Length);

            for (int i = 0; i < 8; i += 2) {
                Assert.AreEqual((i + 1).ToString(), lns[i / 2]);
            }

        }

        [TestMethod]
        public void SimpleFunctionTest08() {

            string sourceText = @"
            for k = 1, 8 do -- Numeric for statement
                print(k)
            end";

            // Run and check output
            var result = LuaVM.DoString(this.lState, sourceText);
            Assert.AreEqual(Nil, result);

            string[] lns = writerOutput.ToString().Split(writer.NewLine);
            Assert.AreEqual(9, lns.Length);

            for (int i = 0; i < 8; i++) {
                Assert.AreEqual((i + 1).ToString(), lns[i]);
            }

        }

        [TestMethod]
        public void SimpleFunctionTest09() {

            string sourceText = @"
            local t = { 2, 4, 8, 16, 32, 64, 128, 256, 512, 1024, 2048, 4096 }
            for k, v in pairs(t) do -- Generic for statement
                print(""2^"" .. k  .. "" = "" .. v)
            end";

            // Run and check output
            var result = LuaVM.DoString(this.lState, sourceText);
            Assert.AreEqual(Nil, result);

            string[] lns = writerOutput.ToString().Split(writer.NewLine);
            Assert.AreEqual(13, lns.Length);

            for (int i = 0; i < 8; i++) {
                int v = (int)Math.Pow(2, i + 1);
                Assert.AreEqual($"2^{i + 1} = {v}", lns[i]);
            }

        }

        [TestMethod]
        public void SimpleFunctionTest10() {

            string sourceText = @"
            if false then
                print(""Hello World"")
            elseif (25 + 25 > 75) then
                print(""Crazy World"")
            else
                print(""This is branched Lua"")
            end";

            // Run and check output
            var result = LuaVM.DoString(this.lState, sourceText);
            Assert.AreEqual(Nil, result);

            string[] lns = writerOutput.ToString().Split(writer.NewLine);
            Assert.AreEqual(2, lns.Length);
            Assert.AreEqual($"This is branched Lua", lns[0]);

        }

        [TestMethod]
        public void SimpleFunctionTest11() {

            string sourceText = @"
            if false then
                print(""Hello World"")
            elseif (25 + 25 > 75) then
                print(""Crazy World"")
            else
                if true then
                    print(""This is branched Lua"")
                end
            end";

            // Run and check output
            var result = LuaVM.DoString(this.lState, sourceText);
            Assert.AreEqual(Nil, result);

            string[] lns = writerOutput.ToString().Split(writer.NewLine);
            Assert.AreEqual(2, lns.Length);
            Assert.AreEqual($"This is branched Lua", lns[0]);

        }

        [TestMethod]
        public void SimpleFunctionTest12() {

            string sourceText = @"
            if false then
                print(""Hello World"")
            elseif (25 + 25 > 75) then
                print(""Crazy World"")
            else
                if false then
                    print(""This is branched Lua"")
                end
            end";

            // Run and check output
            var result = LuaVM.DoString(this.lState, sourceText);
            Assert.AreEqual(Nil, result);

            string[] lns = writerOutput.ToString().Split(writer.NewLine);
            Assert.AreEqual(1, lns.Length);

        }

        [TestMethod]
        public void SimpleFunctionTest13() {

            string sourceText = @"
            for i=1, 10 do
                if i <= 5 then
                    print(""Hello"")
                else
                    break
                end
            end
            print(""done"")";

            // Run and check output
            var result = LuaVM.DoString(this.lState, sourceText);
            Assert.AreEqual(Nil, result);

            string[] lns = writerOutput.ToString().Split(writer.NewLine);
            Assert.AreEqual(7, lns.Length);
            Assert.AreEqual("Hello", lns[0]);
            Assert.AreEqual("Hello", lns[1]);
            Assert.AreEqual("Hello", lns[2]);
            Assert.AreEqual("Hello", lns[3]);
            Assert.AreEqual("Hello", lns[4]);
            Assert.AreEqual("done", lns[5]);

        }

        [TestMethod]
        public void SimpleFunctionTest14() {

            string sourceText = @"
            for i=1, 10 do
                if i <= 5 then
                    print(""Hello"")
                else
                    return 11
                end
            end
            print(""done"")";

            // Run and check output
            var result = LuaVM.DoString(this.lState, sourceText);
            Assert.AreEqual(new LuaNumber(11), result);

            string[] lns = writerOutput.ToString().Split(writer.NewLine);
            Assert.AreEqual(6, lns.Length);
            Assert.AreEqual("Hello", lns[0]);
            Assert.AreEqual("Hello", lns[1]);
            Assert.AreEqual("Hello", lns[2]);
            Assert.AreEqual("Hello", lns[3]);
            Assert.AreEqual("Hello", lns[4]);

        }

        [TestMethod]
        [Timeout(500)]
        public void SimpleFunctionTest15() {

            string sourceText = @"
            local k = 0
            while k < 50 do
                print(k)
                k = k + 1
            end";

            // Run and check output
            var result = LuaVM.DoString(this.lState, sourceText);
            Assert.AreEqual(Nil, result);

            string[] lns = writerOutput.ToString().Split(writer.NewLine);
            Assert.AreEqual(51, lns.Length);

            for (int i = 0; i < 50; i++) {
                Assert.AreEqual(i.ToString(), lns[i]);
            }

        }

        [TestMethod]
        public void SimpleFunctionTest16() {

            // Define source (From lua manual on visibility/scope rules)
            string sourceText = @"
            x = 10
            do
                local x = x
                print(x)
                x = x+1
                do
                    local x = x+1
                    print(x)
                end
                print(x)
            end
            print(x)";

            // Run and check output
            var result = LuaVM.DoString(this.lState, sourceText);
            Assert.AreEqual(Nil, result);

            // Make assertions on output
            string[] lns = writerOutput.ToString().Split(writer.NewLine);
            Assert.AreEqual(5, lns.Length);
            Assert.AreEqual("10", lns[0]);
            Assert.AreEqual("12", lns[1]);
            Assert.AreEqual("11", lns[2]);
            Assert.AreEqual("10", lns[3]);

        }

        [TestMethod]
        public void SimpleFunctionTest17() {

            string sourceText = @"
            local i = 0
            repeat
                local k = i * 2
                print(k)
                i = i + 1
            until i + k >= 50
            print(i)";

            // Run and check output
            var result = LuaVM.DoString(this.lState, sourceText);
            Assert.AreEqual(Nil, result);

            // Make assertions on output
            string[] lns = writerOutput.ToString().Split(writer.NewLine);
            Assert.AreEqual(20, lns.Length);
            for (int i = 0; i < 18; i++) {
                Assert.AreEqual((i * 2).ToString(), lns[i]);
            }
            Assert.AreEqual("18", lns[18]);

        }

        [TestMethod]
        public void SimpleFunctionTest18() {

            string sourceText = @"
            test = { beta = 5 };
            function test:alpha(arg)
                print(arg / (self.beta * 2));
            end
            test:alpha(420);
            ";

            // Run and check output
            var result = LuaVM.DoString(this.lState, sourceText);
            Assert.AreEqual(Nil, result);

            // Make assertions on output
            string[] lns = writerOutput.ToString().Split(writer.NewLine);
            Assert.AreEqual(2, lns.Length);
            Assert.AreEqual("42", lns[0]);

        }

        [TestMethod]
        public void SimpleFunctionTest19() {

            string sourceText = @"
            function dumdum()
                return 1, 2, 3;
            end
            print((dumdum()))
            ";

            // Run and check output
            var result = LuaVM.DoString(this.lState, sourceText);
            Assert.AreEqual(Nil, result);

            // Make assertions on output
            string[] lns = writerOutput.ToString().Split(writer.NewLine);
            Assert.AreEqual(2, lns.Length);
            Assert.AreEqual("1", lns[0]);

        }

        [TestMethod]
        public void SimpleFunctionTest20() {

            string sourceText = @"
            x = 1;
            y = -5;
            z = 11;
            x,y,z = y,z,x;
            print(x);
            print(y);
            print(z);
            ";

            // Run and check output
            var result = LuaVM.DoString(this.lState, sourceText);
            Assert.AreEqual(Nil, result);

            // Make assertions on output
            string[] lns = writerOutput.ToString().Split(writer.NewLine);
            Assert.AreEqual(4, lns.Length);
            Assert.AreEqual("-5", lns[0]);
            Assert.AreEqual("11", lns[1]);
            Assert.AreEqual("1", lns[2]);

        }

        [TestMethod]
        public void SimpleFunctionTest21() {

            string sourceText = @"
            b = { alpha = 5 };
            function b:get(a) 
                self.alpha = self.alpha + a;
                return self.alpha;
            end
            a = {
                b:get(1),
                b:get(2),
                b:get(3)
            }
            print(a[1]);
            print(a[2]);
            print(a[3]);
            ";

            // Run and check output
            var result = LuaVM.DoString(this.lState, sourceText);
            Assert.AreEqual(Nil, result);

            // Make assertions on output
            string[] lns = writerOutput.ToString().Split(writer.NewLine);
            Assert.AreEqual(4, lns.Length);
            Assert.AreEqual("6", lns[0]);
            Assert.AreEqual("8", lns[1]);
            Assert.AreEqual("11", lns[2]);

        }

        [TestMethod]
        public void SimpleFunctionTest22() {

            string sourceText = @"
            function getTrue()
                return true;
            end

            function checkTrue()
                if getTrue() then
                    print(true);
                else
                    print(false);
                end
            end
            checkTrue();
            ";

            // Run and check output
            var result = LuaVM.DoString(this.lState, sourceText);
            Assert.AreEqual(Nil, result);

            // Make assertions on output
            string[] lns = writerOutput.ToString().Split(writer.NewLine);
            Assert.AreEqual(2, lns.Length);
            Assert.AreEqual("true", lns[0]);

        }

    }

}
