using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using Battlegrounds.Game.Battlegrounds;
using Battlegrounds.Json;
using Battlegrounds.Modding;
using Battlegrounds.Steam;

namespace Battlegrounds {

    /// <summary>
    /// 
    /// </summary>
    public static class BattlegroundsInstance {

        /// <summary>
        /// 
        /// </summary>
        public class InternalInstance : IJsonObject {

            /// <summary>
            /// 
            /// </summary>
            [JsonReference(typeof(SteamUser))] public SteamUser User { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public InternalInstance() {
                this.User = null;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <returns></returns>
            public string ToJsonReference() => throw new NotSupportedException();

        }

        private static InternalInstance __instance;

        /// <summary>
        /// 
        /// </summary>
        public static SteamUser LocalSteamuser {
            get => __instance.User;
            set => __instance.User = value;
        }

        /// <summary>
        /// 
        /// </summary>
        public static string BattlegroundHubAddress => "194.37.80.249";

        private static ITuningMod __bgTuningInstance;

        public static ITuningMod BattleGroundsTuningMod => __bgTuningInstance;

        /// <summary>
        /// 
        /// </summary>
        public static void LoadInstance() {

            __instance = JsonParser.Parse<InternalInstance>("local.json");
            if (__instance == null) {
                __instance = new InternalInstance();
            }

            __bgTuningInstance = new BattlegroundsTuning();

        }

        /// <summary>
        /// 
        /// </summary>
        public static void SaveInstance() 
            => File.WriteAllText("local.json", (__instance as IJsonObject).Serialize());

    }

}
