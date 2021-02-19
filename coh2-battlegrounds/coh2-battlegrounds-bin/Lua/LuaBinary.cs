using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Battlegrounds.Util;

namespace Battlegrounds.Lua {
    
    /// <summary>
    /// Static utility class for converting Lua data to and from binary.
    /// </summary>
    public static class LuaBinary {

        public enum LuaBinaryType : byte {
            LT_Nil = 0x0,
            LT_Str = 0x1,
            LT_Num = 0x2,
            Lt_Bol = 0x3,
        }

        public const int PUSH = 0x4;
        public const int SET = 0x5;
        public const int NEW = 0x6;

        // This is NOT actual binary opcodes - this is a simplified version for easily storing Lua tables in a binary format
        private abstract record LuaBinIns();
        private record LuaPush(uint Index) : LuaBinIns;
        private record LuaSet() : LuaBinIns;
        private record LuaNewTable() : LuaBinIns;

        public static byte[] SaveAsBinary(LuaTable table, Encoding encoding = null) {

            if (encoding is null) {
                encoding = Encoding.ASCII;
            }
            
            DistinctList<LuaValue> values = new DistinctList<LuaValue>();
            List<LuaBinIns> instructions = new List<LuaBinIns>();

            void SubTable(LuaTable table) {
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
                instructions.Add(new LuaNewTable());
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

        public static LuaValue FromBinary(MemoryStream binaryData, Encoding encoding) {

            // Create reader
            BinaryReader reader = new BinaryReader(binaryData, encoding, true);

            int len = reader.ReadInt32();
            string binaryType = Encoding.ASCII.GetString(reader.ReadBytes(len));

            if (binaryType.CompareTo("LUA-TABLE") == 0) {

            }

            return new LuaNil();

        }

        private static LuaBinaryType GetLuaBinaryTypeFromValue(LuaValue v) => v switch {
            LuaString => LuaBinaryType.LT_Str,
            LuaBool => LuaBinaryType.Lt_Bol,
            LuaNumber => LuaBinaryType.LT_Num,
            LuaNil => LuaBinaryType.LT_Nil,
            _ => throw new Exception()
        };

        private static byte[] BinaryValue(LuaValue value, Encoding encoding) => value switch {
            LuaString s => BitConverter.GetBytes(s.Length).Concat(value.Str().Encode(encoding)).ToArray(),
            LuaBool b => new byte[] { b.IsTrue ? 0x1 : 0x0 },
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
                    _ => throw new Exception()
                }
            );

    }

}
