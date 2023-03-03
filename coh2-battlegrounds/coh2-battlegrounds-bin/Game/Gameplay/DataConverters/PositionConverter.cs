using System.Collections.Generic;
using Battlegrounds.Data.Generators.Lua;
using Battlegrounds.Data.Generators.Lua.RuntimeServices;

namespace Battlegrounds.Game.Gameplay.DataConverters;

public class PositionConverter : LuaConverter<GamePosition> {
    
    public override void Write(LuaSourceBuilder luaSourceBuilder, GamePosition value) {
        luaSourceBuilder.Writer.WriteTableValue(luaSourceBuilder.BuildTableRaw(new Dictionary<string,object>() {
            ["x"] = value.X,
            ["y"] = value.Y,
            ["z"] = value.Z
        }));
    }

}
