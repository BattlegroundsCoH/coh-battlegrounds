namespace Battlegrounds;

public static class Consts {

    public const string UCS_LANG_ENGLISH = "english";
    public const string UCS_LANG_GERMAN = "german";
    public const string UCS_LANG_FRENCH = "french";
    public const string UCS_LANG_POLISH = "polish";

    public static readonly HashSet<string> SupportedLanguages = [
        UCS_LANG_ENGLISH,
        UCS_LANG_GERMAN,
        UCS_LANG_FRENCH,
        UCS_LANG_POLISH
    ];

}
