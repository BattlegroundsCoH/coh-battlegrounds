using System.Text.Json.Serialization;

namespace Battlegrounds.Models.Companies;

/// <summary>
/// Represents a modifier that describes a specific event or action affecting a company squad, such as kills, experience
/// gains, statistics updates, or item pickups.
/// </summary>
/// <remarks>Use this struct to encapsulate event-related data for a squad within a company context. The modifier
/// includes information about the event type, the squad it applies to, and optional arguments or values relevant to the
/// event. Static factory methods are provided to create modifiers for common event types. This struct is immutable and
/// intended for use in event processing or tracking scenarios.</remarks>
public readonly struct CompanyEventModifier {

    public const string EVENT_TYPE_IN_MATCH = "in_match"; // Modifier for in-match events
    public const string EVENT_TYPE_KILL_SQUAD = "kill_squad"; // Modifier for killing a squad
    public const string EVENT_TYPE_EXPERIENCE_GAIN = "experience_gain"; // Modifier for gaining experience
    public const string EVENT_TYPE_STATISTICS = "statistics"; // Modifier for statistics (update infantry killed, vehicles destroyed, etc.)
    public const string EVENT_TYPE_PICKUP = "pickup"; // Modifier for picking up items
    public const string EVENT_TYPE_CAPTURE = "capture"; // Modifier for capturing items

    public int SquadId { get; init; } // Identifier for the squad this modifier applies to

    public string EventType { get; init; } // Action type this modifier applies to (e.g., "Attack", "Defense", etc.)

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull | JsonIgnoreCondition.WhenWritingDefault)]
    public string? BlueprintArg { get; init; } // Optional argument for the blueprint associated with this modifier

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public int IntValue1 { get; init; } // First integer value 

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public int IntValue2 { get; init; } // Second integer value, if applicable
    
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public int IntValue3 { get; init; } // Third integer value, if applicable

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public float FloatValue { get; init; } // First float value, if applicable

    /// <summary>
    /// Creates a modifier representing the event of killing a squad with the specified squad identifier.
    /// </summary>
    /// <param name="squadId">The unique identifier of the squad to be marked as killed.</param>
    /// <returns>A new CompanyEventModifier configured to represent the kill event for the specified squad.</returns>
    public static CompanyEventModifier Kill(int squadId) 
        => new CompanyEventModifier() { SquadId = squadId, EventType = EVENT_TYPE_KILL_SQUAD }; // Modifier for killing a squad

    /// <summary>
    /// Creates a modifier representing experience gain for a specified squad.
    /// </summary>
    /// <param name="squadId">The identifier of the squad that will receive the experience gain.</param>
    /// <param name="experience">The amount of experience to be awarded to the squad.</param>
    /// <returns>A CompanyEventModifier configured to apply the specified experience gain to the given squad.</returns>
    public static CompanyEventModifier ExperienceGain(int squadId, float experience) 
        => new CompanyEventModifier() { SquadId = squadId, EventType = EVENT_TYPE_EXPERIENCE_GAIN, FloatValue = experience }; // Modifier for gaining experience

    /// <summary>
    /// Creates a company event modifier representing combat statistics for a specific squad.
    /// </summary>
    /// <param name="squadId">The unique identifier of the squad for which the statistics are recorded.</param>
    /// <param name="infantryKilled">The number of enemy infantry units killed by the squad. Must be zero or greater.</param>
    /// <param name="vehiclesDestroyed">The number of enemy vehicles destroyed by the squad. Must be zero or greater.</param>
    /// <param name="losses">The number of losses sustained by the squad. Must be zero or greater.</param>
    /// <returns>A CompanyEventModifier instance containing the specified combat statistics for the squad.</returns>
    public static CompanyEventModifier Statistics(int squadId, int infantryKilled, int vehiclesDestroyed, int losses) 
        => new CompanyEventModifier() { SquadId = squadId, EventType = EVENT_TYPE_STATISTICS, IntValue1 = infantryKilled, IntValue2 = vehiclesDestroyed, IntValue3 = losses }; // Modifier for statistics

    /// <summary>
    /// Creates a modifier representing a pickup event for a specified squad and blueprint.
    /// </summary>
    /// <param name="squadId">The identifier of the squad performing the pickup action.</param>
    /// <param name="blueprintArg">The blueprint argument specifying the item or entity to be picked up.</param>
    /// <returns>A new CompanyEventModifier configured for a pickup event with the specified squad and blueprint.</returns>
    public static CompanyEventModifier Pickup(int squadId, string blueprintArg) 
        => new CompanyEventModifier() { SquadId = squadId, EventType = EVENT_TYPE_PICKUP, BlueprintArg = blueprintArg }; // Modifier for picking up items

    /// <summary>
    /// Creates a modifier that represents a capture event for a specified blueprint argument.
    /// </summary>
    /// <param name="blueprintArg">The identifier or argument associated with the blueprint to be captured. Cannot be null.</param>
    /// <returns>A new instance of CompanyEventModifier configured for a capture event using the specified blueprint argument.</returns>
    public static CompanyEventModifier Capture(string blueprintArg)
        => new CompanyEventModifier() { EventType = EVENT_TYPE_CAPTURE, BlueprintArg = blueprintArg }; // Modifier for capturing items

    /// <summary>
    /// Creates a modifier representing an in-match event for the specified squad.
    /// </summary>
    /// <param name="squadId">The unique identifier of the squad for which the in-match event modifier is created.</param>
    /// <returns>A new instance of CompanyEventModifier configured for in-match events for the specified squad.</returns>
    public static CompanyEventModifier InMatch(int squadId)
        => new CompanyEventModifier() { SquadId = squadId, EventType = EVENT_TYPE_IN_MATCH }; // Modifier for in-match events

}
