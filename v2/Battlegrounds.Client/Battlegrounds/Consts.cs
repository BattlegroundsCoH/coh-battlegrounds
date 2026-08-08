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
    /// How often the dashboard card and the news page re-request the feed while they are on screen.
    /// </summary>
    /// <remarks>Comfortably longer than the API's own sixty-second edge cache on the news routes, so a
    /// poll is usually answered by Cloudflare rather than by Postgres, and short enough that a freshly
    /// published entry appears while someone is still looking at the page.
    /// <para>Both pages stop polling the moment they are hidden — switching page, opening a lobby or
    /// signing out — so this is an interval for a visible page, not a background heartbeat.</para></remarks>
    public static readonly TimeSpan NewsRefreshInterval = TimeSpan.FromMinutes(2);

    /// <summary>
    /// Represents the UTF-8 encoding, which is a character encoding capable of encoding all possible characters (code
    /// points) in Unicode.
    /// </summary>
    /// <remarks>This encoding does not emit a UTF-8 byte order mark (BOM) and will throw an exception if
    /// invalid byte sequences are encountered during decoding.</remarks>
    public static readonly Encoding UTF8 = new UTF8Encoding(encoderShouldEmitUTF8Identifier:false, throwOnInvalidBytes:true);

}
