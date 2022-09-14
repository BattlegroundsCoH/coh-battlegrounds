using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Battlegrounds.Networking.LobbySystem;

/// <summary>
/// 
/// </summary>
public interface IHandleObject<T> {

    /// <summary>
    /// 
    /// </summary>
    [JsonIgnore]
    public T Handle { get; }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="handle"></param>
    [MemberNotNull(nameof(Handle))]
    void SetHandle(T handle);

}
