using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Battlegrounds.Networking.LobbySystem.Local;

public class LocalLobbyMember : ILobbyMember {

    private ILobbyHandle m_handle;

    public ulong MemberID { get; }

    public string DisplayName { get; }

    public byte Role { get; private set; }

    public byte AILevel { get; private set; }

    public LobbyMemberState State { get; private set; }

    public ILobbyCompany? Company { get;private set; }

    public ILobbyHandle Handle => this.m_handle;

    public LocalLobbyMember(ILobbyHandle handle, string name, ulong mid, byte role, byte level, LobbyMemberState state, ILobbyCompany? company = null) {
        
        // Set handle
        this.m_handle = handle;

        // Set properties
        this.DisplayName = name;
        this.MemberID = mid;
        this.Role = role;
        this.AILevel = level;
        this.State = state;
        
        // Set company
        this.Company = company;
        this.Company?.SetHandle(handle);

    }

    public void ChangeCompany(ILobbyCompany? company)
        => this.Company = company;

    public void SetHandle(ILobbyHandle handle) {
        this.m_handle = handle;
        this.Company?.SetHandle(handle);
    }

}
