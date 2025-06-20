﻿namespace Battlegrounds.Gfx;

/// <summary>
/// Enum representing the type data in a <see cref="GfxResource"/>.
/// </summary>
public enum GfxResourceType {

    /// <summary>
    /// PNG data
    /// </summary>
    Png,

    /// <summary>
    /// Targa data
    /// </summary>
    Tga,

    /// <summary>
    /// Bitmap data
    /// </summary>
    Bmp,

    /// <summary>
    /// Xaml data
    /// </summary>
    Xaml,

    /// <summary>
    /// Html data
    /// </summary>
    Html

}

/// <summary>
/// Represents a GFX resource that can be opened into a <see cref="GfxResourceStream"/> for reading.
/// </summary>
public sealed class GfxResource {

    private readonly string m_id;
    private readonly int m_width, m_height;
    private readonly byte[] m_raw;
    private readonly GfxResourceType m_type;

    /// <summary>
    /// Get the width dimension of the resource.
    /// </summary>
    public int Width => this.m_width;

    /// <summary>
    /// Get the height dimension of the resource.
    /// </summary>
    public int Height => this.m_height;

    /// <summary>
    /// Get the identifier of the resource
    /// </summary>
    public string Identifier => this.m_id;

    /// <summary>
    /// Get the Gfx file format type.
    /// </summary>
    public GfxResourceType GfxType => this.m_type;

    /// <summary>
    /// Initialize a new <see cref="GfxResource"/> class with raw data.
    /// </summary>
    /// <param name="id">The identifier to use when identifying the resource.</param>
    /// <param name="rawData">The raw byte data of the GFX resource</param>
    /// <param name="w">The width of the resource.</param>
    /// <param name="h">The height of the resource.</param>
    /// <param name="type">The type of resource</param>
    public GfxResource(string id, byte[] rawData, double w, double h, GfxResourceType type) {
        this.m_id = id;
        this.m_raw = rawData;
        this.m_width = (int)w;
        this.m_height = (int)h;
        this.m_type = type;
    }

    /// <summary>
    /// Verify if the <see cref="GfxResource"/> is has identifier matching the given parameter.
    /// </summary>
    /// <param name="identifier">The identifier to check.</param>
    /// <returns>Will return <see langword="true"/> if identifier is matching resource identifier. Otherwise <see langword="false"/>.</returns>
    public bool IsResource(string identifier) => this.m_id == identifier;

    /// <summary>
    /// Opens a new <see cref="GfxResourceStream"/> for reading the data contained within the <see cref="GfxResource"/>.
    /// </summary>
    /// <returns>An open <see cref="GfxResourceStream"/>.</returns>
    public GfxResourceStream Open() {
        return new GfxResourceStream(this.m_id, this.m_raw, this.m_width, this.m_height) {
            Position = 0
        };
    }

}
