using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Text.Json;

using Battlegrounds.Game.Database;
using Battlegrounds.Game.DataCompany;

using Battlegrounds.Modding;

using Battlegrounds.Verification;
using Battlegrounds.Functional;

using Battlegrounds.Lua.Generator.RuntimeServices;
using Battlegrounds.Lua.Generator;
using System.Linq;

namespace Battlegrounds.Game.Gameplay.DataConverters {

    /// <summary>
    /// Static utility class containing different writers for different objects
    /// </summary>
    public static class SquadWriter {

        /// <summary>
        /// Class for generating a Battlegrounds representation of a squad.
        /// </summary>
        public class SquadLua : LuaConverter<Squad> {

            public override void Write(LuaSourceBuilder luaSourceBuilder, Squad value) {

                // Get base data
                var data = new Dictionary<string, object>() {
                    ["bp_name"] = value.SBP.GetScarName(),
                    ["company_id"] = value.SquadID,
                    ["symbol"] = value.SBP.UI.Symbol,
                    ["category"] = value.GetCategory(true),
                    ["phase"] = (byte)(value.DeploymentPhase - 1),
                    ["veterancy_rank"] = value.VeterancyRank,
                    ["veterancy_progress"] = value.VeterancyProgress,
                    ["upgrades"] = value.Upgrades.Select(x => x.GetScarName()),
                    ["modifiers"] = value.Modifiers,
                    ["slot_items"] = value.SlotItems.Select(x => x.GetScarName()),
                    ["spawned"] = false,
                    ["cost"] = value.GetCost()
                };

                // Write support if any
                if (value.SupportBlueprint is not null) {
                    data["transport"] = GetSupportBlueprint(value);
                }

                // Write crew if any
                if (value.Crew is Squad crew) {
                    data["crew"] = new Dictionary<string, object>() {
                        ["bp_name"] = crew.SBP.GetScarName(),
                        ["company_id"] = crew.SquadID,
                        ["symbol"] = crew.SBP.UI.Symbol,
                        ["veterancy_rank"] = crew.VeterancyRank,
                        ["veterancy_progress"] = crew.VeterancyProgress,
                        ["upgrades"] = crew.Upgrades.Select(x => x.GetScarName()),
                        ["modifiers"] = crew.Modifiers,
                        ["slot_items"] = crew.SlotItems.Select(x => x.GetScarName())
                    };
                }

                // Write AI nudges
                if (value.SBP.Types.IsHeavyArmour || value.SBP.Types.IsHeavyArtillery) {
                    data["heavy"] = true;
                }
                if (value.SBP.Types.IsCommandUnit) {
                    data["command"] = true;
                }
                if (value.SBP.Types.IsSniper || value.SBP.Types.IsSpecialInfantry) {
                    data["special"] = true;
                }

                // Write to builder
                luaSourceBuilder.Writer.WriteTableValue(luaSourceBuilder.BuildTableRaw(data));

            }

            private static Dictionary<string, object> GetSupportBlueprint(Squad value) {
                var data = new Dictionary<string, object>() {
                    ["sbp"] = value.SupportBlueprint.GetScarName(),
                    ["symbol"] = (value.SupportBlueprint as SquadBlueprint).UI.Symbol,
                    ["mode"] = (byte)value.DeploymentMethod,
                };
                if (value.SBP.Types.IsHeavyArtillery || (!value.SBP.Types.IsHeavyArtillery && value.SBP.Types.IsAntiTank)) {
                    data["tow"] = true;
                }
                return data;
            }

        }

        /// <summary>
        /// Class for constructing a <see cref="Squad"/> from json data.
        /// </summary>
        public class SquadJson : JsonConverter<Squad> {

            public override Squad Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {

                // Create builder
                UnitBuilder unitBuilder = new();

                // Read open object
                reader.Read();

                // Read squad ID
                ushort squadID = (ushort)ReadNumberProperty(ref reader, nameof(Squad.SquadID));

                // Read checksum
                string checksum = ReadStringProperty(ref reader, nameof(Squad.Checksum));
                string sbpName = ReadStringProperty(ref reader, nameof(Squad.SBP));
                ModGuid modGuid = ModGuid.BaseGame;

                // Read 
                if (reader.GetString() is nameof(Squad.SBP.PBGID.Mod)) {
                    modGuid = ModGuid.FromGuid(ReadStringProperty(ref reader, nameof(Squad.SBP.PBGID.Mod)));
                }

                // Set mod guid and get deployment phase and combat time
                unitBuilder.SetModGUID(modGuid).SetBlueprint(sbpName);
                unitBuilder.SetCustomName(ReadStringPropertyIfThere(ref reader, nameof(Squad.CustomName), null));
                unitBuilder.SetDeploymentPhase(Enum.Parse<DeploymentPhase>(ReadStringPropertyIfThere(ref reader, nameof(Squad.DeploymentPhase), nameof(DeploymentPhase.PhaseNone))));
                unitBuilder.SetCombatTime(TimeSpan.Parse(ReadStringPropertyIfThere(ref reader, nameof(Squad.CombatTime), TimeSpan.Zero.ToString())));

                // Get deployment method
                string supportBP = ReadStringPropertyIfThere(ref reader, nameof(Squad.SupportBlueprint), string.Empty);
                if (!string.IsNullOrEmpty(supportBP)) {
                    unitBuilder.SetTransportBlueprint(supportBP);
                }
                unitBuilder.SetDeploymentMethod(Enum.Parse<DeploymentMethod>(ReadStringPropertyIfThere(ref reader, nameof(Squad.DeploymentMethod), nameof(DeploymentMethod.None))));

                // Get veterancy
                unitBuilder.SetVeterancyRank((byte)ReadNumberPropertyIfThere(ref reader, nameof(Squad.VeterancyRank), 0));
                unitBuilder.SetVeterancyExperience((float)ReadAccurateNumberPropertyIfThere(ref reader, nameof(Squad.VeterancyProgress), 0));

                // Read if crew
                unitBuilder.SetIsCrew(ReadBooleanPropertyIfThere(ref reader, nameof(Squad.IsCrew), false));

                // Get crew if there
                Squad crew = ReadPropertyThroughSerialisationIfThere<Squad>(ref reader, nameof(Squad.Crew), null);
                if (crew is not null) {
                    unitBuilder.SetCrew(crew, false);
                    reader.Read(); // goto next object
                }

                // Get upgrades
                if (reader.TokenType is not JsonTokenType.EndObject && reader.GetString() is nameof(Squad.Upgrades) && reader.Read()) {
                    unitBuilder.AddUpgrade(reader.GetStringArray());
                    reader.Read();
                }

                // Get upgrades
                if (reader.TokenType is not JsonTokenType.EndObject && reader.GetString() is nameof(Squad.SlotItems) && reader.Read()) {
                    unitBuilder.AddSlotItem(reader.GetStringArray());
                    reader.Read();
                }

                // Get upgrades
                if (reader.TokenType is not JsonTokenType.EndObject && reader.GetString() is nameof(Squad.Modifiers) && reader.Read()) {
                    throw new NotImplementedException();
                }

                // Get squad
                var squad = unitBuilder.Build(squadID);
                squad.CalculateChecksum();
                if (squad.VerifyChecksum(checksum)) {
                    return squad;
                } else {
                    throw new ChecksumViolationException();
                }

            }

            private static bool ReadBooleanPropertyIfThere(ref Utf8JsonReader reader, string property, bool defaultValue) {
                if (reader.TokenType is not JsonTokenType.EndObject && reader.GetString() == property) {
                    reader.Read();
                    return reader.ReadBoolProperty();
                } else {
                    return defaultValue;
                }
            }

            private static string ReadStringPropertyIfThere(ref Utf8JsonReader reader, string property, string defaultValue) {
                if (reader.TokenType is not JsonTokenType.EndObject && reader.GetString() == property) {
                    reader.Read();
                    return reader.ReadProperty();
                } else {
                    return defaultValue;
                }
            }

            private static string ReadStringProperty(ref Utf8JsonReader reader, string property) {
                if (reader.GetString() == property && reader.Read()) {
                    return reader.ReadProperty();
                } else {
                    return null;
                }
            }

            private static float ReadNumberPropertyIfThere(ref Utf8JsonReader reader, string property, float defaultValue) {
                if (reader.TokenType is not JsonTokenType.EndObject && reader.GetString() == property) {
                    reader.Read();
                    return reader.ReadNumberProperty();
                } else {
                    return defaultValue;
                }
            }

            private static double ReadAccurateNumberPropertyIfThere(ref Utf8JsonReader reader, string property, double defaultValue) {
                if (reader.TokenType is not JsonTokenType.EndObject && reader.GetString() == property) {
                    reader.Read();
                    return reader.ReadAccurateNumberProperty();
                } else {
                    return defaultValue;
                }
            }

            private static float ReadNumberProperty(ref Utf8JsonReader reader, string property) {
                if (reader.GetString() == property && reader.Read()) {
                    return reader.ReadNumberProperty();
                } else {
                    return float.NaN;
                }
            }

            public static T ReadPropertyThroughSerialisationIfThere<T>(ref Utf8JsonReader reader, string property, T defaultValue) {
                if (reader.TokenType is not JsonTokenType.EndObject && reader.GetString() == property) {
                    reader.Read();
                    return JsonSerializer.Deserialize<T>(ref reader);
                } else {
                    return defaultValue;
                }
            }

            public override void Write(Utf8JsonWriter writer, Squad value, JsonSerializerOptions options) {

                // Calculate checksum
                value.CalculateChecksum();

                // Start squad object
                writer.WriteStartObject();

                // Write data
                writer.WriteNumber(nameof(Squad.SquadID), value.SquadID);
                writer.WriteString(nameof(Squad.Checksum), value.Checksum.ToString("X8"));
                writer.WriteString(nameof(Squad.SBP), value.SBP.Name);
                if (value.SBP.PBGID.Mod != ModGuid.BaseGame) {
                    writer.WriteString(nameof(Squad.SBP.PBGID.Mod), value.SBP.PBGID.Mod.GUID);
                }

                // Write custom name (if any)
                if (!string.IsNullOrEmpty(value.CustomName)) {
                    writer.WriteString(nameof(Squad.CustomName), value.CustomName);
                }

                // Write deployment phase (if not none)
                if (value.DeploymentPhase is not DeploymentPhase.PhaseNone) {
                    writer.WriteString(nameof(Squad.DeploymentPhase), value.DeploymentPhase.ToString());
                }

                // Write combat time
                if (value.CombatTime.TotalSeconds > 0) {
                    writer.WriteString(nameof(Squad.CombatTime), value.CombatTime.ToString());
                }

                // Write deployment method
                if (value.DeploymentMethod is not DeploymentMethod.None) {
                    writer.WriteString(nameof(Squad.SupportBlueprint), value.SupportBlueprint.Name);
                    writer.WriteString(nameof(Squad.DeploymentMethod), value.DeploymentMethod.ToString());
                }

                // Write rank if there
                if (value.VeterancyRank > 0) {
                    writer.WriteNumber(nameof(Squad.VeterancyRank), value.VeterancyRank);
                }

                // Write experience if there
                if (value.VeterancyProgress > 0) {
                    writer.WriteNumber(nameof(Squad.VeterancyProgress), value.VeterancyProgress);
                }

                // If crew
                if (value.IsCrew) {
                    writer.WriteBoolean(nameof(Squad.IsCrew), value.IsCrew);
                }

                // If crew
                if (value.Crew is not null) {
                    writer.WritePropertyName(nameof(Squad.Crew));
                    JsonSerializer.Serialize(writer, value.Crew, options);
                }

                // Write upgrades (if any)
                if (value.Upgrades.Count > 0) {
                    writer.WritePropertyName(nameof(Squad.Upgrades));
                    writer.WriteStartArray();
                    foreach (Blueprint item in value.Upgrades) {
                        writer.WriteStringValue(item.Name);
                    }
                    writer.WriteEndArray();
                }

                // Write slot items (if any)
                if (value.SlotItems.Count > 0) {
                    writer.WritePropertyName(nameof(Squad.SlotItems));
                    writer.WriteStartArray();
                    foreach (Blueprint item in value.SlotItems) {
                        writer.WriteStringValue(item.Name);
                    }
                    writer.WriteEndArray();
                }

                // Write modifiers (if any)
                if (value.Modifiers.Count > 0) {
                    writer.WritePropertyName(nameof(Squad.Modifiers));
                    writer.WriteStartArray();
                    foreach (Modifier item in value.Modifiers) {
                        JsonSerializer.Serialize(writer, item, options);
                    }
                    writer.WriteEndArray();
                }

                // Start end object
                writer.WriteEndObject();

            }

        }

    }

}
