namespace Battlegrounds.Game.Database; 

public enum ServerItemType {
    Undefined = 0,
    Skin = 1,
    Decal = 2,
    Commander = 3,
    VictoryStrike = 4,
    Buletin = 5,
    Faceplate = 6,
}

public readonly struct ServerItem {

    public static ServerItem None => new ServerItem(ServerItemType.Undefined, uint.MaxValue);

    public uint ServerID { get; }

    public ServerItemType Type { get; }

    public ServerItem(ServerItemType type, uint id) {
        this.Type = type;
        this.ServerID = id;
    }

    public override string ToString() {
        if (this.Type == ServerItemType.Undefined && this.ServerID == uint.MaxValue) {
            return "None";
        } else {
            return $"{this.ServerID} ({this.Type})";
        }
    }

}
