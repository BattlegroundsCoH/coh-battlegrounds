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
            public readonly string CaptureSquadBlueprint;
            public Entry(string faction, string sbp, string capsbp) {
                this.Faction = faction;
                this.SquadBlueprint = sbp;
                this.CaptureSquadBlueprint = capsbp;
            }
        }

        private readonly Entry[] m_entries;

        public SquadBlueprint GetSquad(Faction faction) {
            foreach (var e in this.m_entries) {
                if (e.Faction == faction.RbpPath) {
                    return BlueprintManager.FromBlueprintName<SquadBlueprint>(e.SquadBlueprint);
                }
            }
            return null;
        }

        public SquadBlueprint GetCaptureSquad(Faction faction) {
            foreach (var e in this.m_entries) {
                if (e.Faction == faction.RbpPath) {
                    return BlueprintManager.FromBlueprintName<SquadBlueprint>(e.CaptureSquadBlueprint);
                }
            }
            return null;
        }

        public DriverExtension(Entry[] entries) => this.m_entries = entries;

        public static DriverExtension FromJson(ref Utf8JsonReader reader) {
            List<Entry> entries = new();
            while (reader.Read() && reader.TokenType is not JsonTokenType.EndArray) {
                string[] strValues = new string[3];
                while (reader.Read() && reader.TokenType is not JsonTokenType.EndObject) {
                    strValues[reader.ReadProperty() switch {
                        "SBP" => 0,
                        "Army" => 1,
                        "Capture" => 2,
                        _ => throw new FormatException()
                    }] = reader.GetString();
                }
                entries.Add(new(strValues[1], strValues[0], strValues[2]));
            }
            return new(entries.ToArray());
        }

    }
}
