using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Battlegrounds.Lua;

namespace coh2_battlegrounds_console {
    
    public static class CampaignCompiler {

        public static void Compile(string dir) {

            // Create lua state
            LuaState settingsState = new LuaState();
            settingsState._G["MAP"] = LuaValue.ToLuaValue(0);
            settingsState._G["LOCALE"] = LuaValue.ToLuaValue(1);

            // Verify file exists
            string settingsFile = Path.Combine(dir, "campaign.lua");
            if (!File.Exists(settingsFile)) {
                Console.WriteLine("Invalid campaign folder.");
                return;
            }

            // Load lua file
            if (settingsState.DoFile(settingsFile) is LuaTable manifest) {



            }

        }

    }

}
