using System;
using Battlegrounds.Lua;
using Battlegrounds.Lua.Runtime;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using static Battlegrounds.Lua.LuaNil;

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
            stack.Push(Nil);
            Assert.AreEqual(1, stack.Top);
            Assert.AreEqual(true, stack.Any);
        }

        [TestMethod]
        public void CanPopElement() {
            stack.Push(Nil);
            LuaValue var = stack.Pop();
            Assert.AreEqual(Nil, var);
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
            stack.Push(Nil);
            stack.Push(new LuaBool(true));
            stack.Push(new LuaBool(false));
            LuaValue[] values = stack.Pop(3);
            Assert.AreEqual(new LuaBool(false), values[0]);
            Assert.AreEqual(new LuaBool(true), values[1]);
            Assert.AreEqual(Nil, values[2]);
            Assert.AreEqual(0, stack.Top);
            Assert.AreEqual(false, stack.Any);
        }

        [TestMethod]
        public void CanPopMultipleElementsWithNil() {
            stack.Push(Nil);
            stack.Push(new LuaBool(true));
            stack.Push(new LuaBool(false));
            LuaValue[] values = stack.Pop(5);
            Assert.AreEqual(new LuaBool(false), values[0]);
            Assert.AreEqual(new LuaBool(true), values[1]);
            Assert.AreEqual(Nil, values[2]);
            Assert.AreEqual(Nil, values[3]);
            Assert.AreEqual(Nil, values[4]);
            Assert.AreEqual(0, stack.Top);
            Assert.AreEqual(false, stack.Any);
        }
        [TestMethod]
        public void WillIncreaseCapacity() {
            stack.Push(Nil);
            stack.Push(Nil);
            stack.Push(Nil);
            stack.Push(Nil);
            stack.Push(Nil);
            stack.Push(Nil);
            stack.Push(Nil);
            Assert.AreEqual(7, stack.Top);
            Assert.AreEqual(8, stack.Capacity);
            stack.Push(Nil);
            Assert.AreEqual(8, stack.Top);
            Assert.AreEqual(16, stack.Capacity);
            stack.Push(Nil);
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

        [TestMethod]
        public void CanSetEnd() {
            stack.Push(new LuaNumber(3));
            stack.Push(new LuaNumber(2));
            stack.Push(new LuaNumber(1));
            stack.Lock(stack.Top);
            Assert.AreEqual(false, stack.Any);
            stack.Push(new LuaNumber(0));
            Assert.AreEqual(true, stack.Any);
            Assert.AreEqual(new LuaNumber(0), stack.Pop());
            Assert.AreEqual(Nil, stack.PopOrNil());
            stack.Unlock();
            Assert.AreEqual(new LuaNumber(1), stack.Pop());
        }

    }

}
