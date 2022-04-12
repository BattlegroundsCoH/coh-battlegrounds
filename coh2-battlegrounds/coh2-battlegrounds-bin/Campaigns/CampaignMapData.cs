using Battlegrounds.Lua;

namespace Battlegrounds.Campaigns {

    public class CampaignMapData {

        public byte[] RawImageData { get; }

        public LuaTable Data { get; set; }

        public CampaignMapData(byte[] rawData) {
            this.RawImageData = rawData;
        }

    }

}
