namespace Battlegrounds.Models.Companies;

/// <summary>
/// Represents a company, including its identity, metadata, and associated squads within a game context.
/// </summary>
/// <remarks>A company is uniquely identified by its ID and is associated with a specific game and faction. The
/// squads collection contains all squads that belong to this company. Instances of this class are immutable after
/// initialization.</remarks>
public sealed class Company {

    private readonly List<Squad> _squads = [];

    /// <summary>
    /// Gets the unique identifier for the company. This ID is used to uniquely identify the company across the system and should be treated as immutable after the company instance is created.
    /// </summary>
    /// <remarks>
    /// The ID is a UUID that uniquely identifies this company instance. It is generated at the time of company creation and should not be modified thereafter. 
    /// The ID is used for tracking and referencing the company within the game and related systems.
    /// </remarks>
    public string Id { get; init; } = string.Empty;

    /// <summary>
    /// Gets the date and time when the company was created.
    /// </summary>
    public DateTime CreatedAt { get; init; } = DateTime.Now;

    /// <summary>
    /// Gets the identifier of the user who created the company.
    /// </summary>
    public string CreatedBy { get; init; } = string.Empty;

    /// <summary>
    /// Gets the date and time when the company was last updated.
    /// </summary>
    public DateTime UpdatedAt { get; init; } = DateTime.Now;

    /// <summary>
    /// Gets the identifier of the user/system who last updated the company.
    /// </summary>
    public string UpdatedBy { get; init; } = string.Empty;

    /// <summary>
    /// Gets the name of the company.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Gets the faction the company belongs to.
    /// </summary>
    public string Faction { get; init; } = string.Empty;

    /// <summary>
    /// Gets the unique identifier for the game this company is associated with.
    /// </summary>
    public string GameId { get; init; } = string.Empty;

    /// <summary>
    /// Gets the version number of the company. 
    /// This is used for concurrency control and tracking changes to the company data. Each time the company is updated, this version number should be incremented to reflect the new state of the company.
    /// It helps in ensuring that updates are applied correctly and can be used to detect conflicts when multiple updates occur simultaneously.
    /// </summary>
    public int Version { get; init; } = 1;

    /// <summary>
    /// Gets the collection of squads associated with this instance.
    /// </summary>
    /// <remarks>The collection is read-only after initialization. Use the company initializer or constructor
    /// to set the squads when creating an instance.</remarks>
    public IReadOnlyList<Squad> Squads {
        get => _squads.AsReadOnly();
        init => _squads = [.. value];
    }

}
