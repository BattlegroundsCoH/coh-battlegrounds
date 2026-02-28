using Battlegrounds.Models;
using Battlegrounds.Models.Playing;
using Battlegrounds.Services;

using YamlDotNet.Core;
using YamlDotNet.Serialization;

namespace Battlegrounds.Serializers;

/// <summary>
/// Provides a YAML type converter for serializing and deserializing locale-specific strings using a game locale
/// service.
/// </summary>
/// <remarks>This converter supports deserialization of YAML scalar values into LocaleString instances,
/// interpreting values that begin with a dollar sign ('$') as locale keys. The converter requires an implementation of
/// IGameLocaleService to resolve locale keys to localized strings for the specified game type. Serialization is not
/// implemented and will throw a NotImplementedException if attempted.</remarks>
/// <typeparam name="G">The game type for which locale strings are managed. Must inherit from Game.</typeparam>
/// <param name="localeService">The locale service used to resolve locale strings for the specified game type.</param>
public sealed class LocaleStringYamlTypeConverter<G>(IGameLocaleService localeService) : IYamlTypeConverter where G : Game {

    private readonly IGameLocaleService _localeService = localeService;

    public bool Accepts(Type type) => type == typeof(LocaleString);

    public object? ReadYaml(IParser parser, Type type, ObjectDeserializer rootDeserializer) {

        var scalar = parser.Consume<YamlDotNet.Core.Events.Scalar>();
        if (scalar is null) {
            return null;
        }

        string str = scalar.Value ?? string.Empty;
        if (str.StartsWith('$')) {
            str = str[1..]; // Remove the leading dollar sign
        }

        return uint.TryParse(str, out uint key) ? _localeService.FromGame<G>(key) : LocaleString.TempString(str);

    }

    public void WriteYaml(IEmitter emitter, object? value, Type type, ObjectSerializer serializer) {
        throw new NotImplementedException();
    }

}