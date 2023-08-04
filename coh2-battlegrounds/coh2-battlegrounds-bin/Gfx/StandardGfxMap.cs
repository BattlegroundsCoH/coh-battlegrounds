using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Battlegrounds.Util;

namespace Battlegrounds.Gfx;

/// <summary>
/// Representation of a map overview of GFX resources.
/// </summary>
public sealed class StandardGfxMap : IGfxMap {

    private GfxResource[] m_gfxMapResources;
    private string[] m_gfxMapResourceIdentifiers;

    /// <inheritdoc/>
    public int Capacity => this.m_gfxMapResources.Length;

    /// <inheritdoc/>
    public int Count => this.m_gfxMapResources.Count(x => x is not null);

    /// <inheritdoc/>
    public GfxVersion GfxVersion { get; init; }

    /// <summary>
    /// Initialise a new <see cref="StandardGfxMap"/> instance capable of holding <paramref name="n"/> resource elements.
    /// </summary>
    /// <param name="n">The amount of elements in the map.</param>
    public StandardGfxMap(int n) {
        this.GfxVersion = GfxVersion.V2;
        this.m_gfxMapResources = new GfxResource[n];
        this.m_gfxMapResourceIdentifiers = new string[n];
    }

    /// <inheritdoc/>
    public GfxResource? GetResource(string identifier) => this.m_gfxMapResources.FirstOrDefault(x => x.IsResource(identifier));

    /// <inheritdoc/>
    public GfxResource GetResource(int resourceIndex) => this.m_gfxMapResources[resourceIndex];

    /// <inheritdoc/>
    public void Allocate(int count) {

        // Copy into new resource array
        GfxResource[] gfxResources = new GfxResource[this.Capacity + count];
        Array.Copy(this.m_gfxMapResources, gfxResources, this.Capacity);

        // Set
        this.m_gfxMapResources = gfxResources;

        // Copy into new string array
        string[] identifiers = new string[gfxResources.Length];
        Array.Copy(this.m_gfxMapResourceIdentifiers, identifiers, this.m_gfxMapResourceIdentifiers.Length);

        // Set
        this.m_gfxMapResourceIdentifiers = identifiers;

    }

    /// <inheritdoc/>
    public void CreateResource(int resourceIndex, byte[] rawBinary, string resourceID, double width, double height, GfxResourceType resourceType) {
        if (resourceIndex > this.m_gfxMapResources.Length) {
            throw new ArgumentOutOfRangeException(nameof(resourceIndex), resourceIndex, $"Resource index out of range (Max resource count = {this.m_gfxMapResources.Length})");
        }
        this.m_gfxMapResources[resourceIndex] = new GfxResource(resourceID, rawBinary, width, height, resourceType);
        this.m_gfxMapResourceIdentifiers[resourceIndex] = resourceID;
    }

    /// <inheritdoc/>
    public int CreateResource(string resourceID, BinaryReader source, double width, double height, GfxResourceType resourceType) {

        // Allocate space if none is available
        if (this.Count == this.Capacity)
            this.Allocate(1);

        // Grab resource id
        int resourceId = this.Count;

        // Read bytes
        byte[] data = source.ReadToEnd();

        // Create
        this.CreateResource(resourceId, data, resourceID, width, height, resourceType);

        // Return it
        return resourceId;

    }

    /// <inheritdoc/>
    public bool HasResource(string resourceName) {
        return Array.IndexOf(this.m_gfxMapResourceIdentifiers, resourceName) != -1;
    }

    /// <inheritdoc/>
    public int AddResource(GfxResource gfxResource) {

        // Allocate space if none is available
        if (this.Count == this.Capacity)
            this.Allocate(1);

        // Grab resource id
        int resourceId = this.Count;

        // Assign
        this.m_gfxMapResourceIdentifiers[resourceId] = gfxResource.Identifier;
        this.m_gfxMapResources[resourceId] = gfxResource;

        // Return the Id of the resource
        return resourceId;

    }

    /// <inheritdoc/>
    public string[] GetResourceManifest() => this.m_gfxMapResourceIdentifiers;

    /// <inheritdoc/>
    public IEnumerator<GfxResource> GetEnumerator() => (IEnumerator<GfxResource>)this.m_gfxMapResources.GetEnumerator();

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

}
