﻿using System.Collections.Generic;
using Battlegrounds.Data.Generators.Lua;
using Battlegrounds.Data.Generators.Lua.RuntimeServices;

namespace Battlegrounds.Game.Gameplay.DataConverters;

public class ModifierConverter : LuaConverter<Modifier> {

    public override void Write(LuaSourceBuilder luaSourceBuilder, Modifier value) {
        Dictionary<string, object> modifierData = new() {
            ["name"] = value.Name,
            ["value"] = value.Value
        };
        luaSourceBuilder.Writer.WriteTableValue(luaSourceBuilder.BuildTableRaw(modifierData));
    }

}

