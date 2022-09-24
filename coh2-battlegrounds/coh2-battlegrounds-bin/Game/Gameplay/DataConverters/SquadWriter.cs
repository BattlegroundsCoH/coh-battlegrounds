using System;
using System.Linq;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Text.Json;

using Battlegrounds.Game.Database;
using Battlegrounds.Game.DataCompany;

using Battlegrounds.Modding;

using Battlegrounds.Functional;

using Battlegrounds.Lua.Generator.RuntimeServices;
using Battlegrounds.Lua.Generator;
using System.Globalization;
using Battlegrounds.Game.Database.Management;
using Battlegrounds.Modding.Content.Companies;

namespace Battlegrounds.Game.Gameplay.DataConverters;

/// <summary>
/// Static utility class containing different writers for different objects
/// </summary>
public static class SquadWriter {

    /// <summary>
    /// Class for generating a Battlegrounds representation of a squad.
    /// </summary>
    public class SquadLua : LuaConverter<Squad> {

        public override void Write(LuaSourceBuilder luaSourceBuilder, Squad value) {

            // Grab cost
            var cost = luaSourceBuilder.Context switch {
                FactionCompanyType ft => 
                    ft.GetUnitCost(value.SBP, value.Upgrades.ToArray().MapNotNull(x => x as UpgradeBlueprint), value.VeterancyRank, value.DeploymentRole, value.SupportBlueprint as SquadBlueprint),
                _ => value.SBP.Cost
            };

            // Get base data
            Dictionary<string, object> data = new() {
                ["bp_name"] = value.SBP.GetScarName(),
                ["company_id"] = value.SquadID,
                ["symbol"] = value.SBP.UI.Symbol,
                ["category"] = value.GetCategory(true),
                ["veterancy_rank"] = value.VeterancyRank,
                ["veterancy_progress"] = value.VeterancyProgress,
                ["upgrades"] = value.Upgrades.Cast<UpgradeBlueprint>().Select(GetBlueprintWithSymbol),
                ["slot_items"] = value.SlotItems.Cast<SlotItemBlueprint>().Select(GetBlueprintWithSymbol),
                ["modifiers"] = value.Modifiers,
                ["spawned"] = false,
                ["cost"] = cost
            };

            // Write support if any
            if (value.SupportBlueprint is not null) {
                data["transport"] = GetSupportBlueprint(value);
            }

            // Write custom name if any
            if (!string.IsNullOrEmpty(value.CustomName)) {
                data["name"] = value.CustomName;
            }

            // Write crew if any
            if (value.Crew is Squad crew) {
                data["crew"] = new Dictionary<string, object>() {
                    ["bp_name"] = crew.SBP.GetScarName(),
                    ["company_id"] = crew.SquadID,
                    ["symbol"] = crew.SBP.UI.Symbol,
                    ["veterancy_rank"] = crew.VeterancyRank,
                    ["veterancy_progress"] = crew.VeterancyProgress,
                    ["upgrades"] = crew.Upgrades.Cast<UpgradeBlueprint>().Select(GetBlueprintWithSymbol),
                    ["slot_items"] = crew.SlotItems.Cast<SlotItemBlueprint>().Select(GetBlueprintWithSymbol),
                    ["modifiers"] = crew.Modifiers,
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
            var support = value.SupportBlueprint as SquadBlueprint ?? throw new Exception("Invalid squad support blueprint.");
            Dictionary<string, object> data = new() {
                ["sbp"] = support.GetScarName(),
                ["symbol"] = support.UI.Symbol,
                ["mode"] = (byte)value.DeploymentMethod,
            };
            if (value.SBP.Types.IsHeavyArtillery || (!value.SBP.Types.IsHeavyArtillery && value.SBP.Types.IsAntiTank)) {
                data["tow"] = true;
            }
            return data;
        }

        private static Dictionary<string, object> GetBlueprintWithSymbol<T>(T upg) where T : Blueprint, IUIBlueprint {
            return new() {
                ["bp"] = upg.GetScarName(),
                ["symbol"] = upg.UI.Symbol,
            };
        }

    }

    /// <summary>
    /// Class for constructing a <see cref="Squad"/> from json data.
    /// </summary>
    public class SquadJson : JsonConverter<Squad> {

        public override Squad Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {

            // Read open object
            reader.Read();

            // Read squad ID
            ushort squadID = (ushort)ReadNumberProperty(ref reader, nameof(Squad.SquadID));

            // Read blueprint
            string sbpName = ReadStringProperty(ref reader, nameof(Squad.SBP));
            ModGuid modGuid = ModGuid.BaseGame;

            // Read 
            if (reader.GetString() is nameof(Squad.SBP.PBGID.Mod)) {
                modGuid = ModGuid.FromGuid(ReadStringProperty(ref reader, nameof(Squad.SBP.PBGID.Mod)));
            }

            // Set mod guid and get deployment phase and combat time
            var unitBuilder = UnitBuilder.NewUnit(sbpName, modGuid);
            unitBuilder.SetCustomName(ReadStringPropertyIfThere(ref reader, nameof(Squad.CustomName), string.Empty));
            unitBuilder.SetDeploymentPhase(Enum.Parse<DeploymentPhase>(ReadStringPropertyIfThere(ref reader, nameof(Squad.DeploymentPhase), nameof(DeploymentPhase.PhaseNone))));
            unitBuilder.SetDeploymentRole(Enum.Parse<DeploymentRole>(ReadStringPropertyIfThere(ref reader, nameof(Squad.DeploymentRole), nameof(DeploymentRole.ReserveRole))));
            unitBuilder.SetCombatTime(TimeSpan.Parse(ReadStringPropertyIfThere(ref reader, nameof(Squad.CombatTime), TimeSpan.Zero.ToString()), CultureInfo.InvariantCulture));

            // Get deployment method
            string supportBP = ReadStringPropertyIfThere(ref reader, nameof(Squad.SupportBlueprint), string.Empty);
            if (!string.IsNullOrEmpty(supportBP)) {
                unitBuilder.SetTransportBlueprint(supportBP);
            }
            unitBuilder.SetDeploymentMethod(Enum.Parse<DeploymentMethod>(ReadStringPropertyIfThere(ref reader, nameof(Squad.DeploymentMethod), nameof(DeploymentMethod.None))));

            // Get veterancy
            unitBuilder.SetVeterancyRank((byte)ReadNumberPropertyIfThere(ref reader, nameof(Squad.VeterancyRank), 0));
            unitBuilder.SetVeterancyExperience((float)ReadAccurateNumberPropertyIfThere(ref reader, nameof(Squad.VeterancyProgress), 0));

            // Get sync weapon if there
            var syncBp = ReadStringPropertyIfThere(ref reader, nameof(Squad.SyncWeapon), string.Empty);
            if (!string.IsNullOrEmpty(syncBp)) {
                unitBuilder.SetSyncWeapon(BlueprintManager.FromBlueprintName<EntityBlueprint>(syncBp));
                reader.Read(); // goto next object
            }

            // Get crew if there
            Squad? crew = ReadPropertyThroughSerialisationIfThere<Squad>(ref reader, nameof(Squad.Crew), null);
            if (crew is not null) {
                unitBuilder.SetCrew(crew);
                reader.Read(); // goto next object
            }

            // Get upgrades
            if (reader.TokenType is not JsonTokenType.EndObject && reader.GetString() is nameof(Squad.Upgrades) && reader.Read()) {
                unitBuilder.AddUpgrade(reader.GetStringArray().NotNull());
                reader.Read();
            }

            // Get upgrades
            if (reader.TokenType is not JsonTokenType.EndObject && reader.GetString() is nameof(Squad.SlotItems) && reader.Read()) {
                unitBuilder.AddSlotItem(reader.GetStringArray().NotNull());
                reader.Read();
            }

            // Get upgrades
            if (reader.TokenType is not JsonTokenType.EndObject && reader.GetString() is nameof(Squad.Modifiers) && reader.Read()) {
                throw new NotImplementedException();
            }

            // Get squad
            return unitBuilder.Commit(squadID).Result;

        }

        private static string ReadStringPropertyIfThere(ref Utf8JsonReader reader, string property, string defaultValue) {
            if (reader.TokenType is not JsonTokenType.EndObject && reader.GetString() == property) {
                reader.Read();
                return reader.ReadProperty() ?? string.Empty;
            } else {
                return defaultValue;
            }
        }

        private static string ReadStringProperty(ref Utf8JsonReader reader, string property) {
            if (reader.GetString() == property && reader.Read()) {
                return reader.ReadProperty() ?? string.Empty;
            } else {
                return string.Empty;
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

        public static T? ReadPropertyThroughSerialisationIfThere<T>(ref Utf8JsonReader reader, string property, T? defaultValue) {
            if (reader.TokenType is not JsonTokenType.EndObject && reader.GetString() == property) {
                reader.Read();
                return JsonSerializer.Deserialize<T>(ref reader);
            } else {
                return defaultValue;
            }
        }

        public override void Write(Utf8JsonWriter writer, Squad value, JsonSerializerOptions options) {

            // Start squad object
            writer.WriteStartObject();

            // Write data
            writer.WriteNumber(nameof(Squad.SquadID), value.SquadID);
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

            // Write deployment role
            writer.WriteString(nameof(Squad.DeploymentRole), value.DeploymentRole.ToString());

            // Write combat time
            if (value.CombatTime.TotalSeconds > 0) {
                writer.WriteString(nameof(Squad.CombatTime), value.CombatTime.ToString("c", CultureInfo.InvariantCulture));
            }

            // Write deployment method
            if (value.DeploymentMethod is not DeploymentMethod.None) {
                writer.WriteString(nameof(Squad.SupportBlueprint), value.SupportBlueprint?.Name);
                writer.WriteString(nameof(Squad.DeploymentMethod), value.DeploymentMethod.ToString());
            }

            // Write rank if there
            if (value.VeterancyRank > 0) {
                writer.WriteNumber(nameof(Squad.VeterancyRank), value.VeterancyRank);
            }

            // Write experience if there
            if (value.VeterancyProgress != 0) {
                writer.WriteNumber(nameof(Squad.VeterancyProgress), value.VeterancyProgress);
            }

            // If sync weapon
            if (value.SyncWeapon is not null)
                writer.WriteString(nameof(Squad.SyncWeapon), value.SyncWeapon.GetScarName());

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
