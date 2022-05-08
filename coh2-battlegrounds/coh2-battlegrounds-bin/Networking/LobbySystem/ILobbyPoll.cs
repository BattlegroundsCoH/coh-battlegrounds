using System.Collections.Generic;

namespace Battlegrounds.Networking.LobbySystem;

public interface ILobbyPoll {

    public Dictionary<ulong, bool> Responses { get; }
    
    public uint ResponseId { get; }

    public string PollId { get; }

}

public readonly struct LobbyPollResults {

    public byte Yays { get; }

    public byte Nays { get; }

    public bool YayMajority => this.Yays > this.Nays;

    public bool TimedOut { get; }

    public LobbyPollResults(byte y, byte n, bool t) {
        this.Yays = y;
        this.Nays = n;
        this.TimedOut = t;
    }

    public static implicit operator bool(LobbyPollResults results) => results.YayMajority;

}
