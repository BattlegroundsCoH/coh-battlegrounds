using System.Text.Json.Serialization;

namespace Battlegrounds.Models.Companies;

public readonly struct CompanyEventModifier {

    public const string EVENT_TYPE_IN_MATCH = "in_match"; // Modifier for in-match events
    public const string EVENT_TYPE_KILL_SQUAD = "kill_squad"; // Modifier for killing a squad
    public const string EVENT_TYPE_EXPERIENCE_GAIN = "experience_gain"; // Modifier for gaining experience
    public const string EVENT_TYPE_STATISTICS = "statistics"; // Modifier for statistics (update infantry killed, vehicles destroyed, etc.)
    public const string EVENT_TYPE_PICKUP = "pickup"; // Modifier for picking up items

    public int SquadId { get; init; } // Identifier for the squad this modifier applies to

    public string EventType { get; init; } // Action type this modifier applies to (e.g., "Attack", "Defense", etc.)

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull | JsonIgnoreCondition.WhenWritingDefault)]
    public string? BlueprintArg { get; init; } // Optional argument for the blueprint associated with this modifier

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public int IntValue1 { get; init; } // First integer value 

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public int IntValue2 { get; init; } // Second integer value, if applicable

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public float FloatValue { get; init; } // First float value, if applicable

    public static CompanyEventModifier Kill(int squadId) 
        => new CompanyEventModifier() { SquadId = squadId, EventType = EVENT_TYPE_KILL_SQUAD }; // Modifier for killing a squad
    public static CompanyEventModifier ExperienceGain(int squadId, float experience) 
        => new CompanyEventModifier() { SquadId = squadId, EventType = EVENT_TYPE_EXPERIENCE_GAIN, FloatValue = experience }; // Modifier for gaining experience
    public static CompanyEventModifier Statistics(int squadId, int infantryKilled, int vehiclesDestroyed) 
        => new CompanyEventModifier() { SquadId = squadId, EventType = EVENT_TYPE_STATISTICS, IntValue1 = infantryKilled, IntValue2 = vehiclesDestroyed }; // Modifier for statistics
    public static CompanyEventModifier Pickup(int squadId, string blueprintArg) 
        => new CompanyEventModifier() { SquadId = squadId, EventType = EVENT_TYPE_PICKUP, BlueprintArg = blueprintArg }; // Modifier for picking up items
    public static CompanyEventModifier InMatch(int squadId)
        => new CompanyEventModifier() { SquadId = squadId, EventType = EVENT_TYPE_IN_MATCH }; // Modifier for in-match events

}
