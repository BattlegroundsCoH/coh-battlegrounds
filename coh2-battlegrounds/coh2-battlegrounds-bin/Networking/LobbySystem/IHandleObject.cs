using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Battlegrounds.Networking.LobbySystem;

/// <summary>
/// 
/// </summary>
public interface IHandleObject {

    /// <summary>
    /// 
    /// </summary>
    [JsonIgnore]
    public ILobbyHandle Handle { get; }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="handle"></param>
    [MemberNotNull(nameof(Handle))]
    void SetHandle(ILobbyHandle handle);

}
