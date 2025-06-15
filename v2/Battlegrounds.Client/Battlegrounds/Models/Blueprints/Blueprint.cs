using System.Diagnostics.CodeAnalysis;

using Battlegrounds.Models.Blueprints.Extensions;

namespace Battlegrounds.Models.Blueprints;

/// <summary>
/// Represents an abstract blueprint that serves as a base for defining extensible entities.
/// </summary>
/// <remarks>A blueprint is identified by a unique ID and can be extended with custom extensions. Once frozen, the
/// blueprint becomes immutable, preventing further modifications to its ID or extensions.</remarks>
/// <param name="id"></param>
/// <param name="extensions"></param>
public abstract class Blueprint(string id, HashSet<BlueprintExtension> extensions) {
    
    protected readonly Dictionary<string, BlueprintExtension> _extensions = extensions.ToDictionary(k => k.Name);
    protected string _id = id;
    protected bool _isFrozen = false;

    /// <summary>
    /// Gets or sets the unique identifier for the blueprint.
    /// </summary>
    public string Id {
        get => _id;
        set {
            if (_isFrozen) {
                throw new InvalidOperationException("Cannot change Id after the blueprint has been frozen.");
            }
            if (string.IsNullOrEmpty(value)) {
                throw new ArgumentException("Id cannot be null or empty.", nameof(value));
            }
            _id = value;
        }
    }

    /// <summary>
    /// Gets a value indicating whether the object is in a frozen state.
    /// </summary>
    public bool IsFrozen => _isFrozen;

    /// <summary>
    /// Gets the faction association of the entity, if any.
    /// </summary>
    public string? FactionAssociation { get; init; } = string.Empty;

    /// <summary>
    /// Marks the blueprint as frozen, preventing further modifications.
    /// </summary>
    /// <remarks>Once the blueprint is frozen, any attempt to modify it will result in an exception.  This
    /// method can only be called if the blueprint is not already frozen.</remarks>
    /// <exception cref="InvalidOperationException">Thrown if the blueprint is already frozen.</exception>
    public void Freeze() {
        if (_isFrozen) {
            throw new InvalidOperationException("Blueprint is already frozen.");
        }
        _isFrozen = true;
    }

    /// <summary>
    /// Adds a new extension to the blueprint.
    /// </summary>
    /// <remarks>This method allows you to add a custom extension to the blueprint, enabling additional
    /// functionality. Extensions must have unique names and cannot be added after the blueprint has been
    /// frozen.</remarks>
    /// <param name="extension">The extension to add. Must not be <see langword="null"/> and must have a unique name.</param>
    /// <exception cref="InvalidOperationException">Thrown if the blueprint has been frozen or if an extension with the same name already exists.</exception>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="extension"/> is <see langword="null"/>.</exception>
    public void AddExtension(BlueprintExtension extension) {
        if (_isFrozen) {
            throw new InvalidOperationException("Cannot add extensions after the blueprint has been frozen.");
        }
        if (extension == null) {
            throw new ArgumentNullException(nameof(extension), "Extension cannot be null.");
        }
        if (_extensions.ContainsKey(extension.Name)) {
            throw new InvalidOperationException($"Extension with name {extension.Name} already exists.");
        }
        _extensions[extension.Name] = extension;
    }

    /// <summary>
    /// Retrieves an extension of the specified type from the collection of blueprint extensions.
    /// </summary>
    /// <typeparam name="T">The type of the extension to retrieve. Must derive from <see cref="BlueprintExtension"/>.</typeparam>
    /// <returns>The extension of type <typeparamref name="T"/> if it exists in the collection.</returns>
    /// <exception cref="KeyNotFoundException">Thrown if an extension of the specified type is not found in the collection.</exception>
    public T GetExtension<T>() where T : BlueprintExtension {
        if (_extensions.TryGetValue(typeof(T).Name, out var extension)) {
            return (T)extension;
        }
        throw new KeyNotFoundException($"Extension of type {typeof(T).Name} not found.");
    }

    /// <summary>
    /// Determines whether the current object has an extension of the specified type.
    /// </summary>
    /// <remarks>This method checks for the presence of an extension by its type name in the internal
    /// collection of extensions.</remarks>
    /// <typeparam name="T">The type of the extension to check for. Must derive from <see cref="BlueprintExtension"/>.</typeparam>
    /// <returns><see langword="true"/> if an extension of type <typeparamref name="T"/> exists; otherwise, <see
    /// langword="false"/>.</returns>
    public bool HasExtension<T>() where T : BlueprintExtension {
        return _extensions.ContainsKey(typeof(T).Name);
    }

    /// <summary>
    /// Determines whether an extension with the specified name exists.
    /// </summary>
    /// <param name="name">The name of the extension to check for. Cannot be null or empty.</param>
    /// <returns><see langword="true"/> if an extension with the specified name exists; otherwise, <see langword="false"/>.</returns>
    public bool HasExtension(string name) {
        return _extensions.ContainsKey(name);
    }

    /// <summary>
    /// Attempts to retrieve an extension of the specified type from the current blueprint.
    /// </summary>
    /// <remarks>This method uses the type name of <typeparamref name="T"/> to locate the corresponding
    /// extension. If no matching extension is found, the <paramref name="extension"/> parameter is set to <see
    /// langword="null"/>.</remarks>
    /// <typeparam name="T">The type of the extension to retrieve. Must derive from <see cref="BlueprintExtension"/>.</typeparam>
    /// <param name="extension">When this method returns <see langword="true"/>, contains the extension of type <typeparamref name="T"/>. When
    /// this method returns <see langword="false"/>, contains <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if an extension of type <typeparamref name="T"/> is found; otherwise, <see
    /// langword="false"/>.</returns>
    public bool TryGetExtension<T>([NotNullWhen(true)] out T? extension) where T : BlueprintExtension {
        if (_extensions.TryGetValue(typeof(T).Name, out var ext) && ext is T t) {
            extension = t;
            return true;
        }
        extension = null;
        return false;
    }

    /// <summary>
    /// Determines whether the specified object is equal to the current <see cref="Blueprint"/> instance.
    /// </summary>
    /// <remarks>This method performs a type check to ensure the specified object is a <see cref="Blueprint"/>
    /// and compares  the <see cref="Id"/> property for equality. It also verifies that the runtime type of the
    /// specified object  matches the runtime type of the current instance.</remarks>
    /// <param name="obj">The object to compare with the current instance. Can be <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if the specified object is a <see cref="Blueprint"/> and has the same <see cref="Id"/> 
    /// as the current instance; otherwise, <see langword="false"/>.</returns>
    public override bool Equals(object? obj) {
        return obj is Blueprint blueprint && Id == blueprint.Id && obj.GetType() == this.GetType();
    }

    /// <summary>
    /// Returns a hash code for the current object.
    /// </summary>
    /// <remarks>The hash code is derived from the <see cref="Id"/> property.  This method is suitable for use
    /// in hashing algorithms and data structures such as hash tables.</remarks>
    /// <returns>An integer representing the hash code for the current object.</returns>
    public override int GetHashCode() {
        return Id.GetHashCode();
    }

    /// <summary>
    /// Returns a string representation of the current instance, including its type name and identifier.
    /// </summary>
    /// <returns>A string in the format "<c>TypeName(Id)</c>", where <c>TypeName</c> is the name of the object's type and
    /// <c>Id</c> is its identifier.</returns>
    public override string ToString() => $"{GetType().Name}({Id})";

}
