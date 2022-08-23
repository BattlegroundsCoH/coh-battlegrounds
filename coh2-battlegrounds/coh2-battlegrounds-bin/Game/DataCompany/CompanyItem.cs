using System;
using System.Text.Json;
using System.Text.Json.Serialization;

using Battlegrounds.Functional;
using Battlegrounds.Game.Database;
using Battlegrounds.Game.Database.Management;

namespace Battlegrounds.Game.DataCompany;

[JsonConverter(typeof(CompanyItemSerialiser))]
public class CompanyItem {

    public class CompanyItemSerialiser : JsonConverter<CompanyItem> {
        private static string ReadProperty(ref Utf8JsonReader reader, string property)
            => reader.GetString() == property && reader.Read() ? (reader.ReadProperty() ?? string.Empty) : string.Empty;
        private static uint ReadUintProperty(ref Utf8JsonReader reader, string property)
            => reader.GetString() == property && reader.Read() ? reader.ReadUintProperty() : 0;
        private static bool ReadBoolProperty(ref Utf8JsonReader reader, string property)
            => reader.GetString() == property && reader.Read() ? reader.ReadBoolProperty() : false;

        public override CompanyItem? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {

            // Open
            reader.Read();

            // Get id
            var itemId = ReadUintProperty(ref reader, nameof(ItemId));

            // Get blueprint type
            var bpTyp = Enum.Parse<BlueprintType>(ReadProperty(ref reader, nameof(Blueprint.BlueprintType)));

            // Get if 
            var isVeh = ReadBoolProperty(ref reader, nameof(IsVehicle));

            // Get bp name
            var bpNam = ReadProperty(ref reader, nameof(Item));

            // End
            reader.Read();

            // Return
            return new(itemId, BlueprintManager.FromBlueprintName(bpNam, bpTyp), isVeh);

        }

        public override void Write(Utf8JsonWriter writer, CompanyItem value, JsonSerializerOptions options) {
            
            // Open object
            writer.WriteStartObject();

            // Write item id
            writer.WriteNumber(nameof(ItemId), value.ItemId);

            // Write blueprint type
            writer.WriteString(nameof(Blueprint.BlueprintType), value.Item.BlueprintType.ToString());

            // Write team weapon
            writer.WriteBoolean(nameof(IsVehicle), value.IsVehicle);

            // Write blueprint name
            writer.WriteString(nameof(Item), value.Item.GetScarName());

            // Close object
            writer.WriteEndObject();

        }
    }

    public uint ItemId { get; }

    public Blueprint Item { get; }

    public bool IsVehicle { get; }

    public bool IsEntity => this.Item is EntityBlueprint;

    public bool IsSlotItem => this.Item is SlotItemBlueprint;

    public CompanyItem(uint id, Blueprint blueprint, bool isVehicle) {
        this.ItemId = id;
        this.Item = blueprint;
        this.IsVehicle = isVehicle;
    }

}
