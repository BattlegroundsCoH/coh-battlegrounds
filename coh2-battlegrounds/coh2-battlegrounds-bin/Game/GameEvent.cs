using System;
using System.Collections.Generic;
using System.Text;

namespace coh2_battlegrounds_bin.Game {
    
    /// <summary>
    /// 
    /// </summary>
    public class GameEvent {

        /// <summary>
        /// 
        /// </summary>
        public GamePosition? FirstPosition { get; }

        /// <summary>
        /// 
        /// </summary>
        public GamePosition? SecondPosition { get; }

        /// <summary>
        /// 
        /// </summary>
        public byte Type { get; }

        /// <summary>
        /// 
        /// </summary>
        public byte PlayerID { get; }

        /// <summary>
        /// 
        /// </summary>
        public ushort ActionID { get; }

        /// <summary>
        /// 
        /// </summary>
        public ushort UnitID { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="eventData"></param>
        public GameEvent(byte[] eventData) {

            this.FirstPosition = null;
            this.SecondPosition = null;

            this.Type = eventData[0];
            this.PlayerID = eventData[3];

            ushort index = BitConverter.ToUInt16(eventData, 4);
            ushort category = BitConverter.ToUInt16(eventData, 6);
            ushort objectId = BitConverter.ToUInt16(eventData, 8);

            ushort id = this.FindID(eventData, category);

            if (this.Type < (byte)GameEventType.PCMD_COUNT) {
                Console.WriteLine(((GameEventType)this.Type).ToString() + " : " + id + " : " + objectId + " : " + index);
                if (this.Type == (byte)GameEventType.CMD_BuildSquad || this.Type == (byte)GameEventType.CMD_Upgrade || this.Type == (byte)GameEventType.SCMD_Upgrade) {

                    uint pbgid = BitConverter.ToUInt32(eventData, 13);

                    Console.WriteLine(pbgid);
                }
            } else {
                //Console.WriteLine(this.Type);
            }

        }

        private ushort FindID(byte[] eventData, ushort category) {
            ushort num = (ushort)((uint)category & 15U);
            ushort id = this.toUInt16(eventData, num == (ushort)0 ? 12 : (int)(ushort)(4 * (int)num + 9));
            id = id  == (ushort)195 ? (ushort)314 : id;
            id = id <= (ushort)2000 || this.Type != (byte)GameEventType.SCMD_Ability ? id : (ushort)314;
            return id;
        }

        protected ushort toUInt16(byte[] eventData, int offset) {
            return offset + 1 < eventData.Length ? BitConverter.ToUInt16(eventData, offset) : ushort.MaxValue;
        }

    }

}
