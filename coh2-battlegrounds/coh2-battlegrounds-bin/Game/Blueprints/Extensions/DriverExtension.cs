using System;
using System.Collections.Generic;
using System.Text.Json;

using Battlegrounds.Functional;

namespace Battlegrounds.Game.Blueprints.Extensions;

public class DriverExtension
{

    public readonly struct Entry
    {
        public readonly string Faction;
        public readonly string SquadBlueprint;
        public readonly string CaptureSquadBlueprint;
        public Entry(string faction, string sbp, string capsbp)
        {
            Faction = faction;
            SquadBlueprint = sbp;
            CaptureSquadBlueprint = capsbp;
        }
    }

    private readonly Entry[] m_entries;

    /// <summary>
    /// 
    /// </summary>
    public Entry[] Drivers => m_entries;

    /// <summary>
    /// Get if there are any drivers in the driver extension
    /// </summary>
    public bool Any => m_entries.Length > 0;

    public DriverExtension(Entry[] entries) => m_entries = entries;

    public static DriverExtension FromJson(ref Utf8JsonReader reader)
    {
        List<Entry> entries = new();
        while (reader.Read() && reader.TokenType is not JsonTokenType.EndArray)
        {
            string[] strValues = new string[3];
            while (reader.Read() && reader.TokenType is not JsonTokenType.EndObject)
            {
                strValues[reader.ReadProperty() switch
                {
                    "SBP" => 0,
                    "Army" => 1,
                    "Capture" => 2,
                    _ => throw new FormatException()
                }] = reader.GetString() ?? string.Empty;
            }
            entries.Add(new(strValues[1], strValues[0], strValues[2]));
        }
        return new(entries.ToArray());
    }

}
