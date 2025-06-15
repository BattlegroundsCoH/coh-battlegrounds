using Battlegrounds.Errors.Common;

namespace Battlegrounds.Game.Database.Management;

/// <summary>
/// Exception that is thrown when a blueprint cannot be found in a collection.
/// </summary>
public class BlueprintNotFoundException : BattlegroundsException {

    /// <summary>
    /// Gets the name of the blueprint that was not found.
    /// </summary>
    public string Blueprint { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="BlueprintNotFoundException"/> class
    /// with the specified blueprint name.
    /// </summary>
    /// <param name="blueprint">The name of the blueprint that was not found.</param>
    public BlueprintNotFoundException(string blueprint) : base($"Blueprint '{blueprint}' not found.") {
        this.Blueprint = blueprint;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BlueprintNotFoundException"/> class
    /// with the specified blueprint name and message.
    /// </summary>
    /// <param name="blueprint">The name of the blueprint that was not found.</param>
    /// <param name="message">The message that describes the error.</param>
    public BlueprintNotFoundException(string blueprint, string message) : base(message) { 
        this.Blueprint = blueprint;
    }

}
