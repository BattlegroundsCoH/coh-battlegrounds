using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

using Battlegrounds.Functional;
using Battlegrounds.Game.Database.Management;

namespace Battlegrounds.Game.Database.Extensions {

    public class LoadoutExtension {

        public readonly struct Entry {
            public readonly int Count;
            public readonly string EntityBlueprint;
            public Entry(int count, string ebp) {
                this.Count = count;
                this.EntityBlueprint = ebp;
            }
            public override string ToString() => $"{this.EntityBlueprint}:{this.Count}";
        }

        private readonly Entry[] m_entries;

        public int Count => this.m_entries.Sum(x => x.Count);

        public LoadoutExtension(Entry[] entries) {
            this.m_entries = entries;
        }

        public T[] Select<T>(Func<Entry, T> mapFunction)
            => this.m_entries.Select(mapFunction).ToArray();

        public T[] SelectMany<T>(Func<Entry, T[]> mapFunction)
            => this.m_entries.SelectMany(mapFunction).ToArray();

        public static LoadoutExtension FromJson(ref Utf8JsonReader reader) {
            List<Entry> entries = new();
            while (reader.Read() && reader.TokenType is not JsonTokenType.EndArray) {
                string ebp = string.Empty;
                int count = 0;
                while (reader.Read() && reader.TokenType is not JsonTokenType.EndObject) {
                    if (reader.ReadProperty() is "EBP") {
                        ebp = reader.GetString();
                    } else {
                        count = reader.GetInt32();
                    }
                }
                entries.Add(new(count, ebp));
            }
            return new(entries.ToArray());
        }

        public EntityBlueprint[] GetEntities()
            => (0..this.m_entries.Length).Map(i => this.GetEntity(i));

        public EntityBlueprint GetEntity(Index index)
            => this.GetEntity(index.IsFromEnd ? this.m_entries.Length - index.Value : index.Value);

        public EntityBlueprint GetEntity(int index)
            => 0 <= index && index < this.m_entries.Length ? BlueprintManager.FromBlueprintName<EntityBlueprint>(this.m_entries[index].EntityBlueprint) : null;

        public override string ToString() => string.Join(",", this.m_entries.Select(x => x.ToString()));

    }

}
