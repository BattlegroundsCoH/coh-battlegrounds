using System;
using System.IO;

namespace Battlegrounds.Util;

/// <summary>
/// Represents a reader for a string content.
/// </summary>
public sealed class StringContentReader {

    private readonly string _text;
    private int _pos = 0;

    /// <summary>
    /// Initializes a new instance of the <see cref="StringContentReader"/> class with the specified source string.
    /// </summary>
    /// <param name="source">The source string to read from.</param>
    /// <exception cref="ArgumentNullException"/>
    public StringContentReader(string source) {
        if (source is null) {
            throw new ArgumentNullException(nameof(source), "Input source is null.");
        }
        this._text = source;
    }

    /// <summary>
    /// Sets the position of the reader to the specified position.
    /// </summary>
    /// <param name="pos">The new position of the reader.</param>
    /// <returns>The current <see cref="StringContentReader"/> instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException"/>
    public StringContentReader Seek(int pos) {
        if (pos < 0 || pos > _text.Length) {
            throw new ArgumentOutOfRangeException(nameof(pos), "Position is out of range.");
        }
        _pos = pos; return this;
    }

    /// <summary>
    /// Sets the position of the reader to the specified position, relative to the specified origin.
    /// </summary>
    /// <param name="origin">The origin from which to seek.</param>
    /// <param name="pos">The new position of the reader.</param>
    /// <returns>The current <see cref="StringContentReader"/> instance.</returns>
    public StringContentReader Seek(SeekOrigin origin, int pos) {
        switch (origin) {
            case SeekOrigin.Begin:
                _pos = pos;
                break;
            case SeekOrigin.Current:
                _pos += pos;
                break;
                case SeekOrigin.End:
                    _pos = _text.Length - pos;
                break;
            default: break;
        }
        if (_pos < 0) {
            _pos = 0;
        }
        if (_pos > _text.Length) {
            _pos = _text.Length;
        }
        return this;
    }

    /// <summary>
    /// Reads the next substring from the current position of the reader up to the specified character.
    /// </summary>
    /// <param name="c">The character to read up to.</param>
    /// <returns>The next substring from the current position of the reader up to the specified character.</returns>
    /// <exception cref="ArgumentNullException"/>
    public string ReadUntil(char c) {
        if (c == '\0') {
            throw new ArgumentNullException(nameof(c), "Character is null.");
        }
        int next = _text.IndexOf(c, _pos + 1);
        if (next == -1) {
            return string.Empty;
        }
        string val = _text[(_pos + 1) .. next];
        _pos = next;
        return val;
    }

    /// <summary>
    /// Reads the next substring from the current position of the reader up to the specified character and returns the substring as an output parameter.
    /// </summary>
    /// <param name="c">The character to read up to.</param>
    /// <param name="val">The next substring from the current position of the reader up to the specified character.</param>
    /// <returns>The current <see cref="StringContentReader"/> instance.</returns>
    /// <exception cref="ArgumentNullException"/>
    public StringContentReader ReadUntil(char c, out string val) {
        try {
            val = ReadUntil(c);
        } catch (ArgumentNullException) {
            throw;
        }
        return this;
    }

    /// <summary>
    /// Reads the remainder of the source string from the current position of the reader to the end of the string.
    /// </summary>
    /// <returns>The remainder of the source string from the current position of the reader to the end of the string.</returns>
    public string ReadToEnd() {
        string v = _text[_pos..];
        _pos = _text.Length;
        return v;
    }

    /// <summary>
    /// Implicitly converts a string to a <see cref="StringContentReader"/> instance.
    /// </summary>
    /// <param name="text">The string to convert to a <see cref="StringContentReader"/> instance.</param>
    /// <returns>A new <see cref="StringContentReader"/> instance.</returns>
    public static implicit operator StringContentReader(string text) => new StringContentReader(text);

}
