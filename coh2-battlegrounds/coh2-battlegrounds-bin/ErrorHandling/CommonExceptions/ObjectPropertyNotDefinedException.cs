namespace Battlegrounds.ErrorHandling.CommonExceptions;

/// <summary>
/// Exception thrown when a requested object property is not properly defined (for example if the property value is <see langword="null"/> when expected not to).
/// </summary>
public class ObjectPropertyNotDefinedException<T> : BattlegroundsException {

    /// <summary>
    /// Get the name of the missing property.
    /// </summary>
    public string Property { get; }

    /// <summary>
    /// Initialise a new <see cref="ObjectPropertyNotDefinedException{T}"/> instance with no descriptors.
    /// </summary>
    public ObjectPropertyNotDefinedException() : base("Requested object property not properly defined") {
        this.Property = "Unspecified";
    }

    /// <summary>
    /// Initialise a new <see cref="ObjectPropertyNotDefinedException{T}"/> instance with the <paramref name="property"/> value set.
    /// </summary>
    /// <param name="property">The name of the property not properly defined.</param>
    public ObjectPropertyNotDefinedException(string property) : base($"Requested object property '{property}' on object {typeof(T).Name} was not properly defined.") { 
        this.Property = property; 
    }

    /// <summary>
    /// Initialise a new <see cref="ObjectPropertyNotDefinedException{T}"/> instance with a custom <paramref name="message"/>.
    /// </summary>
    /// <param name="property">The name of the property not properly defined.</param>
    /// <param name="message">The information message to show.</param>
    public ObjectPropertyNotDefinedException(string property, string message) : base(message) { this.Property = property; }

}
