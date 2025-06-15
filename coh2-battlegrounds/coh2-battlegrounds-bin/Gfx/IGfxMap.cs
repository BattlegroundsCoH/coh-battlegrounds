using System.Collections.Generic;
using System.IO;

namespace Battlegrounds.Gfx;

/// <summary>
/// Interface representing a map of GFX resources.
/// </summary>
public interface IGfxMap : IEnumerable<GfxResource> {

    /// <summary>
    /// Get or initialsie the binary version of the <see cref="IGfxMap"/>.
    /// </summary>
    GfxVersion GfxVersion { get; }

    /// <summary>
    /// Get the available space in the <see cref="IGfxMap"/> instance.
    /// </summary>
    int Capacity { get; }

    /// <summary>
    /// Get the current amount of resources registered within the <see cref="IGfxMap"/>.
    /// </summary>
    int Count { get; }

    /// <summary>
    /// Get the manifest of resource identifiers in the <see cref="IGfxMap"/>.
    /// </summary>
    /// <returns>An array of strings representing the resource identifiers.</returns>
    string[] GetResourceManifest();

    /// <summary>
    /// Get a specific resource by index from the <see cref="IGfxMap"/>.
    /// </summary>
    /// <param name="ridx">The index of the resource to retrieve.</param>
    /// <returns>A GfxResource instance corresponding to the specified index.</returns>
    GfxResource GetResource(int ridx);

    /// <summary>
    /// Get a specific resource by its identifier from the <see cref="IGfxMap"/>.
    /// </summary>
    /// <param name="rid">The identifier of the resource to retrieve.</param>
    /// <returns>A nullable GfxResource instance corresponding to the specified identifier, or null if not found.</returns>
    GfxResource? GetResource(string rid);

    /// <summary>
    /// Allocate space for a specified number of resources within the <see cref="IGfxMap"/>.
    /// </summary>
    /// <param name="count">The number of resources to allocate space for.</param>
    void Allocate(int count);

    /// <summary>
    /// Create a new resource in the <see cref="IGfxMap"/> with the specified parameters.
    /// </summary>
    /// <param name="resourceID">The unique identifier for the new resource.</param>
    /// <param name="source">The BinaryReader containing the source data for the new resource.</param>
    /// <param name="width">The width of the new resource.</param>
    /// <param name="height">The height of the new resource.</param>
    /// <param name="resourceType">The type of the new resource.</param>
    /// <returns>The index of the newly created resource in the <see cref="IGfxMap"/>.</returns>
    int CreateResource(string resourceID, BinaryReader source, double width, double height, GfxResourceType resourceType);

    /// <summary>
    /// Create a new resource at the specified index in the <see cref="IGfxMap"/> with the provided parameters.
    /// </summary>
    /// <param name="resourceIndex">The index at which to create the new resource.</param>
    /// <param name="rawBinary">A byte array containing the raw binary data for the new resource.</param>
    /// <param name="resourceID">The unique identifier for the new resource.</param>
    /// <param name="width">The width of the new resource.</param>
    /// <param name="height">The height of the new resource.</param>
    /// <param name="resourceType">The type of the new resource.</param>
    void CreateResource(int resourceIndex, byte[] rawBinary, string resourceID, double width, double height, GfxResourceType resourceType);

    /// <summary>
    /// Check if the <see cref="IGfxMap"/> contains a resource with the specified name.
    /// </summary>
    /// <param name="resourceName">The name of the resource to check for.</param>
    /// <returns>True if the resource exists in the <see cref="IGfxMap"/>; otherwise, false.</returns>
    bool HasResource(string resourceName);

    /// <summary>
    /// Add a new GfxResource to the <see cref="IGfxMap"/>.
    /// </summary>
    /// <param name="gfxResource">The GfxResource instance to add.</param>
    /// <returns>The index of the added resource in the <see cref="IGfxMap"/>.</returns>
    int AddResource(GfxResource gfxResource);

}
