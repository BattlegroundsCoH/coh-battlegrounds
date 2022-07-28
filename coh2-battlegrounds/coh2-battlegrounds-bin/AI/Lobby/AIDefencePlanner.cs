using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Battlegrounds.Functional;
using Battlegrounds.Game;
using Battlegrounds.Game.Database;
using Battlegrounds.Game.Database.Management;
using Battlegrounds.Game.DataCompany;
using Battlegrounds.Modding.Content;
using Battlegrounds.Networking.LobbySystem;
using Battlegrounds.Networking.LobbySystem.json;

namespace Battlegrounds.AI.Lobby;

/// <summary>
/// 
/// </summary>
public class AIDefencePlanner {

    public record AIPlanElement(byte AIIndex, ILobbyPlanElement PlanElement);

    private record SquadPlacement(SquadBlueprint? Sbp, GamePosition Pos, GamePosition? Lookat);
    private record EntityPlacement(EntityBlueprint Ebp, GamePosition Pos, GamePosition? Lookat);

    private readonly AIMapAnalysis? m_analysis;
    private readonly Gamemode m_gamemode;
    private readonly GamePosition[] m_nodes;
    private readonly List<GamePosition> m_defendPositions;
    private readonly List<GamePosition> m_ignorePositions;
    private readonly List<SquadPlacement> m_defendSquads;
    private readonly List<EntityPlacement> m_defendEntities;
    private readonly Dictionary<byte, GamePosition> m_defenderOrigins;
    private readonly Dictionary<byte, List<GamePosition>> m_analysisNodes;
    private readonly Dictionary<byte, List<AIMapAnalysis.RoadConnection>> m_analysisRoads;
    private readonly Dictionary<byte, List<AIMapAnalysis.StrategicValue>> m_analysisStrategic;
    private readonly List<AIPlanElement> m_placeEntities;
    private readonly List<AIPlanElement> m_placeSquads;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="mapAnalysis"></param>
    public AIDefencePlanner(AIMapAnalysis? mapAnalysis, Gamemode gamemode) {
        
        // Set internal fields
        this.m_analysis = mapAnalysis;
        this.m_nodes = mapAnalysis.Nodes;
        this.m_gamemode = gamemode;

        // Create internal containers
        this.m_defendPositions = new();
        this.m_defendEntities = new();
        this.m_defendSquads = new();
        this.m_defenderOrigins = new();
        this.m_analysisNodes = new();
        this.m_analysisRoads = new();
        this.m_analysisStrategic = new();
        this.m_placeEntities = new();
        this.m_placeSquads = new();
        this.m_ignorePositions = new();

    }

    /// <summary>
    /// 
    /// </summary>
    public void SetHumanDefences((GamePosition, GamePosition?)[] squads, (EntityBlueprint, GamePosition, GamePosition?)[] entities) {
        squads.Map(x => new SquadPlacement(null, x.Item1, x.Item2)).ForEach(this.m_defendSquads.Add);
        entities.Map(x => new EntityPlacement(x.Item1, x.Item2, x.Item3)).ForEach(this.m_defendEntities.Add);
        squads.Map(x => x.Item1).Concat(entities.Map(x => x.Item2)).ForEach(this.m_ignorePositions.Add);
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

        // Get players per team
        var teamSize = scenario.MaxPlayers / 2;

        // Define offset
        var offset = tid is 0 ? 0 : teamSize;

        // Grab start positions
        var spos = scenario.Points.Filter(x => x.EntityBlueprint is "starting_position_shared_territory")
            .Filter(x => indices.Any(y => (y + offset) == (x.Owner - 1000)));

        // Now make a lookup
        spos.ForEach(x => this.m_defenderOrigins[(byte)(x.Owner - 1000 - offset)] = x.Position);

        // If there's an analysis, subdivide
        if (this.m_analysis is not null) {

            // Grab origin positions
            var origins = this.m_defenderOrigins.Map((i, p) => (i, p));

            // Loop over and add
            for (int i = 0; i < this.m_analysis.Nodes.Length; i++) {

                // Grab pos
                var pos = this.m_analysis.Nodes[i];

                // Ignore this node if close to a point "covered" by human ally
                if (this.m_ignorePositions.Any(x => x.SquareDistance(pos) < 30))
                    continue;

                // Pick smallest and assign
                var (j, _) = origins.Min(x => x.p.SquareDistance(pos));

                // Register
                if (!this.m_analysisNodes.ContainsKey(j)) {
                    this.m_analysisNodes[j] = new() { pos };
                    this.m_analysisRoads[j] = new(this.m_analysis.Roads.Filter(x => x.First == i || x.Second == i));
                } else {
                    this.m_analysisNodes[j].Add(pos);
                    this.m_analysisRoads[j].AddRange(this.m_analysis.Roads.Filter(x => x.First == i || x.Second == i));
                }

            }

            // Loop over and add
            for (int i = 0; i < this.m_analysis.StrategicPositions.Length; i++) {

                // Grab position
                var pos = this.m_analysis.StrategicPositions[i].Position;

                // Ignore this node if close to a point "covered" by human ally
                if (this.m_ignorePositions.Any(x => x.SquareDistance(pos) < 30))
                    continue;

                // Pick smallest and assign
                var (j, _) = origins.Min(x => x.p.SquareDistance(pos));

                // Register
                if (!this.m_analysisStrategic.ContainsKey(j)) {
                    this.m_analysisStrategic[j] = new() { this.m_analysis.StrategicPositions[i] };
                } else {
                    this.m_analysisStrategic[j].Add(this.m_analysis.StrategicPositions[i]);
                }

            }

        } else {

            // TODO: Random placements...

        }

    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="teamIndex"></param>
    /// <param name="indexOnTeam"></param>
    /// <param name="company"></param>
    /// <param name="scenario"></param>
    public void CreateDefencePlan(byte teamIndex, byte indexOnTeam, Company company, Scenario scenario) {

        // Grab nodes and strategic points
        var nodes = this.m_analysisNodes[indexOnTeam];
        var roads = this.m_analysisRoads[indexOnTeam];
        var strat = this.m_analysisStrategic[indexOnTeam];

        // Grab entities
        var entities = this.m_gamemode.PlanningEntities[company.Army.Name];
        var mines = entities.Filter(x => x.PlacementType is AIPlacementType.Mine);
        var barricades = entities.Filter(x => x.PlacementType is AIPlacementType.Barricade);
        var bunkers = entities.Filter(x => x.PlacementType is AIPlacementType.Bunker);

        // Try place mines
        for (int i = 0; i < mines.Length; i++) {
            if (mines[i].IsLinePlacement) {
                for (int j = 0; j < mines[i].MaxPlacement; j++) {
                    if (roads.Count is 0)
                        break;

                    int k = PickRoad(strat, roads, nodes);
                    if (k != -1) {
                        this.m_placeEntities.Add(new(indexOnTeam, new JsonPlanElement() {
                            Blueprint = mines[i].EntityBlueprint,
                            SpawnPosition = this.m_nodes[roads[k].First],
                            LookatPosition = this.m_nodes[roads[k].Second],
                            IsEntity = true
                        }));
                    }

                }
            }
            // Ignore not line placement
        }

        // Try place barricades
        for (int i = 0; i < barricades.Length; i++) {
            if (barricades[i].IsLinePlacement) {

            }
            // Ignore not line placement
        }

        // Try place bunkers
        for (int i = 0; i < bunkers.Length; i++) {
            
        }

        // Try place units
        var units = company.Units.Filter(x => x.SBP.IsTeamWeapon);
        for (int i = 0; i < Math.Min(units.Length, 12); i++) { 
            


        }

    }

    private int PickNode(List<AIMapAnalysis.StrategicValue> strategics, List<GamePosition> nodes) {

        // Map to score
        float[] scores = new float[nodes.Count];
        for (int i = 0; i < strategics.Count; i++) {
            var j = strategics[i];
            for (int k = 0; k < nodes.Count; k++) {
                scores[k] += (float)nodes[k].SquareDistance(j.Position) * (-j.Weight);
            }
        }

        // Get best score
        return scores.ArgMin();

    }

    private int PickRoad(List<AIMapAnalysis.StrategicValue> strategics, List<AIMapAnalysis.RoadConnection> roads, List<GamePosition> nodes) {

        while (roads.Count > 0 && nodes.Count > 0) {

            int n = PickNode(strategics, nodes);
            int r = roads.FindIndex(x => x.First == n || x.Second == n);
            if (r != -1) {
                return r;
            } else {
                nodes.RemoveAt(n); // Node considered and now ignored
            }

        }


        return -1;

    }

    public AIPlanElement[] GetSquads() => this.m_placeSquads.ToArray();

    public AIPlanElement[] GetEntities() => this.m_placeEntities.ToArray();

}
