using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Battlegrounds.Modding.Content;

public readonly struct Gamemode {

    public readonly struct GamemodeOption {

        public string LocStr { get; }

        public int Value { get; }

        [JsonConstructor]
        public GamemodeOption(string LocStr, int Value) {
            this.LocStr = LocStr;
            this.Value = Value;
        }
    }

    public readonly struct GamemodePlanningEntity {
        public string EBP { get; }
        [JsonConstructor]
        public GamemodePlanningEntity(string EBP) {
            this.EBP = EBP;
        }

    }

    public string ID { get; }

    public string Display { get; }

    public string DisplayDesc { get; }

    public int DefaultOption { get; }

    public string[] Files { get; }

    public GamemodeOption[] Options { get; }

    public bool FixedPosition { get; }

    public bool Planning { get; }

    public Dictionary<string, GamemodePlanningEntity[]> PlanningEntities { get; }

    [JsonConstructor]
    public Gamemode(string ID, string Display, string DisplayDesc, int DefaultOption, string[] Files, GamemodeOption[] Options, 
        bool FixedPosition, bool Planning, Dictionary<string, GamemodePlanningEntity[]>? PlanningEntities) {

        this.ID = ID;
        this.Display = Display;
        this.DisplayDesc = DisplayDesc;
        this.DefaultOption = DefaultOption;
        this.Files = Files;
        this.Options = Options;
        this.FixedPosition = FixedPosition;
        this.Planning = Planning;
        this.PlanningEntities = PlanningEntities ?? new();

    }

}
