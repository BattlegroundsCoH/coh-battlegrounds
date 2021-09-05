using System.Collections.Generic;

using Battlegrounds.Functional;
using Battlegrounds.Lua.Generator;
using Battlegrounds.Lua.Generator.RuntimeServices;

namespace Battlegrounds.Game.Gameplay.DataConverters {

    public class AbilityConverter : LuaConverter<Ability> {
        
        public override void Write(LuaSourceBuilder luaSourceBuilder, Ability value) {
            var data = new Dictionary<string, object>() {
                ["abp"] = value.ABP.GetScarName(),
                ["unlock"] = value.UnlockUpgrade.GetScarName(),
                ["max_use"] = value.MaxUse,
                ["uses"] = value.MaxUse,
                ["nofacing"] = !value.ABP.HasFacingPhase
            };
            if (value.GrantingBlueprints?.Length > 0) {
                data["granters"] = value.GrantingBlueprints.Map(x => x.GetScarName());
            }
            luaSourceBuilder.Writer.WriteTableValue(luaSourceBuilder.BuildTableRaw(data));
        }

    }

}
