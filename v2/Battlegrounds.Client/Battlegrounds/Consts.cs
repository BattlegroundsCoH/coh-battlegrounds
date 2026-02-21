using System.Text;

namespace Battlegrounds;


public static class Consts {

    public const string UCS_LANG_ENGLISH = "english";
    public const string UCS_LANG_GERMAN = "german";
    public const string UCS_LANG_FRENCH = "french";
    public const string UCS_LANG_POLISH = "polish";

    /// <summary>
    /// Hash set of supported UCS languages.
    /// </summary>
    public static readonly HashSet<string> SupportedLanguages = [
        UCS_LANG_ENGLISH,
        UCS_LANG_GERMAN,
        UCS_LANG_FRENCH,
        UCS_LANG_POLISH
    ];

    /// <summary>
    /// Represents the UTF-8 encoding, which is a character encoding capable of encoding all possible characters (code
    /// points) in Unicode.
    /// </summary>
    /// <remarks>This encoding does not emit a UTF-8 byte order mark (BOM) and will throw an exception if
    /// invalid byte sequences are encountered during decoding.</remarks>
    public static readonly Encoding UTF8 = new UTF8Encoding(encoderShouldEmitUTF8Identifier:false, throwOnInvalidBytes:true);

}
