using System.Collections.Generic;

using Battlegrounds.Lua.Generator;
using Battlegrounds.Lua.Generator.RuntimeServices;

namespace Battlegrounds.Game.Gameplay.DataConverters {

    public class AbilityConverter : LuaConverter<Ability> {
        
        public override void Write(LuaSourceBuilder luaSourceBuilder, Ability value) {
            luaSourceBuilder.Writer.WriteTableValue(luaSourceBuilder.BuildTableRaw(new Dictionary<string, object>() {
                ["abp"] = value.ABP.GetScarName(),
                ["unlock"] = value.UnlockUpgrade.GetScarName(),
                ["max_use"] = value.MaxUse,
                ["nofacing"] = !value.ABP.HasFacingPhase
            }));
        }

    }

}
