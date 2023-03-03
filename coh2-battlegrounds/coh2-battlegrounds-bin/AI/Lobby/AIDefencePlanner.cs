using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

using Battlegrounds.Functional;
using Battlegrounds.Game;
using Battlegrounds.Game.Database;
using Battlegrounds.Game.DataCompany;
using Battlegrounds.Game.Scenarios;
using Battlegrounds.Modding.Content;
using Battlegrounds.Networking.LobbySystem;
using Battlegrounds.Networking.LobbySystem.json;
using Battlegrounds.Util;

namespace Battlegrounds.AI.Lobby;

/// <summary>
/// 
/// </summary>
public class AIDefencePlanner {

    public const double RANDOM_OFFSET_RANGE = 45;

    public record AIPlanElement(byte AIIndex, ILobbyPlanElement PlanElement);

    private record SquadPlacement(SquadBlueprint? Sbp, GamePosition Pos, GamePosition? Lookat);
    private record EntityPlacement(EntityBlueprint Ebp, GamePosition Pos, GamePosition? Lookat);

    private readonly AIMapAnalysis? m_analysis;
    private readonly Gamemode m_gamemode;
    private readonly GamePosition[] m_nodes;
    private readonly List<GamePosition> m_attackerPositions;
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
        this.m_nodes = mapAnalysis?.Nodes ?? Array.Empty<GamePosition>();
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
        this.m_attackerPositions = new();

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
        var tpos = scenario.Points.Filter(x => x.EntityBlueprint is "starting_position_shared_territory");

        // Grab start positions
        var dpos = tpos.Filter(x => {
            int pid = x.Owner - 1000;
            return tid is 0 ? pid < teamSize : (pid >= teamSize);
        });
        var apos = tpos.Filter(x => {
            int pid = x.Owner - 1000;
            return tid is 0 ? (pid >= teamSize) : pid < teamSize;
        });

        // Grab attacker start pos
        this.m_attackerPositions.AddRange(apos.Map(x => x.Position));

        // Now make a lookup
        dpos.ForEach(x => this.m_defenderOrigins[(byte)(x.Owner - 1000 - offset)] = x.Position);

        // If there's an analysis, subdivide
        if (this.m_analysis is not null) {

            // Grab origin positions
            var origins = this.m_defenderOrigins.Map((i, p) => (i, p));

            // Loop over and add
            for (int i = 0; i < this.m_analysis.Nodes.Length; i++) {

                // Grab pos
                var pos = this.m_analysis.Nodes[i].RandomOffset(RANDOM_OFFSET_RANGE);

                // Ignore this node if close to a point "covered" by human ally
                if (this.m_ignorePositions.Any(x => x.SquareDistance(pos) < 45))
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
                var pos = this.m_analysis.StrategicPositions[i].Position.RandomOffset(RANDOM_OFFSET_RANGE);

                // Ignore this node if close to a point "covered" by human ally
                if (this.m_ignorePositions.Any(x => x.SquareDistance(pos) < 45))
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
        var strat = this.m_analysisStrategic[indexOnTeam].ToArray();

        // Grab entities
        var entities = this.m_gamemode.PlanningEntities[company.Army.Name];
        var mines = entities.Filter(x => x.PlacementType is AIPlacementType.Mine);
        var barricades = entities.Filter(x => x.PlacementType is AIPlacementType.Barricade);
        var bunkers = entities.Filter(x => x.PlacementType is AIPlacementType.Bunker);

        // Grab attackers and array
        var startAttackers = this.m_attackerPositions.ToArray();

        // Try place bunkers
        for (int i = 0; i < bunkers.Length; i++) {
            if (strat.Length is 0)
                break;
            int max = BattlegroundsInstance.RNG.Next(Math.Min(bunkers[i].MaxPlacement, 6));
            for (int j = 0; j < max; j++) {

                // Bail on empty nodes
                if (nodes.Count is 0)
                    break;

                // Pick a node
                int k = PickNode(strat, nodes);

                // Get nearest strat and attacking pos
                var t1 = strat.Min(x => x.Position.SquareDistance(nodes[k]) * (1 - x.Weight));
                var t2 = startAttackers.Min(x => x.SquareDistance(nodes[k]));

                // Compute angles
                var d1 = Lookat(nodes[k], t1.Position);
                var d2 = Lookat(nodes[k], t2);
                var angle = Math.Acos(Vector2.Dot(d1, d2) / (d1.Length() * d2.Length())) * Numerics.RAD2DEG;

                // If small angle, look at target; otherwise at start point
                this.m_placeEntities.Add(new(indexOnTeam, new JsonPlanElement() {
                    Blueprint = bunkers[i].EntityBlueprint,
                    SpawnPosition = nodes[k].RandomOffset(RANDOM_OFFSET_RANGE),
                    LookatPosition = angle < 45 ? t1.Position : t2,
                    IsEntity = true,
                    IsDirectional = true
                }));

                // Remove node from consideration
                nodes.RemoveAt(k);

            }
        }

        // Try place mines
        for (int i = 0; i < mines.Length; i++) {
            if (mines[i].IsLinePlacement) {
                int max = BattlegroundsInstance.RNG.Next(Math.Min(mines[i].MaxPlacement, 8));
                for (int j = 0; j < max; j++) {
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
                        roads.RemoveAt(k);
                        nodes.Remove(this.m_nodes[roads[k].First]);
                    }

                }
            }
            // Ignore not line placement
        }

        // Try place barricades
        for (int i = 0; i < barricades.Length; i++) {
            if (barricades[i].IsLinePlacement) {
                int max = BattlegroundsInstance.RNG.Next(Math.Min(barricades[i].MaxPlacement, 8));
                for (int j = 0; j < max; j++) {
                    if (roads.Count is 0)
                        break;
                    int k = PickRoad(strat, roads, nodes);
                    if (k != -1) {
                        var mid = this.m_nodes[roads[k].First].Interpolate(this.m_nodes[roads[k].Second], 0.5);
                        var dir = Lookat(mid, this.m_nodes[roads[k].Second]);
                        var cdir = new Vector2(dir.Y, -dir.X);
                        this.m_placeEntities.Add(new(indexOnTeam, new JsonPlanElement() {
                            Blueprint = barricades[i].EntityBlueprint,
                            SpawnPosition = Translate(mid, dir, -6.0f),
                            LookatPosition = Translate(mid, dir, 6.0f),
                            IsEntity = true
                        }));
                        nodes.Remove(this.m_nodes[roads[k].First]);
                        roads.RemoveAt(k);
                    }
                }
            }
            // Ignore not line placement
        }

        // Try place units
        var units = company.Units.Filter(x => x.SBP.IsTeamWeapon);
        int place = units.Length > 0 ? BattlegroundsInstance.RNG.Next(Math.Min(units.Length, 10)) : 0;
        for (int i = 0; i < place; i++) {
            if (roads.Count is 0)
                break;
            int k = PickNode(strat, nodes);
            if (k != -1) {

                // Get nearest strat and attacking pos
                var t1 = strat.Min(x => x.Position.SquareDistance(nodes[k]) * (1 - x.Weight));
                var t2 = startAttackers.Min(x => x.SquareDistance(nodes[k]));

                // Compute angles
                var d1 = Lookat(nodes[k], t1.Position);
                var d2 = Lookat(nodes[k], t2);
                var angle = Math.Acos(Vector2.Dot(d1, d2) / (d1.Length() * d2.Length())) * Numerics.RAD2DEG;

                // If small angle, look at target; otherwise at start point
                this.m_placeSquads.Add(new(indexOnTeam, new JsonPlanElement() {
                    SpawnPosition = nodes[k].RandomOffset(RANDOM_OFFSET_RANGE),
                    LookatPosition = angle < 45 ? t1.Position : t2,
                    IsEntity = false,
                    IsDirectional = true,
                    CompanyId = units[i].SquadID
                }));

                // Remove road segment
                nodes.RemoveAt(k);

            }
        }

    }

    private static Vector2 Lookat(GamePosition source, GamePosition target)
        => new((float)(target.X - source.X), (float)(target.Y - source.Y));

    private static GamePosition Translate(GamePosition root, Vector2 dir, float scalar) {
        return root + new GamePosition(dir.X * scalar, dir.Y * scalar);
    }

    private int PickNode(AIMapAnalysis.StrategicValue[] strategics, List<GamePosition> nodes) {

        // Map to score
        float[] scores = new float[nodes.Count];
        for (int i = 0; i < strategics.Length; i++) {
            var j = strategics[i];
            for (int k = 0; k < nodes.Count; k++) {
                scores[k] += (float)nodes[k].SquareDistance(j.Position) * (1 - j.Weight);
            }
        }

        // Get best score
        return scores.ArgMin();

    }

    private int PickRoad(AIMapAnalysis.StrategicValue[] strategics, List<AIMapAnalysis.RoadConnection> roads, List<GamePosition> nodes) {

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
