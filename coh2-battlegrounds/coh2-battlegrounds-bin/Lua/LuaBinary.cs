using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using Battlegrounds.Util;
using Battlegrounds.Util.Lists;
using Battlegrounds.Functional;

namespace Battlegrounds.Lua {
    
    /// <summary>
    /// Static utility class for converting Lua data to and from binary.
    /// </summary>
    public static class LuaBinary {

        /// <summary>
        /// Binary representation of a Lua type.
        /// </summary>
        public enum LuaBinaryType : byte {
            LT_Nil = 0x0,
            LT_Str = 0x1,
            LT_Num = 0x2,
            Lt_Bol = 0x3,
        }

        // Opcode identifiers
        public const int PUSH = 0x4;
        public const int SET = 0x5;
        public const int NEW = 0x6;
        public const int POP = 0x7;

        // This is NOT actual binary opcodes - this is a simplified version for easily storing Lua tables in a binary format
        private abstract record LuaBinIns();
        private record LuaPush(uint Index) : LuaBinIns;
        private record LuaSet() : LuaBinIns;
        private record LuaNewTable() : LuaBinIns;
        private record LuaPopTable() : LuaBinIns;

        /// <summary>
        /// Save a <see cref="LuaTable"/> in a compact binary form that can easily be reconstructed.
        /// </summary>
        /// <param name="table">The <see cref="LuaTable"/> to save in binary form.</param>
        /// <param name="encoding">The encoding to use when saving values.</param>
        /// <returns>A <see cref="byte"/> array containg values and instructions for rebuilding the <see cref="LuaTable"/>.</returns>
        public static byte[] SaveAsBinary(LuaTable table, Encoding encoding = null) {

            if (encoding is null) {
                encoding = Encoding.ASCII;
            }
            
            DistinctList<LuaValue> values = new DistinctList<LuaValue>();
            List<LuaBinIns> instructions = new List<LuaBinIns>();

            void SubTable(LuaTable table) {
                instructions.Add(new LuaNewTable());
                table.Pairs((k, v) => {
                    values.Add(k);
                    instructions.Add(new LuaPush((uint)values.IndexOf(k)));
                    if (v is LuaTable sub) {
                        SubTable(sub);
                    } else {
                        values.Add(v);
                        instructions.Add(new LuaPush((uint)values.IndexOf(v)));
                        instructions.Add(new LuaSet());
                    }
                });
                instructions.Add(new LuaPopTable());
            }

            SubTable(table);

            using MemoryStream ms = new MemoryStream();
            using BinaryWriter bw = new BinaryWriter(ms, encoding);

            byte[] lidentifier = "LUA-TABLE".Encode();
            bw.Write(lidentifier.Length);
            bw.Write(lidentifier);

            // Write values (const table)
            bw.Write(values.Count);
            values.ForEach(x => {
                bw.Write((byte)GetLuaBinaryTypeFromValue(x));
                byte[] v = BinaryValue(x, encoding);
                if (v.Length > 0) {
                    bw.Write(v);
                }
            });

            // Write instructions
            instructions.ForEach(x => BinaryCode(x, bw));

            ms.Position = 0;
            return ms.ToArray();

        }

        /// <summary>
        /// Rebuild a <see cref="LuaValue"/> that has been saved in binary form.
        /// </summary>
        /// <param name="binaryData">The <see cref="MemoryStream"/> containing the binary data of the value.</param>
        /// <param name="encoding">The encoding to use when reading string data.</param>
        /// <returns>A rebuilt <see cref="LuaValue"/> if successful. Otherwise <see cref="LuaNil"/>.</returns>
        public static LuaValue FromBinary(MemoryStream binaryData, Encoding encoding) {

            // Create reader
            BinaryReader reader = new BinaryReader(binaryData, encoding, true);

            // Read type
            string binaryType = reader.ReadASCIIString();

            // Is it binary?
            if (binaryType.CompareTo("LUA-TABLE") == 0) {

                // Read count
                LuaValue[] values = new LuaValue[reader.ReadInt32()];

                // Get all values
                for (int i = 0; i < values.Length; i++) {
                    values[i] = GetLuaBinaryTypeFromByte(reader.ReadByte()) switch {
                        LuaBinaryType.LT_Nil => new LuaNil(),
                        LuaBinaryType.LT_Str => new LuaString(reader.ReadEncodedString(encoding)),
                        LuaBinaryType.LT_Num => new LuaNumber(BitConverter.ToDouble(reader.ReadBytes(sizeof(double)))),
                        LuaBinaryType.Lt_Bol => new LuaBool(reader.ReadByte() == 0x1),
                        _ => throw new Exception()
                    };
                }

                // Create list for insructions
                List<LuaBinIns> instructions = new List<LuaBinIns>();

                // Read instructions
                while (!reader.HasReachedEOS()) {
                    instructions.Add(reader.ReadByte() switch {
                        PUSH => new LuaPush(reader.ReadUInt32()),
                        SET => new LuaSet(),
                        NEW => new LuaNewTable(),
                        POP => new LuaPopTable(),
                        _ => throw new Exception()
                    });
                }

                // Table maintainer
                LuaTable current = new LuaTable();

                // Runstack
                Stack<LuaValue> stackValues = new Stack<LuaValue>();
                stackValues.Push(new LuaString("_G"));

                // Execute instructions
                for (int i = 0; i < instructions.Count; i++) {
                    switch (instructions[i]) {
                        case LuaPush push:
                            stackValues.Push(values[push.Index]);
                            break;
                        case LuaSet:
                            var v = stackValues.Pop();
                            var k = stackValues.Pop();
                            current[k] = v;
                            break;
                        case LuaPopTable:
                            var top = stackValues.Pop() as LuaTable;
                            var tk = stackValues.Pop();
                            top[tk] = current;
                            current = top;
                            break;
                        case LuaNewTable:
                            stackValues.Push(current);
                            current = new LuaTable();
                            break;
                        default:
                            break;
                    }
                }

                // Return current table (Top)
                return current["_G"] as LuaTable;

            }

            // Return nil
            return new LuaNil();

        }

        private static LuaBinaryType GetLuaBinaryTypeFromByte(byte b) => b switch {
            0x0 => LuaBinaryType.LT_Nil,
            0x1 => LuaBinaryType.LT_Str,
            0x2 => LuaBinaryType.LT_Num,
            0x3 => LuaBinaryType.Lt_Bol,
            _ => throw new Exception(),
        };

        private static LuaBinaryType GetLuaBinaryTypeFromValue(LuaValue v) => v switch {
            LuaString => LuaBinaryType.LT_Str,
            LuaBool => LuaBinaryType.Lt_Bol,
            LuaNumber => LuaBinaryType.LT_Num,
            LuaNil => LuaBinaryType.LT_Nil,
            _ => throw new Exception()
        };

        private static byte[] BinaryValue(LuaValue value, Encoding encoding) => value switch {
            LuaString s => value.Str().Encode(encoding).Then(b => BitConverter.GetBytes(b.Length).Concat(b).ToArray()),
            LuaBool b => new byte[] { (byte)(b.IsTrue ? 0x1 : 0x0) },
            LuaNumber n => BitConverter.GetBytes(n),
            LuaNil => Array.Empty<byte>(),
            _ => throw new Exception()
        };

        private static void BinaryCode(LuaBinIns instruction, BinaryWriter writer) 
            => writer.Write(
                instruction switch {
                    LuaPush push => (new byte[] { PUSH, }).Concat(BitConverter.GetBytes(push.Index)).ToArray(),
                    LuaSet => new byte[] { SET },
                    LuaNewTable => new byte[] { NEW },
                    LuaPopTable => new byte[] { POP },
                    _ => throw new Exception()
                }
            );

    }

}
