using System;
using System.Linq;

using static Battlegrounds.Lua.LuaNil;

namespace Battlegrounds.Lua.Runtime {

    /// <summary>
    /// Represents the stack of <see cref="LuaValue"/> objects. Can dynamically increase in size and offers various push/pop functionality.
    /// </summary>
    public class LuaStack {

        private LuaValue[] m_stack;
        private int m_topPtr;
        private int m_bottomPtr;

        /// <summary>
        /// Get the pointer to the top stack value.
        /// </summary>
        public int Top => this.m_topPtr;

        /// <summary>
        /// Get the capacity of the stack.
        /// </summary>
        public int Capacity => this.m_stack.Length;

        /// <summary>
        /// Get if any value is currently on the stack.
        /// </summary>
        public bool Any => this.m_topPtr > this.m_bottomPtr;

        /// <summary>
        /// Initialize a new and empty <see cref="LuaStack"/> class with default capacity of 8 elements.
        /// </summary>
        public LuaStack() {
            this.m_stack = new LuaValue[8];
            this.m_topPtr = 0;
            this.m_bottomPtr = 0;
        }

        /// <summary>
        /// Initialize a new and empty <see cref="LuaStack"/> class with a capacity of <paramref name="stackSize"/>.
        /// </summary>
        /// <param name="stackSize">The amount of elements that can be stored on the stack.</param>
        public LuaStack(int stackSize) {
            this.m_stack = new LuaValue[stackSize];
            this.m_topPtr = 0;
        }

        /// <summary>
        /// Push a new element onto the top of the stack.
        /// </summary>
        /// <param name="value">The <see cref="LuaValue"/> to place on top of stack.</param>
        public void Push(LuaValue value) {
            this.m_stack[this.m_topPtr++] = value;
            if (this.m_topPtr == this.Capacity) {
                this.IncreaseCapacity();
            }
        }

        private void IncreaseCapacity() {
            var buffer = new LuaValue[this.m_stack.Length + 8]; // Note: The stack should never get too big, se there's no point in doubling the size every ime.
            Array.Copy(this.m_stack, buffer, this.m_stack.Length);
            this.m_stack = buffer;
        }

        /// <summary>
        /// Pop the stack by <paramref name="count"/> times.
        /// </summary>
        /// <param name="count">The amount of pop-actions to perform.</param>
        /// <returns>Array of length <paramref name="count"/> and filled with top-most elements with first element being the first element popped.</returns>
        public LuaValue[] Pop(int count) {
            int i = 0;
            LuaValue[] vals = new LuaValue[count];
            while (this.m_topPtr > 0) {
                vals[i] = this.m_stack[--this.m_topPtr];
                i++;
            }
            if (i != count) {
                Array.Fill(vals, Nil, i, count - i);
            }
            return vals;
        }

        /// <summary>
        /// Pop the top-most element of the stack.
        /// </summary>
        /// <returns>The top-most element.</returns>
        /// <exception cref="InvalidOperationException"/>
        public LuaValue Pop() {
            if (this.m_topPtr - 1 >= this.m_bottomPtr) {
                return this.m_stack[--this.m_topPtr]; // We dont set to null, we just change our stack ptr
            } else {
                throw new InvalidOperationException("Cannot pop an empty stack.");
            }
        }

        /// <summary>
        /// Get the top-most element of the stack without removing it.
        /// </summary>
        /// <returns>The top-most element of the stack.</returns>
        /// <exception cref="InvalidOperationException"/>
        public LuaValue Peek() {
            if (this.m_topPtr - 1 >= this.m_bottomPtr) {
                return this.m_stack[this.m_topPtr - 1];
            } else {
                throw new InvalidOperationException("Cannot peek an empty stack.");
            }
        }

        /// <summary>
        /// Pops the top element of the stack. If no element is available, <see cref="LuaNil"/> is returned.
        /// </summary>
        /// <returns>The top element or <see cref="LuaNil"/> if there's no top element.</returns>
        public LuaValue PopOrNil() {
            if (this.Any) {
                return this.Pop();
            } else {
                return Nil;
            }
        }

        /// <summary>
        /// Creates a new array of <see cref="LuaValue"/> references currently contained on the stack.
        /// </summary>
        /// <param name="topFirst">Should the top-element of the stack be the first element.</param>
        /// <returns>An array representing the stack.</returns>
        public LuaValue[] ToArray(bool topFirst) => topFirst ? this.m_stack.Reverse().ToArray() : this.m_stack.Clone() as LuaValue[];

        /// <summary>
        /// Convert the stack into a string with a list of contained <see cref="LuaValue"/> elements.
        /// </summary>
        /// <param name="topFirst">Top-element should be the first element in returned string.</param>
        /// <returns>The converted string of <see cref="LuaValue"/> string representations.</returns>
        public string ToString(bool topFirst)
            => string.Join(", ", this.ToArray(topFirst).Select(x => x.Str()));

        /// <summary>
        /// Shift the whole stack to the left by <paramref name="count"/> elements.
        /// </summary>
        /// <param name="count">The amount of elements to shift.</param>
        /// <returns>The new top pointer of the stack.</returns>
        /// <exception cref="InvalidOperationException"/>
        public int ShiftLeft(int count) {
            if (count > this.Capacity) {
                throw new InvalidOperationException("Cannot shift more elements than available.");
            }
            var buffer = new LuaValue[this.Capacity];
            Array.Copy(this.m_stack, count, buffer, 0, this.Capacity - count);
            this.m_topPtr -= count;
            this.m_stack = buffer;
            return this.m_topPtr;
        }

        /// <summary>
        /// Lock stack operations op until the given stack index.
        /// </summary>
        /// <param name="at">The index to place lock at.</param>
        /// <exception cref="IndexOutOfRangeException"/>
        public void Lock(int at) {
            if (at > this.Top) {
                throw new IndexOutOfRangeException("Attempt to lock an index greater than top value.");
            }
            this.m_bottomPtr = at;
        }

        /// <summary>
        /// Unlocks the stack-lock placed by a <see cref="Lock(int)"/> command.
        /// </summary>
        public void Unlock() => this.m_bottomPtr = 0;

    }

}
