namespace Battlegrounds.Core.Games;

public sealed class CoH3() : Game(COH3_NAME) {

    public const string COH3_NAME = "company_of_heroes_3";

    private static readonly (string, string)[] COH3_SKIRMISH_SETTINGS = [
        ("gamemode", "victory_points"),
        ("gamemode_option", "500"),
        ("income_modifier", "1.0"),
        ("damage_modifier", "1.0"),
        ("supply", "1"),
        ("weather", "1"),
    ];

    public override string DefaultScenario => "desert_village_2p_mkiii";

    public override (string, string)[] SkirmishSettings => COH3_SKIRMISH_SETTINGS;

}
