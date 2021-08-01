using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

using Battlegrounds.Functional;
using Battlegrounds.Game.Database.Extensions;
using Battlegrounds.Game.Database.Management;
using Battlegrounds.Modding;

namespace Battlegrounds.Game.Database {

    /// <summary>
    /// Representation of a <see cref="Blueprint"/> with upgrade specific values. Inherits from <see cref="Blueprint"/>. This class cannot be inheritted.
    /// </summary>
    [JsonConverter(typeof(UpgradeBlueprintConverter))]
    public class UpgradeBlueprint : Blueprint {

        public enum OwnerType {
            None,
            Self,
            Player,
            EntityInSquad
        }

        /// <summary>
        /// The unique PropertyBagGroupdID assigned to this blueprint.
        /// </summary>
        public override BlueprintUID PBGID { get; }

        public override BlueprintType BlueprintType => BlueprintType.UBP;

        public override string Name { get; }

        /// <summary>
        /// Get the <see cref="OwnerType"/> for the upgrade.
        /// </summary>
        public OwnerType OwnershipType { get; }

        /// <summary>
        /// Get the UI extension of the upgrade
        /// </summary>
        public UIExtension UI { get; }

        /// <summary>
        /// Get the cost extension of the ability.
        /// </summary>
        public CostExtension Cost { get; }

        /// <summary>
        /// The names of the granted <see cref="SlotItemBlueprint"/> by the <see cref="UpgradeBlueprint"/>.
        /// </summary>
        public HashSet<string> SlotItems { get; set; }

        /// <summary>
        /// Get the requirements for getting the upgrade.
        /// </summary>
        public RequirementExtension[] Requirements { get; }

        /// <summary>
        /// New <see cref="UpgradeBlueprint"/> instance. This should only ever be used by the database loader!
        /// </summary>
        public UpgradeBlueprint(string name, BlueprintUID pbgid, OwnerType ownertype, UIExtension ui, CostExtension cost, RequirementExtension[] requirements, string[] slotItems) : base() {
            this.Name = name;
            this.PBGID = pbgid;
            this.OwnershipType = ownertype;
            this.UI = ui;
            this.Cost = cost;
            this.SlotItems = new(slotItems);
            this.Requirements = requirements;
        }

    }

    public class UpgradeBlueprintConverter : JsonConverter<UpgradeBlueprint> {

        public override UpgradeBlueprint Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
            Dictionary<string, object> __lookup = new();
            while (reader.Read() && reader.TokenType is not JsonTokenType.EndObject) {
                string prop = reader.ReadProperty();
                __lookup[prop] = prop switch {
                    "Cost" => CostExtension.FromJson(ref reader),
                    "Display" => UIExtension.FromJson(ref reader),
                    "PBGID" => reader.GetUInt64(),
                    "Name" => reader.GetString(),
                    "ModGUID" => reader.GetString(),
                    "SlotItems" => reader.GetStringArray(),
                    "OwnerType" => reader.GetString() switch {
                        "self" => UpgradeBlueprint.OwnerType.Self,
                        "player" => UpgradeBlueprint.OwnerType.Player,
                        "entity_in_squad" => UpgradeBlueprint.OwnerType.EntityInSquad,
                        _ => UpgradeBlueprint.OwnerType.None,
                    },
                    "Requirements" => JsonSerializer.Deserialize<RequirementExtension[]>(ref reader, options),
                    _ => throw new NotImplementedException(prop)
                };
            }
            ModGuid modguid = __lookup.ContainsKey("ModGUID") ? ModGuid.FromGuid(__lookup["ModGUID"] as string) : ModGuid.BaseGame;
            BlueprintUID pbgid = new BlueprintUID(__lookup.GetValueOrDefault("PBGID", 0ul), modguid);
            return new(__lookup.GetValueOrDefault("Name", string.Empty), pbgid,
                __lookup.GetValueOrDefault("OwnerType", UpgradeBlueprint.OwnerType.None),
                __lookup.GetValueOrDefault("Display", new UIExtension()),
                __lookup.GetValueOrDefault("Cost", new CostExtension()),
                __lookup.GetValueOrDefault("Requirements", Array.Empty<RequirementExtension>()),
                __lookup.GetValueOrDefault("SlotItems", Array.Empty<string>()));
        }

        public override void Write(Utf8JsonWriter writer, UpgradeBlueprint value, JsonSerializerOptions options) => writer.WriteStringValue(value.Name);

    }

}
