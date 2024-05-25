using Battlegrounds.Core.Games.Scenarios;

namespace Battlegrounds.Core.Test.Games.Scenarios;

public static class ScenarioFixture {

    public static readonly IScenario desert_village_2p_mkiii = new BaseScenario() { 
        Description = "", 
        FileName = "desert_village_2p_mkiii", 
        Name = "Desert Village (2)", 
        PlayerCount = 2 
    };

}
