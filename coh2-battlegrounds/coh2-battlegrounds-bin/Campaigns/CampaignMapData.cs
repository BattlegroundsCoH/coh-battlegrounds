using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
