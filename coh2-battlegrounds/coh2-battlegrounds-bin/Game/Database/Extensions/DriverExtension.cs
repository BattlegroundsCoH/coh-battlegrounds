using System;
using System.Collections.Generic;
using System.Text.Json;

using Battlegrounds.Functional;
using Battlegrounds.Game.Database.Management;
using Battlegrounds.Game.Gameplay;

namespace Battlegrounds.Game.Database.Extensions {
    
    public class DriverExtension {

        public readonly struct Entry {
            public readonly string Faction;
            public readonly string SquadBlueprint;
            public Entry(string faction, string sbp) {
                this.Faction = faction;
                this.SquadBlueprint = sbp;
            }
        }

        private Entry[] m_entries;

        public SquadBlueprint GetSquad(Faction faction) {
            foreach (Entry e in this.m_entries) {
                if (e.Faction == faction.RbpPath) {
                    return BlueprintManager.FromBlueprintName<SquadBlueprint>(e.SquadBlueprint);
                }
            }
            return null;
        }

        public DriverExtension(Entry[] entries) {
            this.m_entries = entries;
        }

        public static DriverExtension FromJson(ref Utf8JsonReader reader) {
            List<Entry> entries = new();
            while (reader.Read() && reader.TokenType is not JsonTokenType.EndArray) {
                string[] strValues = new string[2];
                while (reader.Read() && reader.TokenType is not JsonTokenType.EndObject) {
                    strValues[reader.ReadProperty() switch {
                        "SBP" => 0,
                        "Army" => 1,
                        _ => throw new NotSupportedException()
                    }] = reader.GetString();
                }
                entries.Add(new(strValues[1], strValues[0]));
            }
            return new(entries.ToArray());
        }

    }
}
