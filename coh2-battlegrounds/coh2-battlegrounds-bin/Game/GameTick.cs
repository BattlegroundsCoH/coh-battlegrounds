using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using coh2_battlegrounds_bin.Util;

namespace coh2_battlegrounds_bin.Game {
    
    /// <summary>
    /// 
    /// </summary>
    public sealed class GameTick {

        private uint m_tick;
        private uint m_tickIndex;
        private uint m_bundleCount;

        private List<GameEvent> m_events;

        public GameTick() {
            this.m_tick = 0;
            this.m_tickIndex = 0;
            this.m_events = new List<GameEvent>();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="reader"></param>
        public void Parse(BinaryReader reader) {

            reader.Skip(1);
            //byte[] tickData = reader.ReadBytes((int)reader.ReadUInt32());

            this.m_tick = reader.ReadUInt32();

            reader.Skip(4);

            this.m_bundleCount = reader.ReadUInt32();

            for (uint i = 0; i < this.m_bundleCount; i++) {

                reader.Skip(8);

                uint count = reader.ReadUInt32();

                reader.Skip(1);

                if (count != 0) {

                    uint j = 0;

                    uint k = 0;

                    while (count > j) {

                        ushort length = reader.ReadUInt16();

                        byte[] data = reader.ReadBytes(length - 2);

                        GameEvent ge = new GameEvent(data);
                        m_events.Add(ge);

                        j += length;
                        k++;

                    }

                }

            }

        }

    }

}
