using System;
using System.Diagnostics.CodeAnalysis;
using System.Windows;

using Battlegrounds.Game.Database;
using Battlegrounds.Networking.LobbySystem;

namespace BattlegroundsApp.Lobby.Planning;

/// <summary>
/// Class representing a planning object.
/// </summary>
public class LobbyPlanningObject {

    /// <summary>
    /// Get the unique id identifying the object.
    /// </summary>
    public int ObjectId { get; }

    /// <summary>
    /// Get or set the visual starting position.
    /// </summary>
    public Point VisualPosStart { get; set; }

    /// <summary>
    /// Get or set the visual end position.
    /// </summary>
    public Point? VisualPointEnd { get; set; }

    /// <summary>
    /// Get the <see cref="Battlegrounds.Game.Database.Blueprint"/> associated with the object.
    /// </summary>
    public Blueprint? Blueprint { get; }

    /// <summary>
    /// Get if the <see cref="Blueprint"/> member is a <see cref="EntityBlueprint"/> instance.
    /// </summary>
    [MemberNotNullWhen(true, nameof(Blueprint))]
    public bool IsEntity => this.Blueprint is EntityBlueprint;

    /// <summary>
    /// Get if the <see cref="Blueprint"/> member is a <see cref="SquadBlueprint"/> instance.
    /// </summary>
    [MemberNotNullWhen(true, nameof(Blueprint))]
    public bool IsSquad => this.Blueprint is SquadBlueprint;

    /// <summary>
    /// Get if this is a line placement.
    /// </summary>
    public bool IsLine { get; set; }

    /// <summary>
    /// Get the Id of the element owner.
    /// </summary>
    public ulong Owner { get; }

    /// <summary>
    /// Get the planning objective type.
    /// </summary>
    public PlanningObjectiveType ObjectiveType { get; }

    /// <summary>
    /// Get the objective order.
    /// </summary>
    public int ObjectiveOrder { get; }

    /// <summary>
    /// Iniialise a new squad/entity planning object instance.
    /// </summary>
    /// <param name="id">The id of the planning instance.</param>
    /// <param name="owner">The owenr id of the planning instance.</param>
    /// <param name="blueprint">The blueprint of the instance.</param>
    /// <param name="start">The start position</param>
    /// <param name="end">The end position</param>
    /// <param name="isLine">flag if line placement.</param>
    public LobbyPlanningObject(int id, ulong owner, Blueprint blueprint, Point start, Point? end, bool isLine = false) {

        // Verify blueprint type
        if (blueprint is not SquadBlueprint and not EntityBlueprint) {
            throw new ArgumentException($"Invalid blueprint type found '{blueprint.GetType()}'; expected {typeof(SquadBlueprint)} or {typeof(EntityBlueprint)}", nameof(blueprint));
        }

        this.ObjectId = id;
        this.Owner = owner;
        this.Blueprint = blueprint;
        this.VisualPosStart = start;
        this.VisualPointEnd = end;
        this.IsLine = isLine;
        
    }

    /// <summary>
    /// Initialise a new objective planning object instnace.
    /// </summary>
    /// <param name="id">The id of the planning instance.</param>
    /// <param name="owner">The owenr id of the planning instance.</param>
    /// <param name="start">The start position</param>
    /// <param name="objectiveType">The objective type</param>
    /// <param name="objectiveOrder">The objective order</param>
    public LobbyPlanningObject(int id, ulong owner, Point start, PlanningObjectiveType objectiveType, int objectiveOrder) {
        this.ObjectId = id;
        this.Owner = owner;
        this.Blueprint = null;
        this.VisualPosStart = start;
        this.ObjectiveType = objectiveType;
        this.ObjectiveOrder = objectiveOrder;
    }

}
