using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Battlegrounds.Lua;

namespace Battlegrounds.Gfx {
    
    public class GfxAtlas {

        public GfxAtlas() { }

        public byte[] AsBinary() => Array.Empty<byte>();

        public static GfxAtlas FromBinary() => new GfxAtlas();

        public static GfxAtlas FromLua(LuaTable gfxTable, string gfxFolder) => new GfxAtlas();

    }

}
