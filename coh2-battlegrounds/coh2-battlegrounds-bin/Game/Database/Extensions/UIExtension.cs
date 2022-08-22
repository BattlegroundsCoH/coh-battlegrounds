using System;
using System.Text.Json;

using Battlegrounds.ErrorHandling.CommonExceptions;
using Battlegrounds.Functional;

namespace Battlegrounds.Game.Database.Extensions;

public class UIExtension {

    public string ScreenName { get; init; }

    public string ShortDescription { get; init; }

    public string LongDescription { get; init; }

    public string Icon { get; init; }

    public string Symbol { get; init; }

    public string Portrait { get; init; }

    public int Position { get; init; }

    public UIExtension() {
        this.ScreenName = string.Empty;
        this.ShortDescription = string.Empty;
        this.LongDescription = string.Empty;
        this.Icon = string.Empty;
        this.Symbol = string.Empty;
        this.Portrait = string.Empty;
    }

    public static UIExtension FromJson(ref Utf8JsonReader reader) {
        string[] values = new string[6];
        int pos = 0;
        while (reader.Read() && reader.TokenType is not JsonTokenType.EndObject) {
            string prop = reader.ReadProperty() ?? throw new ObjectPropertyNotFoundException();
            if (prop is "Position") {
                pos = reader.GetInt32();
            } else {
                values[prop switch {
                    "LocaleName" => 0,
                    "LocaleDescriptionShort" => 1,
                    "LocaleDescriptionLong" => 2,
                    "Icon" => 3,
                    "Symbol" => 4,
                    "Portrait" => 5,
                    _ => throw new FormatException($"Unexpected UI property '{prop}'.")
                }] = reader.GetString() ?? string.Empty;
            }
        }
        return new() {
            ScreenName = values[0],
            ShortDescription = values[1],
            LongDescription = values[2],
            Icon = values[3],
            Symbol = values[4],
            Portrait = values[5],
            Position = pos
        };
    }

}

