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

            byte[] tickData = reader.ReadBytes((int)reader.ReadUInt32());

            this.m_tick = BitConverter.ToUInt32(tickData, 1);
            this.m_bundleCount = BitConverter.ToUInt32(tickData, 9);

            int i = 12;
            for (uint bundleCount = 0; bundleCount < m_bundleCount; ++bundleCount) {
                i += ParseEvents(i, tickData) + 13;
            }

        }

        private int ParseEvents(int i, byte[] data) {

            int bundleLength = (int)BitConverter.ToUInt32(data, i + 9);

            int j = 14;
            while (j < bundleLength + 2) {
                
                int length = BitConverter.ToInt16(data, i + j);
                
                byte[] bundleData = new byte[length];

                for (int k = 0; k < length; k++) {
                    bundleData[k] = data[k + i + j];
                }

                this.m_events.Add(new GameEvent(bundleData));

                j += BitConverter.ToUInt16(data, i + j);
            }

            return bundleLength;

        }

    }

}
