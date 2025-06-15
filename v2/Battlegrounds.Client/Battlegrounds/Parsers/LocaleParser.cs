using System.IO;
using System.Text;

using Microsoft.Extensions.Logging;

using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Battlegrounds.Parsers;

/// <summary>
/// Provides functionality for parsing locale data from YAML-formatted streams.
/// </summary>
/// <remarks>This class is designed to read and deserialize YAML content containing locale information, filter out
/// unsupported languages,  and convert locale keys to their numeric representations. It ensures that only languages
/// specified in  <see cref="Consts.SupportedLanguages"/> are included in the parsed result.</remarks>
/// <param name="logger">The service logger</param>
public sealed class LocaleParser(ILogger<LocaleParser> logger) {

    private struct LocaleRefStr : IYamlConvertible {
        public uint Value { get; set; }

        public void Read(IParser parser, Type expectedType, ObjectDeserializer nestedObjectDeserializer) {
            var scalar = parser.Consume<Scalar>();
            string scalarValue = scalar.Value.Trim();
            if (scalarValue.StartsWith('$')) {
                scalarValue = scalarValue[1..]; // Remove leading '$'
            }
            if (!uint.TryParse(scalarValue, out uint value)) {
                throw new YamlException(scalar.Start, scalar.End, $"Invalid locale reference string: '{scalarValue}'");
            }
            Value = value;
        }

        public void Write(IEmitter emitter, ObjectSerializer nestedObjectSerializer) {
            throw new NotImplementedException();
        }
    }

    private readonly ILogger<LocaleParser> _logger = logger;
    private readonly IDeserializer _deserializer = 
        new DeserializerBuilder()
            .WithNamingConvention(HyphenatedNamingConvention.Instance)
            .IgnoreFields()
            .IgnoreUnmatchedProperties()
            .Build();

    /// <summary>
    /// Parses locale data from a YAML-formatted stream and returns a dictionary of supported languages mapped to their
    /// respective locale entries.
    /// </summary>
    /// <remarks>The method reads and deserializes YAML content from the provided stream, filters out
    /// unsupported languages, and converts locale keys to their numeric representations. Only languages specified in
    /// <see cref="Consts.SupportedLanguages"/> are included in the result.</remarks>
    /// <param name="source">The input stream containing YAML-formatted locale data. The stream must be readable and non-empty.</param>
    /// <returns>A dictionary where each key is a supported language code (e.g., "english", "german") and the value is another dictionary
    /// mapping numeric locale keys to their corresponding string values.</returns>
    /// <exception cref="ArgumentException">Thrown if <paramref name="source"/> is not readable or is empty.</exception>
    public async Task<Dictionary<string, Dictionary<uint, string>>> ParseLocalesAsync(Stream source) {
        ArgumentNullException.ThrowIfNull(source, nameof(source));
        if (!source.CanRead) {
            throw new ArgumentException("The provided stream is not readable.", nameof(source));
        }
        
        if (source.Length == 0) {
            throw new ArgumentException("The provided stream is empty.", nameof(source));
        }

        using StreamReader reader = new(source, encoding: Encoding.UTF8, leaveOpen: true);
        Dictionary<string, Dictionary<LocaleRefStr, string>> locales = await DeserializeYamlContents(reader);
        Dictionary<string, Dictionary<uint, string>> result = [];
        
        foreach (var (lang, entries) in locales) {
            if (!Consts.SupportedLanguages.Contains(lang)) {
                _logger.LogWarning("Unsupported language '{Language}' found in locale data, skipping.", lang);
                continue;
            }
            result[lang] = entries.ToDictionary(kvp => kvp.Key.Value, kvp => kvp.Value);
        }
        return result;

    }

    private Task<Dictionary<string, Dictionary<LocaleRefStr, string>>> DeserializeYamlContents(StreamReader reader) => Task.Run(() => {
        Dictionary<string, Dictionary<LocaleRefStr, string>> result = [];
        try {
            result = _deserializer.Deserialize<Dictionary<string, Dictionary<LocaleRefStr, string>>>(reader);
        } catch (YamlException ex) {
            _logger.LogError(ex, "Failed to deserialize YAML content: {Message}", ex.Message);
            throw new InvalidDataException("The provided stream does not contain valid YAML content.", ex); // Needs to wrap the exception for better clarity
        }
        return result;
    });

}
