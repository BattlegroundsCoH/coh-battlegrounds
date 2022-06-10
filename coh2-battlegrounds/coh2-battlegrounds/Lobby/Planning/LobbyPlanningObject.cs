using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

using Battlegrounds.Game.Database;

namespace BattlegroundsApp.Lobby.Planning;

public class LobbyPlanningObject {

    public Point VisualPosStart { get; set; }

    public Point? VisualPointEnd { get; set; }

    public Blueprint Blueprint { get; }

    public bool IsEntity => this.Blueprint is EntityBlueprint;

    public bool IsSquad => this.Blueprint is SquadBlueprint;

    public ulong Owner { get; }

    public LobbyPlanningObject(ulong owner, Blueprint blueprint, Point start, Point? end = null) {
        this.Owner = owner;
        this.Blueprint = blueprint;
        this.VisualPosStart = start;
        this.VisualPointEnd = end;
    }

}
