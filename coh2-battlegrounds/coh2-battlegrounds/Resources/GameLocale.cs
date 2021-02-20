using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Battlegrounds.Game.DataSource;

namespace BattlegroundsApp.Resources {
    public static class GameLocale {

        private static UcsFile LocaleFile { get; set; }

        static GameLocale() {
            string language = Battlegrounds.BattlegroundsInstance.Localize.Language.ToString();
            if (language == "Default") {
                language = "English";
            }
            string localePath = Path.Combine(Battlegrounds.Pathfinder.GetOrFindCoHPath(), $"CoH2\\Locale\\{language}\\RelicCoH2.{language}.ucs");
            if (File.Exists(localePath)) {
                LocaleFile = UcsFile.LoadFromFile(localePath);
            }
        }

        public static string GetString(uint key) {
            return LocaleFile[key];
        }

    }
}
