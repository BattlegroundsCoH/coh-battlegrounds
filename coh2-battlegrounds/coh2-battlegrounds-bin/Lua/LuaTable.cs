using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Battlegrounds.Lua.Runtime;

using static Battlegrounds.Lua.LuaNil;

namespace Battlegrounds.Lua {

    /// <summary>
    /// Delegate for handing a key-value paired iteration.
    /// </summary>
    /// <param name="key">The key value used to index the value in table.</param>
    /// <param name="value">The value found at key position.</param>
    /// <returns>A <see cref="LuaNil"/> instance if iteration should continue. Otherwise a single return <see cref="LuaValue"/>.</returns>
    public delegate LuaValue Pairs(LuaValue key, LuaValue value);

    /// <summary>
    /// Representation of a Lua table. Extends <see cref="LuaValue"/> and can be enumerated over.
    /// </summary>
    public sealed class LuaTable : LuaValue, IEnumerable<KeyValuePair<LuaValue, LuaValue>>, ICloneable, IMetatableParent {

        /// <summary>
        /// Get an empty table. Do not write to this.
        /// </summary>
        public static readonly LuaTable Empty = new();

        /*private readonly struct TableKey {
            public readonly LuaValue key;
            public readonly int index;
            public TableKey(LuaValue valueKey, int index) {
                this.key = valueKey;
                this.index = index;
            }
            public override int GetHashCode() => this.key.GetHashCode();
            public override bool Equals(object obj) {
                if (obj is TableKey k) {
                    return k.key.Equals(this.key);
                } else {
                    return false;
                }
            }
        }

        private List<LuaValue> m_tableElements;
        private HashSet<TableKey> m_tableKeys;*/

        private readonly Dictionary<LuaValue, LuaValue> m_table;
        private readonly Dictionary<int, LuaValue> m_rawIndices;

        /// <summary>
        /// Get the (C#) size of the table.
        /// </summary>
        public int Size => this.m_table.Count;

        /// <summary>
        /// Get the Lua size of the table.
        /// </summary>
        public int Length => this.Len();

        /// <summary>
        /// Get or set the metatable of the table.
        /// </summary>
        public LuaTable MetaTable => this["__metatable"] as LuaTable;

        /// <summary>
        /// Get the Lua state the table was created in.
        /// </summary>
        public LuaState LuaState { get; }

        /// <summary>
        /// Get all the key entries in the table
        /// </summary>
        public LuaValue[] Keys => this.m_table.Keys.ToArray();

        /// <summary>
        /// Get all the key entries in the table as strings
        /// </summary>
        public string[] StringKeys => this.m_table.Keys.Select(x => x.Str()).ToArray();

        /// <summary>
        /// Get the first key in the table.
        /// </summary>
        public LuaValue FirstKey => this.m_table.First().Key;

        /// <summary>
        /// Get the last key in the table.
        /// </summary>
        public LuaValue LastKey => this.m_table.Last().Key;

        /// <summary>
        /// Initialize a new stateless <see cref="LuaTable"/> class without entries.
        /// </summary>
        public LuaTable() {
            this.m_table = new Dictionary<LuaValue, LuaValue>();
            this.m_rawIndices = new Dictionary<int, LuaValue>();
        }

        /// <summary>
        /// Initialize a new <see cref="LuaTable"/> class without entries.
        /// </summary>
        /// <param name="state"></param>
        public LuaTable(LuaState state) : this() => this.LuaState = state;

        public override bool Equals(LuaValue value) {
            if (value is LuaTable table) {
                return this.m_table.All(x => table.Contains(x));
            }
            return false;
        }

        public override bool Equals(object obj) => obj is LuaValue v ? this.Equals(v) : base.Equals(obj);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public LuaValue this[LuaValue key] {
            get => this.m_table.ContainsKey(key) ? this.m_table[key] : Nil;
            set => this.SetIndexValue(key, value);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public LuaValue this[string key] {
            get => this[new LuaString(key)];
            set => this[new LuaString(key)] = value;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public LuaValue this[int key] {
            get => this[new LuaNumber(key)];
            set => this[new LuaNumber(key)] = value;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <param name="value"></param>
        private void SetIndexValue(LuaValue index, LuaValue value) {
            if (this.m_table.ContainsKey(index)) {
                this.m_table[index] = value;
            } else {
                this.m_table[index] = value;
                this.m_rawIndices[this.m_rawIndices.Count] = index;
            }
        }

        /// <summary>
        /// Retrieve an element by <see cref="LuaValue"/> key.
        /// </summary>
        /// <typeparam name="T">Expected <see cref="LuaValue"/> type.</typeparam>
        /// <param name="key">The <see cref="LuaValue"/> key to use when getting value.</param>
        /// <returns>The <see cref="LuaValue"/> identified by <paramref name="key"/>. Otherwise <see langword="default"/>.</returns>
        public T ByKey<T>(LuaValue key) where T : LuaValue => this[key] is T v ? v : default;

        /// <summary>
        /// Retrieve a <see cref="LuaValue"/> by absolute index.
        /// </summary>
        /// <typeparam name="T">The expected <see cref="LuaValue"/> type.</typeparam>
        /// <param name="index">The absolute index to lookup table value by (0-indexed).</param>
        /// <returns>The value found at specified raw index.</returns>
        /// <exception cref="KeyNotFoundException"/>
        public T RawIndex<T>(int index) where T : LuaValue {
            if (this.m_rawIndices.ContainsKey(index)) {
                return this.m_table[this.m_rawIndices[index]] as T;
            } else {
                throw new KeyNotFoundException();
            }
        }

        /// <summary>
        /// Retrieve the key-value pair found at absolute <paramref name="index"/>.
        /// </summary>
        /// <param name="index">The absolte index to lookup pair by (0-indexed).</param>
        /// <returns>The value found at specified index.</returns>
        /// <exception cref="KeyNotFoundException"/>
        public KeyValuePair<LuaValue, LuaValue> KeyValueByRawIndex(int index) {
            if (this.m_rawIndices.TryGetValue(index, out LuaValue k)) {
                return new KeyValuePair<LuaValue, LuaValue>(k, this[k]);
            } else {
                throw new KeyNotFoundException();
            }
        }

        /// <summary>
        /// Retrieve an element by <see cref="string"/> key (Implict <see cref="LuaString"/>).
        /// </summary>
        /// <typeparam name="T">Expected <see cref="LuaValue"/> type.</typeparam>
        /// <param name="key">The <see cref="LuaValue"/> key to use when getting value.</param>
        /// <returns>The <see cref="LuaValue"/> identified by <paramref name="key"/>. Otherwise <see langword="default"/>.</returns>
        public T ByKey<T>(string key) where T : LuaValue {
            if (this[new LuaString(key)] is T v)
                return v;
            else
                return default;
        }

        /// <summary>
        /// Do a pairs run on the table.
        /// </summary>
        /// <param name="pairs">The delegate to run on each pair iteration.</param>
        public void Pairs(Action<LuaValue, LuaValue> pairsAction) => this.Pairs((k, v) => { pairsAction(k, v); return Nil; });

        /// <summary>
        /// Do a pairs run on the table.
        /// </summary>
        /// <param name="pairs">The delegate to run on each pair iteration.</param>
        /// <returns><see cref="LuaNil"/> if pairs iterator reached the end of the table. Otherwise returned <see cref="LuaValue"/>.</returns>
        public LuaValue Pairs(Pairs pairs) {
            var e = this.GetEnumerator();
            while (e.MoveNext()) {
                var s = pairs(e.Current.Key, e.Current.Value);
                if (!s.Equals(Nil)) {
                    return s;
                }
            }
            return Nil;
        }

        /// <summary>
        /// Get the table in a managed array format. Index from 0 to retrieve elements.
        /// </summary>
        /// <returns>An array of <see cref="LuaValue"/> objects.</returns>
        public LuaValue[] ToArray() => this.m_table.Values.ToArray();

        /// <summary>
        /// Get the table in a managed array format. Index from 0 to retrieve elements.
        /// </summary>
        /// <returns>An array of <typeparamref name="T"/> objects.</returns>
        public T[] ToArray<T>() where T : LuaValue => this.m_table.Values.Select(x => x as T).ToArray();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool Contains(LuaValue value) {
            foreach (var kv in this) {
                if (kv.Value == value) {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="managedObj"></param>
        /// <returns></returns>
        public bool Contains(object managedObj) => this.Contains(LuaMarshal.ToLuaValue(managedObj));

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool GetIfExists(string key, out LuaValue value)
            => this.GetIfExists(new LuaString(key), out value);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool GetIfExists(LuaString key, out LuaValue value) {
            foreach (var kv in this.m_table) {
                if (kv.Key.Equals(key)) {
                    value = kv.Value;
                    return true;
                }
            }
            value = Nil;
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public LuaValue GetOrDefault(string key, LuaValue defaultValue)
            => this.GetOrDefault(new LuaString(key), defaultValue);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public LuaValue GetOrDefault(LuaString key, LuaValue defaultValue) {
            if (this.m_table.TryGetValue(key, out LuaValue v)) {
                return v;
            } else {
                return defaultValue;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public LuaTable Clone() {
            LuaTable clone = new LuaTable();
            foreach (var kv in this.m_table) {
                clone.SetIndexValue(kv.Key, kv.Value);
            }
            return clone;
        }

        object ICloneable.Clone() => this.Clone();

        public override string Str() => this.LuaState is not null ? LuaMetatableUtil.__ToString(this) : $"0x{this.GetHashCode():X8}";

        public override string ToString() => this.Str();

        public override LuaType GetLuaType() => LuaType.LUA_TABLE;

        public IEnumerator<KeyValuePair<LuaValue, LuaValue>> GetEnumerator() => ((IEnumerable<KeyValuePair<LuaValue, LuaValue>>)this.m_table).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)this.m_table).GetEnumerator();

        public override int GetHashCode() => this.m_table.GetHashCode();

        /// <summary>
        /// Get the length (as defined by the lua 5.1 manual, NOT accurate).
        /// </summary>
        /// <returns>The amount of items indexable from 1 to n.</returns>
        public int Len() {
            int len = 0;
            while (true) {
                if (this.m_table.ContainsKey(new LuaNumber(len + 1))) {
                    len++;
                } else {
                    return len;
                }
            }
        }

        private LuaValue GetNext(LuaValue key, out LuaValue nextKey) {
            for (int i = 0; i < this.m_rawIndices.Count; i++) {
                if (this.m_rawIndices[i] == key) {
                    if (i + 1 < this.m_rawIndices.Count) {
                        nextKey = this.m_rawIndices[i + 1];
                        return this.m_table[nextKey];
                    }
                }
            }
            nextKey = Nil;
            return Nil;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="current"></param>
        /// <param name="i"></param>
        /// <returns></returns>
        public LuaValue Next(LuaValue current, out LuaValue i) {
            i = Nil;
            if (this.m_rawIndices.Count == 0) {
                return i;
            }
            if (current is LuaNil) {
                i = this.m_rawIndices[0];
                return this.m_table[i];
            } else {
                return GetNext(current, out i);
            }
        }

        /// <summary>
        /// Determines if the <see cref="LuaTable"/> can be considered to be an array.
        /// </summary>
        /// <returns><see langword="true"/> if table is an array; Otherwise <see langword="false"/>.</returns>
        public bool IsArray() {
            bool hasFirst = this.FirstKey is not null && this.FirstKey is LuaNumber startIndex && startIndex.IsInteger() && startIndex == 1;
            bool hasLast = this.LastKey is not null && this.LastKey is LuaNumber lastIndex && lastIndex.IsInteger();
            if (!hasFirst || !hasLast) {
                return false;
            }
            int diff = (this.LastKey as LuaNumber).ToInt() - (this.FirstKey as LuaNumber).ToInt();
            return diff == this.Len();
        }

    }

}
