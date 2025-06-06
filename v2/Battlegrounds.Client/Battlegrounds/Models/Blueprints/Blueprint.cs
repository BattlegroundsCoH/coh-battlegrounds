using System.Diagnostics.CodeAnalysis;

using Battlegrounds.Models.Blueprints.Extensions;

namespace Battlegrounds.Models.Blueprints;

public abstract class Blueprint(string id, HashSet<BlueprintExtension> extensions) {
    
    protected readonly Dictionary<string, BlueprintExtension> _extensions = extensions.ToDictionary(k => k.Name);

    public string Id => id;

    public string? FactionAssociation { get; init; } = string.Empty;

    public T GetExtension<T>() where T : BlueprintExtension {
        if (_extensions.TryGetValue(typeof(T).Name, out var extension)) {
            return (T)extension;
        }
        throw new KeyNotFoundException($"Extension of type {typeof(T).Name} not found.");
    }

    public bool HasExtension<T>() where T : BlueprintExtension {
        return _extensions.ContainsKey(typeof(T).Name);
    }

    public bool HasExtension(string name) {
        return _extensions.ContainsKey(name);
    }

    public bool TryGetExtension<T>([NotNullWhen(true)] out T? extension) where T : BlueprintExtension {
        if (_extensions.TryGetValue(typeof(T).Name, out var ext) && ext is T t) {
            extension = t;
            return true;
        }
        extension = null;
        return false;
    }

    public override bool Equals(object? obj) {
        return obj is Blueprint blueprint && Id == blueprint.Id && obj.GetType() == this.GetType();
    }

    public override int GetHashCode() {
        return Id.GetHashCode();
    }

    public override string ToString() => $"{GetType().Name}({Id})";

}
