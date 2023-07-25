using System;
using System.Collections.Generic;

using Battlegrounds.Game.Gameplay;

namespace Battlegrounds.Game.Scenarios.CoH3;

public class CoH3Scenario : IScenario {

    public string Name { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public string Description { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public string RelativeFilename { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public string SgaName { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public byte MaxPlayers { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public ScenarioTheatre Theatre { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public bool IsWintermap { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public bool IsVisibleInLobby { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    public bool IsWorkshopMap => throw new NotImplementedException();

    public IList<string> Gamemodes { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    public bool HasValidInfoOrOptionsFile => throw new NotImplementedException();

    public PointPosition[] Points { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public GameSize PlayableSize { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public GameSize TerrainSize { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public GameSize MinimapSize { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    public GameCase Game => GameCase.CompanyOfHeroes3;

    public GamePosition FromMinimapPosition(double minimapWidth, double minimapHeight, double x, double y) {
        throw new NotImplementedException();
    }

    public GamePosition FromMinimapPosition(double minimapWidth, double minimapHeight, GamePosition minipos) {
        throw new NotImplementedException();
    }

    public GamePosition ToMinimapPosition(double minimapWidth, double minimapHeight, GamePosition worldPos) {
        throw new NotImplementedException();
    }

}
