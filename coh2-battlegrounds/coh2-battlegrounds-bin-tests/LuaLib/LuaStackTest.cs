using System;
using Battlegrounds.Lua;
using Battlegrounds.Lua.Runtime;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace coh2_battlegrounds_bin_tests.LuaLib {
    
    [TestClass]
    public class LuaStackTest {

        LuaStack stack;

        [TestInitialize]
        public void Setup() => stack = new LuaStack();

        [TestMethod]
        public void InitialCapacityIs8() => Assert.AreEqual(8, stack.Capacity);

        [TestMethod]
        public void CanPushElement() {
            stack.Push(new LuaNil());
            Assert.AreEqual(1, stack.Top);
            Assert.AreEqual(true, stack.Any);
        }

        [TestMethod]
        public void CanPopElement() {
            stack.Push(new LuaNil());
            LuaValue var = stack.Pop();
            Assert.AreEqual(new LuaNil(), var);
            Assert.AreEqual(0, stack.Top);
            Assert.AreEqual(false, stack.Any);
        }

        [TestMethod]
        public void CanPeekElement() {
            stack.Push(new LuaBool(true));
            Assert.AreEqual(new LuaBool(true), stack.Peek());
            Assert.AreEqual(1, stack.Top);
            Assert.AreEqual(true, stack.Any);
        }

        [TestMethod]
        public void CannotPopBeyondStack() => Assert.ThrowsException<InvalidOperationException>(() => stack.Pop());

        [TestMethod]
        public void CanPopMultipleElements() {
            stack.Push(new LuaNil());
            stack.Push(new LuaBool(true));
            stack.Push(new LuaBool(false));
            LuaValue[] values = stack.Pop(3);
            Assert.AreEqual(new LuaBool(false), values[0]);
            Assert.AreEqual(new LuaBool(true), values[1]);
            Assert.AreEqual(new LuaNil(), values[2]);
            Assert.AreEqual(0, stack.Top);
            Assert.AreEqual(false, stack.Any);
        }

        [TestMethod]
        public void CanPopMultipleElementsWithNil() {
            stack.Push(new LuaNil());
            stack.Push(new LuaBool(true));
            stack.Push(new LuaBool(false));
            LuaValue[] values = stack.Pop(5);
            Assert.AreEqual(new LuaBool(false), values[0]);
            Assert.AreEqual(new LuaBool(true), values[1]);
            Assert.AreEqual(new LuaNil(), values[2]);
            Assert.AreEqual(new LuaNil(), values[3]);
            Assert.AreEqual(new LuaNil(), values[4]);
            Assert.AreEqual(0, stack.Top);
            Assert.AreEqual(false, stack.Any);
        }
        [TestMethod]
        public void WillIncreaseCapacity() {
            stack.Push(new LuaNil());
            stack.Push(new LuaNil());
            stack.Push(new LuaNil());
            stack.Push(new LuaNil());
            stack.Push(new LuaNil());
            stack.Push(new LuaNil());
            stack.Push(new LuaNil());
            Assert.AreEqual(7, stack.Top);
            Assert.AreEqual(8, stack.Capacity);
            stack.Push(new LuaNil());
            Assert.AreEqual(8, stack.Top);
            Assert.AreEqual(16, stack.Capacity);
            stack.Push(new LuaNil());
            Assert.AreEqual(9, stack.Top);
            Assert.AreEqual(16, stack.Capacity);
        }

        [TestMethod]
        public void CanShiftLeft() {
            stack.Push(new LuaNumber(3));
            stack.Push(new LuaNumber(2));
            stack.Push(new LuaNumber(1));
            Assert.AreEqual(3, stack.Top);
            int top = stack.ShiftLeft(2);
            Assert.AreEqual(1, top);
            Assert.AreEqual(new LuaNumber(1), stack.Pop());
        }

    }

}
