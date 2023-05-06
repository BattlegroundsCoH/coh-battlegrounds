using System;
using System.Text.Json;

using Battlegrounds.Errors.Common;
using Battlegrounds.Functional;

namespace Battlegrounds.Game.Blueprints.Extensions;

/// <summary>
/// Class representing a blueprint attribute
/// </summary>
public sealed class UIExtension {

    /// <summary>
    /// Get or initialise the screen name
    /// </summary>
    public string ScreenName { get; init; }

    /// <summary>
    /// Get or initialise the short description
    /// </summary>
    public string ShortDescription { get; init; }

    /// <summary>
    /// Get or initialise the long description.
    /// </summary>
    public string LongDescription { get; init; }

    /// <summary>
    /// Get or initialise the icon.
    /// </summary>
    public string Icon { get; init; }

    /// <summary>
    /// Get or initialise the symbol identifier.
    /// </summary>
    public string Symbol { get; init; }

    /// <summary>
    /// Get or initialise the portrait identifier.
    /// </summary>
    public string Portrait { get; init; }

    /// <summary>
    /// Get or initialise the position the icon should be displayed at.
    /// </summary>
    public int Position { get; init; }

    /// <summary>
    /// Initialise a new <see cref="UIExtension"/> instance.
    /// </summary>
    public UIExtension() {
        ScreenName = string.Empty;
        ShortDescription = string.Empty;
        LongDescription = string.Empty;
        Icon = string.Empty;
        Symbol = string.Empty;
        Portrait = string.Empty;
    }

    /// <summary>
    /// Read a <see cref="UIExtension"/> from a <see cref="Utf8JsonReader"/>.
    /// </summary>
    /// <param name="reader">The reader to read data from.</param>
    /// <returns>A <see cref="UIExtension"/> that is represented by the incoming json data.</returns>
    /// <exception cref="ObjectPropertyNotFoundException"></exception>
    /// <exception cref="FormatException"></exception>
    public static UIExtension FromJson(ref Utf8JsonReader reader) {
        string[] values = { "", "", "", "", "", "" };
        int pos = 0;
        while (reader.Read() && reader.TokenType is not JsonTokenType.EndObject) {
            string prop = reader.ReadProperty() ?? throw new ObjectPropertyNotFoundException();
            if (prop is "Position") {
                pos = reader.GetInt32();
            } else {
                var val = reader.GetString();
                values[prop switch {
                    "LocaleName" => 0,
                    "LocaleDescriptionShort" => 1,
                    "LocaleDescriptionLong" => 2,
                    "Icon" => 3,
                    "Symbol" => 4,
                    "Portrait" => 5,
                    _ => throw new FormatException($"Unexpected UI property '{prop}'.")
                }] = string.IsNullOrEmpty(val) ? string.Empty : val;
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
