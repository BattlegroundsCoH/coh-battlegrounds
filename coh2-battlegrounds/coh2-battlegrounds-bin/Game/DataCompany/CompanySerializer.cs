using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

using Battlegrounds.ErrorHandling.CommonExceptions;
using Battlegrounds.Functional;
using Battlegrounds.Game.Database;
using Battlegrounds.Game.Gameplay;
using Battlegrounds.Modding;
using Battlegrounds.Verification;

namespace Battlegrounds.Game.DataCompany;

/// <summary>
/// Class for serializing a <see cref="Company"/> to and from json. Extends <see cref="JsonConverter{T}"/>.
/// </summary>
public class CompanySerializer : JsonConverter<Company> {

    public override Company? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
        var newOptions = new JsonSerializerOptions(options);
        newOptions.Converters.Add(new UnverifiedCompanyJson());
        var unverified = JsonSerializer.Deserialize<UnverifiedCompany>(ref reader, newOptions);
        if (unverified.Success) {
            return unverified.Company;
        } else {
            Trace.WriteLine("Failed to read company json...", nameof(CompanySerializer));
            return null;
        }
    }

    private static string ReadProperty(ref Utf8JsonReader reader, string property)
        => reader.GetString() == property && reader.Read() ? (reader.ReadProperty() ?? string.Empty) : string.Empty;

    private static ulong ReadChecksum(ref Utf8JsonReader reader, string property)
        => reader.GetString() == property && reader.Read() ? reader.ReadUlongProperty() : 0;

    private static T? ReadPropertyThroughSerialisation<T>(ref Utf8JsonReader reader, string property)
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
        writer.WriteString(nameof(Company.Type), value.Type.Id);
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
    /// 
    /// </summary>
    /// <param name="company"></param>
    /// <param name="filepath"></param>
    public static void SaveCompanyToFile(Company company, string filepath) {
        
        // Serialise once -> will possibly contain faulty checksum
        var json1 = JsonSerializer.Serialize(company, new JsonSerializerOptions());
        
        // Deserialise -> read in with checksum OK values (again, we need to do this because of rounding errors and such)
        Company? delta = GetCompanyFromJson(json1);
        if (delta is null) {
            throw new Exception("Failed to save company file since intermeddiate save step failed!");
        }
        
        // Save finally -> delta should now be checksum safe
        File.WriteAllText(filepath, JsonSerializer.Serialize(delta, new JsonSerializerOptions() { WriteIndented = true }), Encoding.UTF8);

    }

    /// <summary>
    /// Get a company from raw json data.
    /// </summary>
    /// <param name="rawJsonData">The raw json data to parse.</param>
    /// <returns>The company built from the <paramref name="rawJsonData"/>. Will be <see langword="null"/> if deserialization fails.</returns>
    public static Company? GetCompanyFromJson(string rawJsonData)
        => JsonSerializer.Deserialize<Company>(rawJsonData); // this will not test if verified

    /// <summary>
    /// 
    /// </summary>
    /// <param name="filepath"></param>
    /// <param name="checkChecksum"></param>
    /// <returns></returns>
    public static Company? GetCompanyFromFile(string filepath, bool checkChecksum = true) {
        
        // Make sure file exists
        if (!File.Exists(filepath)) {
            return null;
        }

        // Open file stream
        using FileStream fs = File.OpenRead(filepath);

        // Deserialise
        var options = new JsonSerializerOptions();
        options.Converters.Add( new UnverifiedCompanyJson());
        var unverified = JsonSerializer.Deserialize<UnverifiedCompany>(fs, options: options);
        if (unverified.Success) {

            // Grab result
            Company result = unverified.Company;

            // Verify checksum
            if (checkChecksum && !result.VerifyChecksum(unverified.Checksum)) {
                string dbstr =
#if DEBUG
                    $" (0x{unverified.Checksum:X} - 0x{result.Checksum:X})";
#else
                    "";
#endif
                Trace.WriteLine($"Fatal - Company '{result.Name}' has been modified{dbstr}.", $"{nameof(CompanySerializer)}::{nameof(GetCompanyFromFile)}");
                
                throw new ChecksumViolationException(0, unverified.Checksum);
            }

            // Return company
            return result;

        } else {

            // Log error
            Trace.WriteLine("Failed to read company file...", $"{nameof(CompanySerializer)}::{nameof(GetCompanyFromFile)}");
            return null;

        }

    }

    public readonly struct UnverifiedCompany { 
        
        public ulong Checksum { get; }
        
        public Company? Company { get; }

        [MemberNotNullWhen(true, nameof(Company))]
        public bool Success => this.Company is not null;

        public UnverifiedCompany(ulong checksum, Company? company) {
            this.Checksum = checksum;
            this.Company = company;
        }

    }

    public class UnverifiedCompanyJson : JsonConverter<UnverifiedCompany> {
        public override UnverifiedCompany Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {

            // Open object
            reader.Read();

            // Read company name (if not there, return null)
            string name = ReadProperty(ref reader, nameof(Company.Name));
            if (string.IsNullOrEmpty(name)) {
                return new(0, null);
            }

            // Read company army (if not there, return null)
            string army = ReadProperty(ref reader, nameof(Company.Army));
            if (string.IsNullOrEmpty(army)) {
                return new(0, null);
            }

            // Get proper faction
            var faction = Faction.FromName(army);

            // Read mod GUID and BG App version
            var guid = ModGuid.FromGuid(ReadProperty(ref reader, nameof(Company.TuningGUID)));
            var version = ReadProperty(ref reader, nameof(Company.AppVersion));

            // Verify package
            if (ModManager.GetPackageFromGuid(guid) is not ModPackage companyModPackage) {
                Trace.WriteLine($"Failed to find mod package for tuning mod '{guid}'.", nameof(CompanySerializer));
                return new(0, null);
            }

            // Read checksum
            ulong checksum = ReadChecksum(ref reader, nameof(Company.Checksum));

            // Read type data
            string typeStr = ReadProperty(ref reader, nameof(Company.Type));
            var type = companyModPackage.GetCompanyType(faction, typeStr);
            var availability = Enum.Parse<CompanyAvailabilityType>(ReadProperty(ref reader, nameof(Company.AvailabilityType)));

            // Verify type
            if (type is null) {
                Trace.WriteLine($"Failed to find company type '{typeStr}' in mod package '{companyModPackage.ID}'.", nameof(CompanySerializer));
                return new(0, null);
            }

            // Create builder instance
            CompanyBuilder builder = CompanyBuilder.NewCompany(name, type, availability, faction, guid);
            if (ReadPropertyThroughSerialisation<CompanyStatistics>(ref reader, nameof(Company.Statistics)) is CompanyStatistics statistics) {
                builder.Statistics = statistics;
            }

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
                string property = reader.ReadProperty() ?? throw new ObjectPropertyNotFoundException();
                var inputType = arrayTypes[property];

                // Get data and set it
                if (JsonSerializer.Deserialize(ref reader, inputType) is not Array values) {
                    throw new InvalidDataException();
                }

                switch (property) {
                    case nameof(Company.Units):
                        for (int i = 0; i < values.Length; i++) {
                            builder.AddUnit(UnitBuilder.EditUnit(values.GetValue(i) as Squad ?? throw new InvalidDataException()));
                        }
                        break;
                    case nameof(Company.Abilities):
                        for (int i = 0; i < values.Length; i++) {
                            builder.AddAbility(values.GetValue(i) as Ability ?? throw new InvalidDataException());
                        }
                        break;
                    case nameof(Company.Inventory):
                        for (int i = 0; i < values.Length; i++) {
                            builder.AddEquipment(values.GetValue(i) as CompanyItem ?? throw new InvalidDataException());
                        }
                        break;
                    case nameof(Company.Upgrades):
                        // TMP
                        if (values.Length > 0)
                            throw new NotImplementedException();
                        break;
                    case nameof(Company.Modifiers):
                        // TMP
                        if (values.Length > 0)
                            throw new NotImplementedException();
                        break;
                    default:
                        throw new InvalidDataException();
                }

            }

            // Verify checksum and return if success; otherwise throw checksum violation error
            Company result = builder.Commit().Result;

            // Return result
            return new(checksum, result);

        }

        public override void Write(Utf8JsonWriter writer, UnverifiedCompany value, JsonSerializerOptions options) {
            JsonSerializer.Serialize(writer, value.Company, options);
        }

    }

}
