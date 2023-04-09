using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

using Battlegrounds.Errors.Common;
using Battlegrounds.Functional;

namespace Battlegrounds.Game.Blueprints.Extensions;

public class LoadoutExtension {

    public readonly struct Entry {
        public readonly int Count;
        public readonly string EntityBlueprint;
        public Entry(int count, string ebp) {
            Count = count;
            EntityBlueprint = ebp;
        }
        public override string ToString() => $"{EntityBlueprint}:{Count}";
    }

    private readonly Entry[] m_entries;

    public int Count => m_entries.Sum(x => x.Count);

    public LoadoutExtension(Entry[] entries) {
        m_entries = entries;
    }

    public T[] Select<T>(Func<Entry, T> mapFunction)
        => m_entries.Select(mapFunction).ToArray();

    public T[] SelectMany<T>(Func<Entry, T[]> mapFunction)
        => m_entries.SelectMany(mapFunction).ToArray();

    public static LoadoutExtension FromJson(ref Utf8JsonReader reader) {
        List<Entry> entries = new();
        while (reader.Read() && reader.TokenType is not JsonTokenType.EndArray) {
            string ebp = string.Empty;
            int count = 0;
            while (reader.Read() && reader.TokenType is not JsonTokenType.EndObject) {
                if (reader.ReadProperty() is "EBP") {
                    ebp = reader.GetString() ?? throw new ObjectPropertyNotFoundException("EBP");
                } else {
                    count = reader.GetInt32();
                }
            }
            entries.Add(new(count, ebp));
        }
        return new(entries.ToArray());
    }

    public string?[] GetEntities()
        => (0..m_entries.Length).Map(GetEntityName);

    public string? GetEntityName(Index index)
        => GetEntityName(index.IsFromEnd ? m_entries.Length - index.Value : index.Value);

    public string? GetEntityName(int index)
        => 0 <= index && index < m_entries.Length ? m_entries[index].EntityBlueprint : null;

    public override string ToString() => string.Join(",", m_entries.Select(x => x.ToString()));

}
