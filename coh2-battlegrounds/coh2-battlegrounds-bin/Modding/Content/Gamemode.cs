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

    public readonly struct GamemodeAdditionalOption {

        public string Type { get; }

        public string Title { get; }

        public string Desc { get; }

        public string Value { get; }

        public GamemodeOption[] Options { get; }

        public int Default { get; }

        public int Min { get; }

        public int Max { get; }

        public int Step { get; }

        public int Yes { get; }

        public int No { get; }

        [JsonConstructor]
        public GamemodeAdditionalOption(string Type, string Title, string Desc, 
            string Value, GamemodeOption[] Options, int Default, 
            int Min, int Max, int Step, int Yes, int No) {

            this.Type = Type;
            this.Title = Title;
            this.Step = Step;
            this.Desc = Desc;
            this.Default = Default;
            this.Min = Min;
            this.Max = Max;
            this.Options = Options;
            this.Value = Value;
            this.Yes = Yes;
            this.No = No;

        }

    }

    public string ID { get; }

    public string Display { get; }

    public string DisplayDesc { get; }

    public int DefaultOption { get; }

    public string[] Files { get; }

    public GamemodeOption[] Options { get; }

    public Dictionary<string, GamemodeAdditionalOption> AdditionalOptions { get; }

    public Dictionary<string, string> TeamNames { get; }

    public bool FixedPosition { get; }

    public bool Planning { get; }

    public Dictionary<string, FactionDefence[]> PlanningEntities { get; }

    [JsonConstructor]
    public Gamemode(string ID, string Display, string DisplayDesc, int DefaultOption, string[] Files, GamemodeOption[] Options,
        Dictionary<string, GamemodeAdditionalOption> AdditionalOptions, bool FixedPosition, bool Planning,
        Dictionary<string, string> TeamNames, Dictionary<string, FactionDefence[]>? PlanningEntities) {

        this.ID = ID;
        this.Display = Display;
        this.DisplayDesc = DisplayDesc;
        this.DefaultOption = DefaultOption;
        this.Files = Files;
        this.Options = Options;
        this.AdditionalOptions = AdditionalOptions;
        this.TeamNames = TeamNames;
        this.FixedPosition = FixedPosition;
        this.Planning = Planning;
        this.PlanningEntities = PlanningEntities ?? new();

    }

}
