using System;
using System.Text.Json;

using Battlegrounds.Functional;

namespace Battlegrounds.Game.Database.Extensions {

    public class UIExtension {

        public string ScreenName { get; init; }

        public string ShortDescription { get; init; }

        public string LongDescription { get; init; }

        public string Icon { get; init; }

        public string Symbol { get; init; }

        public string Portrait { get; init; }

        public static UIExtension FromJson(ref Utf8JsonReader reader) {
            string[] values = new string[6];
            while (reader.Read() && reader.TokenType is not JsonTokenType.EndObject) {
                string prop = reader.ReadProperty();
                values[prop switch {
                    "LocaleName" => 0,
                    "LocaleDescriptionShort" => 1,
                    "LocaleDescriptionLong" => 2,
                    "Icon" => 3,
                    "Symbol" => 4,
                    "Portrait" => 5,
                    _ => throw new Exception()
                }] = reader.GetString();
            }
            return new() {
                ScreenName = values[0] ?? string.Empty,
                ShortDescription = values[1] ?? string.Empty,
                LongDescription = values[2] ?? string.Empty,
                Icon = values[3] ?? string.Empty,
                Symbol = values[4] ?? string.Empty,
                Portrait = values[5] ?? string.Empty,
            };
        }

    }

}
