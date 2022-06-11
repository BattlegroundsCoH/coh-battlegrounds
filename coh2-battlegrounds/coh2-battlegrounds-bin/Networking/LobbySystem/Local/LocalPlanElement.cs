using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Battlegrounds.Game;

namespace Battlegrounds.Networking.LobbySystem.Local;

/// <summary>
/// 
/// </summary>
public class LocalPlanElement : ILobbyPlanElement {

    /// <summary>
    /// 
    /// </summary>
    public int ElementId { get; }

    /// <summary>
    /// 
    /// </summary>
    public ulong ElementOwnerId { get; }

    /// <summary>
    /// 
    /// </summary>
    public GamePosition SpawnPosition { get; }

    /// <summary>
    /// 
    /// </summary>
    public GamePosition LookatPosition { get; }

    /// <summary>
    /// 
    /// </summary>
    public string Blueprint { get; }

    /// <summary>
    /// 
    /// </summary>
    public ushort CompanyId { get; }

    /// <summary>
    /// 
    /// </summary>
    public bool IsEntity { get; }

    /// <summary>
    /// 
    /// </summary>
    public bool IsDirectional { get; }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="eid"></param>
    /// <param name="owner"></param>
    /// <param name="blueprint"></param>
    /// <param name="spawn"></param>
    /// <param name="lookat"></param>
    /// <param name="directional"></param>
    public LocalPlanElement(int eid, ulong owner, string blueprint, GamePosition spawn, GamePosition? lookat = null, bool directional = true) {
        this.ElementId = eid;
        this.ElementOwnerId = owner;
        this.SpawnPosition = spawn;
        this.LookatPosition = lookat ?? GamePosition.Naught;
        this.Blueprint = blueprint;
        this.CompanyId = ushort.MaxValue;
        this.IsEntity = true;
        this.IsDirectional = directional;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="eid"></param>
    /// <param name="owner"></param>
    /// <param name="blueprint"></param>
    /// <param name="companyId"></param>
    /// <param name="spawn"></param>
    /// <param name="lookat"></param>
    /// <param name="directional"></param>
    public LocalPlanElement(int eid, ulong owner, string blueprint, ushort companyId, GamePosition spawn, GamePosition? lookat = null) {
        this.ElementId = eid;
        this.ElementOwnerId = owner;
        this.SpawnPosition = spawn;
        this.LookatPosition = lookat ?? GamePosition.Naught;
        this.Blueprint = blueprint;
        this.CompanyId = companyId;
        this.IsDirectional = lookat is not null;
    }

}
