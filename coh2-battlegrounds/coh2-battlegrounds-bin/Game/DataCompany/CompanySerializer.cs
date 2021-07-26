using System;
using System.Text.Json;
using System.Text.Json.Serialization;

using Battlegrounds.Functional;
using Battlegrounds.Game.Gameplay;
using Battlegrounds.Modding;

namespace Battlegrounds.Game.DataCompany {

    public class CompanySerializer : JsonConverter<Company> {

        public override Company Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {

            // Open object
            reader.Read();

            // Read initial important data
            string name = ReadProperty(ref reader, nameof(Company.Name));
            string army = ReadProperty(ref reader, nameof(Company.Army));

            // Create builder instance
            CompanyBuilder builder = new CompanyBuilder()
                .NewCompany(Faction.FromName(army))
                .ChangeName(name)
                .ChangeTuningMod(ModGuid.FromGuid(ReadProperty(ref reader, nameof(Company.TuningGUID))))
                .ChangeAppVersion(ReadProperty(ref reader, nameof(Company.AppVersion)));

            // Read checksum
            string checksum = ReadProperty(ref reader, nameof(Company.Checksum));

            // Commit
            builder.Commit();

            // TODO: Verify checksum

            // Return result
            return builder.Result;

        }

        private static string ReadProperty(ref Utf8JsonReader reader, string property) {
            if (reader.GetString() == property && reader.Read()) {
                return reader.ReadProperty();
            } else {
                return string.Empty;
            }
        }

        public override void Write(Utf8JsonWriter writer, Company value, JsonSerializerOptions options) {

            // Begin object
            writer.WriteStartObject();

            // Write all the data
            writer.WriteString(nameof(Company.Name), value.Name);
            writer.WriteString(nameof(Company.Army), value.Army.Name);
            writer.WriteString(nameof(Company.TuningGUID), value.TuningGUID.GUID);
            writer.WriteString(nameof(Company.AppVersion), value.AppVersion);
            writer.WriteString(nameof(Company.Checksum), value.Checksum);
            writer.WriteString(nameof(Company.Type), value.Type.ToString());
            writer.WriteString(nameof(Company.AvailabilityType), value.AvailabilityType.ToString());

            // Write statistics
            writer.WritePropertyName(nameof(Company.Statistics));
            JsonSerializer.Serialize(writer, value.Statistics);

            // Write Units
            writer.WritePropertyName(nameof(Company.Units));
            JsonSerializer.Serialize(writer, value.Units);

            // Write abilities
            if (value.Abilities.Length > 0) {
                writer.WritePropertyName(nameof(Company.Abilities));
                JsonSerializer.Serialize(writer, value.Abilities);
            }

            // Write inventory
            if (value.Inventory.Length > 0) {
                writer.WritePropertyName(nameof(Company.Inventory));
                JsonSerializer.Serialize(writer, value.Inventory);
            }

            // Write upgrades
            if (value.Upgrades.Length > 0) {
                writer.WritePropertyName(nameof(Company.Upgrades));
                JsonSerializer.Serialize(writer, value.Upgrades);
            }

            // Write modifiers
            if (value.Modifiers.Length > 0) {
                writer.WritePropertyName(nameof(Company.Modifiers));
                JsonSerializer.Serialize(writer, value.Modifiers);
            }

            // Close company object
            writer.WriteEndObject();

        }

        public static string GetCompanyAsJson(Company company, bool indent = true) 
            => JsonSerializer.Serialize(company, new JsonSerializerOptions() { WriteIndented = indent });

        public static Company GetCompanyFromJson(string rawJsonData)
            => JsonSerializer.Deserialize<Company>(rawJsonData);

    }

}
