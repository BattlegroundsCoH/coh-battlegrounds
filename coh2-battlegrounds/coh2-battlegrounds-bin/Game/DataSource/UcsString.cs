using System;

namespace Battlegrounds.Game.DataSource;

/// <summary>
/// Struct representing a reference to a string value in a <see cref="UcsFile"/>.
/// </summary>
public readonly struct UcsString {

    private static readonly UcsFile ucsFile = new UcsFile("0");

    public static readonly UcsString None = new(ucsFile, 0);

    private readonly UcsFile m_ucs;
    private readonly uint m_key;

    /// <summary>
    /// Initialise a new <see cref="UcsString"/> instance referencing the string identified by <paramref name="key"/> in <paramref name="file"/>.
    /// </summary>
    /// <param name="file">The <see cref="UcsFile"/> to reference.</param>
    /// <param name="key">The key of the referenced string.</param>
    public UcsString(UcsFile file, uint key) {

        // Validate file
        if (file is null) {
            throw new ArgumentNullException(nameof(file), "UCS file cannot be null");
        }

        // Set fields
        this.m_ucs = file;
        this.m_key = key;

    }

    /// <summary>
    /// Set the value of the <see cref="UcsString"/>.
    /// </summary>
    /// <param name="value">The new string value to update.</param>
    public void SetValue(string value)
        => this.m_ucs.UpdateString(this.m_key, value);

    /// <summary>
    /// Get the referenced string from a <see cref="UcsString"/> value.
    /// </summary>
    /// <param name="str">The <see cref="UcsString"/> to get.</param>
    public static implicit operator string(UcsString str)
        => str.m_ucs[str.m_key];

    /// <summary>
    /// Returns the referenced locale string.
    /// </summary>
    /// <returns>The referenced string or base error message if not found.</returns>
    public override string ToString() => this.m_ucs[this.m_key];

    /// <summary>
    /// Indicates whether a specified <see cref="UcsString"/> is null or the empty string or if value is "0".
    /// </summary>
    /// <param name="ucs">The UCS string to check.</param>
    /// <returns><see langword="true"/> if <paramref name="str"/> is null or the empty string or if "0"; Otherwise <see langword="false"/>.</returns>
    public static bool IsNullOrEmpty(UcsString? ucs)
        => !(ucs is UcsString str && str.m_key is not 0);

    /// <summary>
    /// Indicates whether a specified <see cref="string"/> is null or the empty string or if key value is "0".
    /// </summary>
    /// <param name="str">The string value to check</param>
    /// <returns><see langword="true"/> if <paramref name="str"/> is null or the empty string or if "0"; Otherwise <see langword="false"/>.</returns>
    public static bool IsNullOrEmpty(string str)
        => string.IsNullOrEmpty(str) || str is "0";

    /// <summary>
    /// Get a temporary localised key (this will prefix 'LOC:')
    /// </summary>
    /// <remarks>
    /// Any calls to this method should be phased out ASAP.
    /// </remarks>
    /// <param name="loc">The localised content to display.</param>
    /// <returns>A <see cref="UcsString"/> with string value of <paramref name="loc"/>.</returns>
    public static UcsString CreateLocString(string loc)
        => ucsFile.CreateKey((uint)ucsFile.KeyCount, loc).GetRef((uint)(ucsFile.KeyCount - 1));

}
