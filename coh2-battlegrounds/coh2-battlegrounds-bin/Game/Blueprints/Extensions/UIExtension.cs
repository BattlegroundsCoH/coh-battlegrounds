using System;
using System.Text.Json;

using Battlegrounds.Errors.Common;
using Battlegrounds.Functional;
using Battlegrounds.Game.DataSource;

namespace Battlegrounds.Game.Blueprints.Extensions;

/// <summary>
/// Class representing a blueprint attribute containing UI information.
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

    /// <summary>
    /// Determines if the specified UIExtension object is empty in terms of text content.
    /// </summary>
    /// <param name="ui">The UIExtension object to check for emptiness.</param>
    /// <returns>True if the text content is empty, regardless of graphical content; otherwise, false.</returns>
    public static bool IsEmpty(UIExtension ui)
        => IsEmpty(ui, false);

    /// <summary>
    /// Determines if the specified UIExtension object is empty in terms of text and, optionally, graphical content.
    /// </summary>
    /// <param name="ui">The UIExtension object to check for emptiness.</param>
    /// <param name="isCompletelyEmpty">If true, considers the UIExtension empty only if both text and graphical content are empty; otherwise, only checks text content.</param>
    /// <returns>True if the specified UIExtension object is empty based on the provided criteria; otherwise, false.</returns>
    public static bool IsEmpty(UIExtension ui, bool isCompletelyEmpty) {
        bool hasNoText = UcsString.IsNullOrEmpty(ui.ScreenName) && UcsString.IsNullOrEmpty(ui.ShortDescription) && UcsString.IsNullOrEmpty(ui.LongDescription);
        bool hasNoGfx = !isCompletelyEmpty || (string.IsNullOrEmpty(ui.Portrait) && string.IsNullOrEmpty(ui.Icon) && string.IsNullOrEmpty(ui.Symbol));
        return hasNoText && hasNoGfx;
    }

}
