using Battlegrounds.Services;

namespace Battlegrounds.Models.Blueprints;

/// <summary>
/// Represents a reference to a blueprint of type T that can be resolved from a repository on demand.
/// </summary>
/// <remarks>The referenced blueprint is resolved lazily and cached after the first access. This class provides an
/// implicit conversion to T for convenient usage in code.</remarks>
/// <typeparam name="T">The type of blueprint to reference. Must inherit from Blueprint.</typeparam>
/// <param name="repository">The repository used to resolve the blueprint instance.</param>
/// <param name="bp">The unique identifier or key of the blueprint to reference within the repository.</param>
public class BlueprintReference<T>(IBlueprintRepository repository, string bp)
    where T : Blueprint {
    
    public static readonly BlueprintReference<T> None = new(null!, string.Empty);

    private readonly IBlueprintRepository _repository = repository;
    private readonly string _bp = bp;

    private T? _resolved;

    /// <summary>
    /// Gets the resolved blueprint instance of type T from the repository.
    /// </summary>
    /// <remarks>The blueprint is retrieved from the repository on first access and cached for subsequent
    /// calls. Accessing this property does not guarantee a new instance; repeated accesses return the same resolved
    /// object.</remarks>
    public T? Blueprint {
        get {
            if (_resolved is not null)
                return _resolved;
            return _resolved = _repository?.GetBlueprint<T>(_bp);
        }
    }

    /// <summary>
    /// Defines an implicit conversion from a BlueprintReference<T> to its underlying blueprint of type T.
    /// </summary>
    /// <remarks>This operator enables direct assignment of a BlueprintReference<T> to a variable of type T,
    /// returning the referenced blueprint. If the reference is null, the result will be the default value for type
    /// T.</remarks>
    /// <param name="reference">The BlueprintReference<T> instance to convert.</param>
    public static implicit operator T?(BlueprintReference<T> reference) => reference.Blueprint;

    public override string ToString() => Blueprint?.Id ?? (string.IsNullOrEmpty(_bp) ? "No Reference" : _bp);

    public override bool Equals(object? obj) => obj is BlueprintReference<T> reference && reference._bp == _bp;

    public override int GetHashCode() {
        return HashCode.Combine(_bp);
    }

}
