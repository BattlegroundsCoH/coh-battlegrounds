namespace Battlegrounds.Errors.Common;

/// <summary>
/// Exception thrown when a requested object property is not found. Mainly occurs while trying to deserialise json.
/// </summary>
public class ObjectPropertyNotFoundException : BattlegroundsException {

    /// <summary>
    /// Get the name of the missing property.
    /// </summary>
    public string Property { get; }

    /// <summary>
    /// Initialise a new <see cref="ObjectPropertyNotFoundException"/> instance with no descriptors.
    /// </summary>
    public ObjectPropertyNotFoundException() : base("Requested object property not found") {
        this.Property = "Unspecified";
    }

    /// <summary>
    /// Initialise a new <see cref="ObjectPropertyNotFoundException"/> instance with a custom <paramref name="message"/>.
    /// </summary>
    /// <param name="property">The name of the property not found.</param>
    public ObjectPropertyNotFoundException(string property) : base($"Requested object property '{property}' was not found for object.") { this.Property = property; }

    /// <summary>
    /// Initialise a new <see cref="ObjectPropertyNotFoundException"/> instance with a custom <paramref name="message"/>.
    /// </summary>
    /// <param name="property">The name of the property not found.</param>
    /// <param name="message">The information message to show.</param>
    public ObjectPropertyNotFoundException(string property, string message) : base(message) { this.Property = property; }

}
