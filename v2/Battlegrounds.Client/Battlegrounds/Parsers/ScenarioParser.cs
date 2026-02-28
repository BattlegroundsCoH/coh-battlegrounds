using System.IO;

using Battlegrounds.Models.Playing;
using Battlegrounds.Serializers;
using Battlegrounds.Services;

using Microsoft.Extensions.Logging;

using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Battlegrounds.Parsers;

/// <summary>
/// Provides functionality to parse scenario data for a specified game type from a stream source.
/// </summary>
/// <remarks>This parser uses a custom deserialization strategy to handle scenario data, including locale string
/// conversion. Logging is performed for errors encountered during parsing. The parser is intended for use with streams
/// containing scenario definitions in a supported format.</remarks>
/// <typeparam name="G">The type of game for which scenarios are parsed. Must inherit from Game.</typeparam>
public sealed class ScenarioParser<G>(IGameLocaleService gameLocaleService, ILogger<ScenarioParser<G>> logger) where G : Game {

    private readonly ILogger<ScenarioParser<G>> _logger = logger;
    private readonly IDeserializer _deserializer = new DeserializerBuilder()
            .WithNamingConvention(HyphenatedNamingConvention.Instance)
            .IgnoreFields()
            .IgnoreUnmatchedProperties()
            .WithTypeConverter(new LocaleStringYamlTypeConverter<G>(gameLocaleService)) // Register a type converter for LocaleString to use the locale service
            .Build();

    public async Task<List<Scenario>> ParseScenarios(Stream source) {

        if (!source.CanRead) {
            throw new ArgumentException("The provided stream is not readable.", nameof(source));
        }

        using StreamReader reader = new(source, leaveOpen: true);

        return await Task.Run(() => {
            try {
                return _deserializer.Deserialize<List<Scenario>>(reader);
            } catch (Exception ex) {
                _logger.LogError(ex, "Failed to parse scenarios from the provided stream.");
                throw;
            }
        });

    }

}
