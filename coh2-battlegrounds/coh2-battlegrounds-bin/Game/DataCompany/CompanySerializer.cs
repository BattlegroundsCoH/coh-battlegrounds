using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

using Battlegrounds.Functional;
using Battlegrounds.Game.Database;
using Battlegrounds.Game.Gameplay;
using Battlegrounds.Modding;
using Battlegrounds.Verification;

namespace Battlegrounds.Game.DataCompany {

    /// <summary>
    /// Class for serializing a <see cref="Company"/> to and from json.
    /// </summary>
    public class CompanySerializer : JsonConverter<Company> {

        public override Company Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {

            // Open object
            reader.Read();

            // Read company name (if not there, return null)
            string name = ReadProperty(ref reader, nameof(Company.Name));
            if (name is null) {
                return null;
            }

            // Read company army (if not there, return null)
            string army = ReadProperty(ref reader, nameof(Company.Army));
            if (army is null) {
                return null;
            }

            // Read mod GUID and BG App version
            var guid = ModGuid.FromGuid(ReadProperty(ref reader, nameof(Company.TuningGUID)));
            var version = ReadProperty(ref reader, nameof(Company.AppVersion));

            // Read checksum
            ulong checksum = ReadChecksum(ref reader, nameof(Company.Checksum));

            // Read type data
            var type = Enum.Parse<CompanyType>(ReadProperty(ref reader, nameof(Company.Type)));
            var availability = Enum.Parse<CompanyAvailabilityType>(ReadProperty(ref reader, nameof(Company.AvailabilityType)));

            // Create builder instance
            CompanyBuilder builder = CompanyBuilder.NewCompany(name, type, availability, Faction.FromName(army), guid);
            builder.Statistics = ReadPropertyThroughSerialisation<CompanyStatistics>(ref reader, nameof(Company.Statistics));

            // Create helper dictionary
            var arrayTypes = new Dictionary<string, Type>() {
                [nameof(Company.Abilities)] = typeof(Ability[]),
                [nameof(Company.Units)] = typeof(Squad[]),
                [nameof(Company.Upgrades)] = typeof(UpgradeBlueprint[]),
                [nameof(Company.Modifiers)] = typeof(Modifier[]),
                [nameof(Company.Inventory)] = typeof(Blueprint[]),
            };

            // Read arrays
            while (reader.Read() && reader.TokenType is not JsonTokenType.EndObject) {

                // Read property
                string property = reader.ReadProperty();
                var inputType = arrayTypes[property];

                // Get data and set it
                Array values = JsonSerializer.Deserialize(ref reader, inputType) as Array;
                switch (property) {
                    case nameof(Company.Units):
                        for (int i = 0; i < values.Length; i++) {
                            builder.AddUnit(new UnitBuilder(values.GetValue(i) as Squad, overrideIndex: true));
                        }
                        break;
                    case nameof(Company.Abilities):
                        for (int i = 0; i < values.Length; i++) {
                            builder.AddAbility(values.GetValue(i) as Ability);
                        }
                        break;
                    default:
                        //throw new NotImplementedException();
                        break;
                }

            }

            // Commit
            builder.Commit();

            // Verify checksum and return if success; otherwise throw checksum violation error
            Company result = builder.Result;
            result.CalculateChecksum();
            if (!result.VerifyChecksum(checksum)) {
                Trace.WriteLine($"Warning - Company '{result.Name}' has been modified.", nameof(CompanySerializer));
            }
            return result;
            //return result.VerifyChecksum(checksum) ? result : throw new ChecksumViolationException(result.Checksum, checksum);

        }

        private static string ReadProperty(ref Utf8JsonReader reader, string property)
            => reader.GetString() == property && reader.Read() ? reader.ReadProperty() : null;

        private static ulong ReadChecksum(ref Utf8JsonReader reader, string property)
            => reader.GetString() == property && reader.Read() ? reader.ReadUlongProperty() : 0;

        private static T ReadPropertyThroughSerialisation<T>(ref Utf8JsonReader reader, string property)
            => reader.GetString() == property && reader.Read() ? JsonSerializer.Deserialize<T>(ref reader) : default;

        public override void Write(Utf8JsonWriter writer, Company value, JsonSerializerOptions options) {

            // Recalculate the checksum
            value.CalculateChecksum();

            // Begin object
            writer.WriteStartObject();

            // Write all the data
            writer.WriteString(nameof(Company.Name), value.Name);
            writer.WriteString(nameof(Company.Army), value.Army.Name);
            writer.WriteString(nameof(Company.TuningGUID), value.TuningGUID.GUID);
            writer.WriteString(nameof(Company.AppVersion), value.AppVersion);
            writer.WriteNumber(nameof(Company.Checksum), value.Checksum);
            writer.WriteString(nameof(Company.Type), value.Type.ToString());
            writer.WriteString(nameof(Company.AvailabilityType), value.AvailabilityType.ToString());

            // Write statistics
            writer.WritePropertyName(nameof(Company.Statistics));
            JsonSerializer.Serialize(writer, value.Statistics, options);

            // Write Units
            writer.WritePropertyName(nameof(Company.Units));
            JsonSerializer.Serialize(writer, value.Units, options);

            // Write abilities
            writer.WritePropertyName(nameof(Company.Abilities));
            JsonSerializer.Serialize(writer, value.Abilities, options);

            // Write inventory
            writer.WritePropertyName(nameof(Company.Inventory));
            JsonSerializer.Serialize(writer, value.Inventory, options);

            // Write upgrades
            writer.WritePropertyName(nameof(Company.Upgrades));
            JsonSerializer.Serialize(writer, value.Upgrades, options);

            // Write modifiers
            writer.WritePropertyName(nameof(Company.Modifiers));
            JsonSerializer.Serialize(writer, value.Modifiers, options);

            // Close company object
            writer.WriteEndObject();

        }

        /// <summary>
        /// Get a company in its json format.
        /// </summary>
        /// <param name="company">The company to serialise.</param>
        /// <param name="indent">Should the json be indent formatted.</param>
        /// <returns>The json string data.</returns>
        public static string GetCompanyAsJson(Company company, bool indent = true)
            => JsonSerializer.Serialize(company, new JsonSerializerOptions() { WriteIndented = indent });

        /// <summary>
        /// Get a company from raw json data.
        /// </summary>
        /// <param name="rawJsonData">The raw json data to parse.</param>
        /// <returns>The company built from the <paramref name="rawJsonData"/>. Will be <see langword="null"/> if deserialization fails.</returns>
        public static Company GetCompanyFromJson(string rawJsonData)
            => JsonSerializer.Deserialize<Company>(rawJsonData);

    }

}
