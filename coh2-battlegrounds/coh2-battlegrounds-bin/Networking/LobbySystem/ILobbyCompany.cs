namespace Battlegrounds.Networking.LobbySystem;

/// <summary>
/// 
/// </summary>
public interface ILobbyCompany : IHandleObject {

    /// <summary>
    /// 
    /// </summary>
    public bool IsAuto { get; }
    
    /// <summary>
    /// 
    /// </summary>
    public bool IsNone { get; }
    
    /// <summary>
    /// 
    /// </summary>
    public string Name { get; }
    
    /// <summary>
    /// 
    /// </summary>
    public string Army { get; }
    
    /// <summary>
    /// 
    /// </summary>
    public float Strength { get; }

    /// <summary>
    /// 
    /// </summary>
    public string Specialisation { get; }

}
