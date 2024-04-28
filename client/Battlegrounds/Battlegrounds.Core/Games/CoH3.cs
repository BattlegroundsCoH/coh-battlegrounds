namespace Battlegrounds.Core.Games;

public sealed class CoH3 : IGame {

    private static readonly (string, string)[] COH3_SKIRMISH_SETTINGS = [
        ("gamemode", "victory_points"),
        ("gamemode_option", "500"),
        ("income_modifier", "1.0"),
        ("damage_modifier", "1.0"),
        ("supply", "1"),
        ("weather", "1"),
    ];

    public string Name => "company_of_heroes_3";

    public string DefaultScenario => "desert_village_2p_mkiii";

    public (string, string)[] SkirmishSettings => COH3_SKIRMISH_SETTINGS;

}
