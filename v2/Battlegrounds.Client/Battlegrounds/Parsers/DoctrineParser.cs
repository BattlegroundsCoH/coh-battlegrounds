using System.IO;

using Battlegrounds.Models.Blueprints;
using Battlegrounds.Models.Doctrines;
using Battlegrounds.Models.Playing;
using Battlegrounds.Services;

using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Battlegrounds.Parsers;

public sealed class DoctrineParser<G> where G : Game {

    private readonly IDeserializer _deserializer;
    private readonly IBlueprintService _blueprintService;
    private readonly IDoctrineService _doctrineService;

    public DoctrineParser(IBlueprintService blueprintService, IDoctrineService doctrineService) {

        _blueprintService = blueprintService;
        _doctrineService = doctrineService;

        _deserializer = new DeserializerBuilder()
            .WithNamingConvention(HyphenatedNamingConvention.Instance)
            .IgnoreFields()
            .IgnoreUnmatchedProperties()
            .WithTypeConverter(new LazyDoctrineDefinitionTypeDeserializer(_doctrineService))
            .WithTypeConverter(new BlueprintReferenceTypeDeserializer<SquadBlueprint>(_blueprintService))
            .WithTypeConverter(new BlueprintReferenceTypeDeserializer<UpgradeBlueprint>(_blueprintService))
            .Build();

    }
    
    public async Task<DoctrineDefinition> ParseDoctrineAsync(string doctrineFile, IBlueprintService blueprintService, CancellationToken cancellationToken) {

        using var stream = File.OpenRead(doctrineFile);
        using StreamReader reader = new(stream, leaveOpen: true);

        return (await Task.Run(() => _deserializer.Deserialize<DoctrineContainer>(reader))).Doctrine;

    }

    private class DoctrineContainer {
        public required DoctrineDefinition Doctrine { get; init; }
    }

    private class LazyDoctrineDefinitionTypeDeserializer(IDoctrineService doctrineService) : IYamlTypeConverter {

        public bool Accepts(Type type) => type == typeof(Lazy<DoctrineDefinition>);

        public object? ReadYaml(IParser parser, Type type, ObjectDeserializer rootDeserializer) {

            if (parser.Current is Scalar scalar) {
                if (scalar.Value is null) {
                    parser.MoveNext(); // Move to the next parser event after reading the scalar value.
                    return null; // Return null if the scalar value is null, indicating no parent doctrine.
                }
                string? parentId = scalar.Value;
                parser.MoveNext(); // Move to the next parser event after reading the scalar value.
                return new Lazy<DoctrineDefinition>(() => doctrineService.GetDoctrineById(parentId));
            }

            throw new InvalidDataException("Expected a scalar value for lazy doctrine reference.");
        }

        public void WriteYaml(IEmitter emitter, object? value, Type type, ObjectSerializer serializer) {
            throw new NotSupportedException();
        }

    }

    private class BlueprintReferenceTypeDeserializer<T>(IBlueprintService blueprintService) : IYamlTypeConverter where T : Blueprint {
        public bool Accepts(Type type) => type == typeof(BlueprintReference<T>);

        public object? ReadYaml(IParser parser, Type type, ObjectDeserializer rootDeserializer) {
            if (parser.Current is Scalar scalar) {
                if (scalar.Value is null) {
                    parser.MoveNext(); // Move to the next parser event after reading the scalar value.
                    return BlueprintReference<T>.None; // Return an empty reference if the scalar value is null.
                }
                string? blueprintId = scalar.Value;
                parser.MoveNext(); // Move to the next parser event after reading the scalar value.
                return new BlueprintReference<T>(blueprintService.GetBlueprintRepositoryForGame<G>(), blueprintId);
            }
            throw new InvalidDataException("Expected a scalar value for blueprint reference.");
        }

        public void WriteYaml(IEmitter emitter, object? value, Type type, ObjectSerializer serializer) {
            throw new NotSupportedException();
        }
    }

}
