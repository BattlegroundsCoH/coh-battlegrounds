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

    public EntityBlueprint? Blueprint { get; }

    public ulong Owner { get; }

    public LobbyPlanningObject(ulong owner, EntityBlueprint blueprint, Point start, Point? end = null) {
        this.Owner = owner;
        this.Blueprint = blueprint;
        this.VisualPosStart = start;
        this.VisualPointEnd = end;
    }

}
