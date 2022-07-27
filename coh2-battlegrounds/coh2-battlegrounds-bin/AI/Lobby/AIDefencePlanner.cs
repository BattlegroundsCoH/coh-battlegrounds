using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Battlegrounds.Functional;
using Battlegrounds.Game;
using Battlegrounds.Game.Database;
using Battlegrounds.Game.DataCompany;
using Battlegrounds.Networking.LobbySystem;

namespace Battlegrounds.AI.Lobby;

/// <summary>
/// 
/// </summary>
public class AIDefencePlanner {

    public record AIPlanElement(byte AIIndex, ILobbyPlanElement PlanElement);

    private record SquadPlacement(SquadBlueprint? Sbp, GamePosition Pos, GamePosition? Lookat);
    private record EntityPlacement(EntityBlueprint Ebp, GamePosition Pos, GamePosition? Lookat);

    private readonly AIMapAnalysis? m_analysis;
    private readonly List<GamePosition> m_defendPositions;
    private readonly List<SquadPlacement> m_defendSquads;
    private readonly List<EntityPlacement> m_defendEntities;
    private readonly Dictionary<byte, GamePosition> m_defenderOrigins;
    private readonly int m_defenderCount;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="mapAnalysis"></param>
    public AIDefencePlanner(AIMapAnalysis? mapAnalysis, int defenders) {
        this.m_analysis = mapAnalysis;
        this.m_defendPositions = new();
        this.m_defendEntities = new();
        this.m_defendSquads = new();
        this.m_defenderOrigins = new();
        this.m_defenderCount = defenders;
    }

    /// <summary>
    /// 
    /// </summary>
    public void SetHumanDefences((GamePosition, GamePosition?)[] squads, (EntityBlueprint, GamePosition, GamePosition?)[] entities) {
        squads.Map(x => new SquadPlacement(null, x.Item1, x.Item2)).ForEach(this.m_defendSquads.Add);
        entities.Map(x => new EntityPlacement(x.Item1, x.Item2, x.Item3)).ForEach(this.m_defendEntities.Add);
    }

    /// <summary>
    /// 
    /// </summary>
    public void SetDefenceGoals(GamePosition[] defendPositions)
        => this.m_defendPositions.AddRange(defendPositions);

    /// <summary>
    /// Create subdivisions of the map, marking where an AI player can place units/defences 
    /// </summary>
    /// <param name="scenario">The scenario to subdivide</param>
    public void Subdivide(Scenario scenario, byte tid, byte[] indices) {



    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="teamIndex"></param>
    /// <param name="indexOnTeam"></param>
    /// <param name="company"></param>
    /// <param name="scenario"></param>
    public void CreateDefencePlan(byte teamIndex, byte indexOnTeam, Company company, Scenario scenario) {

        // Invoke barricde/mine placement
        if (this.m_analysis is not null) {

        }

    }

    public AIPlanElement[] GetSquads() => Array.Empty<AIPlanElement>();

    public AIPlanElement[] GetEntities() => Array.Empty<AIPlanElement>();

}
