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
    /// 
    /// </summary>
    /// <returns></returns>
    string[] GetResourceManifest();

    /// <summary>
    /// 
    /// </summary>
    /// <param name="ridx"></param>
    /// <returns></returns>
    GfxResource GetResource(int ridx);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="rid"></param>
    /// <returns></returns>
    GfxResource? GetResource(string rid);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="count"></param>
    void Allocate(int count);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="resourceID"></param>
    /// <param name="source"></param>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <param name="resourceType"></param>
    /// <returns></returns>
    int CreateResource(string resourceID, BinaryReader source, double width, double height, GfxResourceType resourceType);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="resourceIndex"></param>
    /// <param name="rawBinary"></param>
    /// <param name="resourceID"></param>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <param name="resourceType"></param>
    void CreateResource(int resourceIndex, byte[] rawBinary, string resourceID, double width, double height, GfxResourceType resourceType);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="resourceName"></param>
    /// <returns></returns>
    bool HasResource(string resourceName);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="gfxResource"></param>
    /// <returns></returns>
    int AddResource(GfxResource gfxResource);

}
