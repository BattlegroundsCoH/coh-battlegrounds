using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

using Battlegrounds.Errors.Common;
using Battlegrounds.Functional;
using Battlegrounds.Game.Blueprints;
using Battlegrounds.Game.Gameplay;
using Battlegrounds.Game.Gameplay.DataConverters;
using Battlegrounds.Logging;
using Battlegrounds.Modding;
using Battlegrounds.Verification;

namespace Battlegrounds.Game.DataCompany;

/// <summary>
/// Class for serializing a <see cref="Company"/> to and from json. Extends <see cref="JsonConverter{T}"/>.
/// </summary>
public class CompanySerializer : JsonConverter<Company> {

    private static readonly Logger logger = Logger.CreateLogger();

    /// <inheritdoc/>
    public override Company? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
        var newOptions = new JsonSerializerOptions(options);
        newOptions.Converters.Add(new UnverifiedCompanyJson());
        var unverified = JsonSerializer.Deserialize<UnverifiedCompany>(ref reader, newOptions);
        if (unverified.Success) {
            return unverified.Company;
        } else {
            logger.Error("Failed to read company json...");
            return null;
        }
    }

    private static string ReadProperty(ref Utf8JsonReader reader, string property)
        => reader.GetString() == property && reader.Read() ? (reader.ReadProperty() ?? string.Empty) : string.Empty;

    private static string ReadPropertyOrValueIfNotExists(ref Utf8JsonReader reader, string property, string propertyOrValue)
        => reader.GetString() == property && reader.Read() ? (reader.ReadProperty() ?? string.Empty) : propertyOrValue;

    private static ulong ReadChecksum(ref Utf8JsonReader reader, string property)
        => reader.GetString() == property && reader.Read() ? reader.ReadUlongProperty() : 0;

    private static T? ReadPropertyThroughSerialisation<T>(ref Utf8JsonReader reader, string property)
        => reader.GetString() == property && reader.Read() ? JsonSerializer.Deserialize<T>(ref reader) : default;

    /// <inheritdoc/>
    public override void Write(Utf8JsonWriter writer, Company value, JsonSerializerOptions options) {

        JsonSerializerOptions newOptions = new JsonSerializerOptions(options);
        newOptions.Converters.Add(new SquadWriter.SquadJson(null));
        newOptions.Converters.Add(new CompanyItem.CompanyItemSerialiser(null));

        // Recalculate the checksum
        value.CalculateChecksum();

        // Begin object
        writer.WriteStartObject();

        // Write all the data
        writer.WriteString(nameof(Company.Name), value.Name);
        writer.WriteString(nameof(Company.Army), value.Army.Name);
        writer.WriteString(nameof(Company.Game), value.Game.ToString());
        writer.WriteString(nameof(Company.TuningGUID), value.TuningGUID.GUID);
        writer.WriteString(nameof(Company.AppVersion), value.AppVersion);
        writer.WriteNumber(nameof(Company.Checksum), value.Checksum);
        writer.WriteString(nameof(Company.Type), value.Type.Id);
        writer.WriteString(nameof(Company.AvailabilityType), value.AvailabilityType.ToString());

        // Write statistics
        writer.WritePropertyName(nameof(Company.Statistics));
        JsonSerializer.Serialize(writer, value.Statistics, newOptions);

        // Write Units
        writer.WritePropertyName(nameof(Company.Units));
        JsonSerializer.Serialize(writer, value.Units, newOptions);

        // Write abilities
        writer.WritePropertyName(nameof(Company.Abilities));
        JsonSerializer.Serialize(writer, value.Abilities, newOptions);

        // Write inventory
        writer.WritePropertyName(nameof(Company.Inventory));
        JsonSerializer.Serialize(writer, value.Inventory, newOptions);

        // Write upgrades
        writer.WritePropertyName(nameof(Company.Upgrades));
        JsonSerializer.Serialize(writer, value.Upgrades, newOptions);

        // Write modifiers
        writer.WritePropertyName(nameof(Company.Modifiers));
        JsonSerializer.Serialize(writer, value.Modifiers, newOptions);

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
        Company? delta = GetCompanyFromJson(json1) ?? throw new Exception("Failed to save company file since intermeddiate save step failed!");

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
        options.Converters.Add(new UnverifiedCompanyJson());
        options.Converters.Add(new SquadWriter.SquadJson(null));
        options.Converters.Add(new CompanyItem.CompanyItemSerialiser(null));
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
                logger.Error($"Fatal - Company '{result.Name}' has been modified{dbstr}.");
                
                throw new ChecksumViolationException(0, unverified.Checksum);
            }

            // Return company
            return result;

        } else {

            // Log error
            logger.Error("Failed to read company file...");
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

        private static readonly Logger logger = Logger.CreateLogger();

        /// <inheritdoc/>
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

            // Read the game case (and always assume CoH2)
            GameCase game = Enum.TryParse(ReadPropertyOrValueIfNotExists(ref reader, nameof(Company.Game), GameCase.CompanyOfHeroes2.ToString()), out GameCase _gm)
                ? _gm : GameCase.CompanyOfHeroes2;

            // Get proper faction
            var faction = Faction.FromName(army, game);

            // Read mod GUID and BG App version
            var guid = ModGuid.FromGuid(ReadProperty(ref reader, nameof(Company.TuningGUID)));
            var version = ReadProperty(ref reader, nameof(Company.AppVersion));
            // TODO: Validate version

            // Verify package
            if (BattlegroundsContext.ModManager.GetPackageFromGuid(guid, game) is not IModPackage companyModPackage) {
                logger.Error($"Failed to find mod package for tuning mod '{guid}'.");
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
                logger.Error($"Failed to find company type '{typeStr}' in mod package '{companyModPackage.ID}'.");
                return new(0, null);
            }

            // Create builder instance
            CompanyBuilder builder = CompanyBuilder.NewCompany(name, type, availability, faction, guid);
            if (ReadPropertyThroughSerialisation<CompanyStatistics>(ref reader, nameof(Company.Statistics)) is CompanyStatistics statistics) {
                builder.Statistics = statistics;
            }

            // Create converters
            var subOptions = new JsonSerializerOptions(options);
            if (subOptions.Converters.FirstOrDefault(x => x is SquadWriter.SquadJson) is SquadWriter.SquadJson existingSquadWriter) {
                subOptions.Converters.Remove(existingSquadWriter);
            }
            subOptions.Converters.Add(new SquadWriter.SquadJson(BattlegroundsContext.DataSource.GetBlueprints(companyModPackage, game)));
            subOptions.Converters.Add(new CompanyItem.CompanyItemSerialiser(BattlegroundsContext.DataSource.GetBlueprints(companyModPackage, game)));

            // Create helper dictionary
            var arrayTypes = new Dictionary<string, Type>() {
                [nameof(Company.Abilities)] = typeof(Ability[]),
                [nameof(Company.Units)] = typeof(Squad[]),
                [nameof(Company.Upgrades)] = typeof(UpgradeBlueprint[]),
                [nameof(Company.Modifiers)] = typeof(Modifier[]),
                [nameof(Company.Inventory)] = typeof(CompanyItem[]),
            };

            // Read arrays
            while (reader.Read() && reader.TokenType is not JsonTokenType.EndObject) {

                // Read property
                string property = reader.ReadProperty() ?? throw new ObjectPropertyNotFoundException();
                var inputType = arrayTypes[property];

                // Get data and set it
                if (JsonSerializer.Deserialize(ref reader, inputType, subOptions) is not Array values) {
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

        /// <inheritdoc/>
        public override void Write(Utf8JsonWriter writer, UnverifiedCompany value, JsonSerializerOptions options) {
            
            JsonSerializerOptions newOptions = new(options);
            newOptions.Converters.Add(new SquadWriter.SquadJson(null));
            
            JsonSerializer.Serialize(writer, value.Company, newOptions);

        }

    }

}
