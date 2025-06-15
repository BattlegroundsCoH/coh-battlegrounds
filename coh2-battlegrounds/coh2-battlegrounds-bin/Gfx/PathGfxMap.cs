using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Battlegrounds.Errors.Common;
using Battlegrounds.Util;

namespace Battlegrounds.Gfx;

/// <summary>
/// A <see cref="IGfxMap"/> implementation using an internal <see cref="SearchTree{T}"/> to store identifiers.
/// </summary>
public sealed class PathGfxMap : IGfxMap {

    private readonly SearchTree<GfxResource> tree;
    private readonly Dictionary<string, int> indices;

    /// <inheritdoc/>
    public GfxVersion GfxVersion => GfxVersion.V3;

    /// <inheritdoc/>
    public int Capacity => tree.Count + 1;

    /// <inheritdoc/>
    public int Count => tree.Count;

    /// <summary>
    /// Get or set the path delimiter to use when constructing the tree.
    /// </summary>
    public char Delimiter { // TODO: Add (freeze) property so this is not accidentally changed while data has been loaded)
        get => tree.PathDelimiter;
        set => tree.PathDelimiter = value;
    }

    /// <summary>
    /// Initialise a new and empty <see cref="PathGfxMap"/> instance.
    /// </summary>
    public PathGfxMap() {
        tree = new SearchTree<GfxResource>();
        indices = new Dictionary<string, int>();
    }

    /// <inheritdoc/>
    public int AddResource(GfxResource gfxResource) {
        int ridx = tree.Count;
        if (tree.Contains(gfxResource.Identifier)) {
            throw new InvalidOperationException("Cannot insert duplicate resources: identifier already found");
        }
        tree.Add(gfxResource.Identifier, gfxResource);
        indices[gfxResource.Identifier] = ridx;
        return ridx;
    }

    /// <inheritdoc/>
    public void Allocate(int count) {}

    /// <inheritdoc/>
    public int CreateResource(string resourceID, BinaryReader source, double width, double height, GfxResourceType resourceType) {
        byte[] data = source.ReadBytes((int)(source.BaseStream.Length - source.BaseStream.Position));
        GfxResource resource = new GfxResource(resourceID, data, width, height, resourceType);
        return AddResource(resource);
    }

    /// <inheritdoc/>
    public void CreateResource(int resourceIndex, byte[] rawBinary, string resourceID, double width, double height, GfxResourceType resourceType) {
        GfxResource resource = new GfxResource(resourceID, rawBinary, width, height, resourceType);
        AddResource(resource);
    }

    /// <inheritdoc/>
    public IEnumerator<GfxResource> GetEnumerator() => tree.GetEnumerator();

    /// <inheritdoc/>
    public GfxResource GetResource(int ridx) {
        string identifier = indices.FirstOrDefault(x => x.Value == ridx).Key;
        if (string.IsNullOrEmpty(identifier)) {
            throw new ObjectNotFoundException("Failed finding resource with index " + ridx);
        }
        if (tree.Lookup(identifier, out GfxResource? resource)) {
            return resource;
        }
        throw new ObjectNotFoundException("Failed finding resource with identifier " + identifier);
    }

    /// <inheritdoc/>
    public GfxResource? GetResource(string rid) {
        if (tree.Lookup(rid, out GfxResource? resource)) {
            return resource;
        }
        throw new ObjectNotFoundException("Failed finding resource with identifier " + rid);
    }

    /// <inheritdoc/>
    public string[] GetResourceManifest() => tree.Keys.ToArray();

    /// <inheritdoc/>
    public bool HasResource(string resourceName) => tree.Contains(resourceName);

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <summary>
    /// Get the map as a <see cref="SearchTree{GfxResource}"/>.
    /// </summary>
    /// <returns>The map as a search tree.</returns>
    public SearchTree<GfxResource> AsSearchTree() => tree;

}
