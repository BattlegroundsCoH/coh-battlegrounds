using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json.Serialization;

using Battlegrounds.Functional;
using Battlegrounds.Lua;

namespace Battlegrounds.Game.Database;

/// <summary>
/// The theatre of war a scenario is taking place in.
/// </summary>
public enum ScenarioTheatre {

    /// <summary>
    /// Axis vs Soviets
    /// </summary>
    EasternFront,

    /// <summary>
    /// Axis vs UKF & USF
    /// </summary>
    WesternFront,

    /// <summary>
    /// Axis vs Allies (Germany)
    /// </summary>
    SharedFront,

}

/// <summary>
/// Represents a scenario. Implements <see cref="IJsonObject"/>. This class cannot be inherited.
/// </summary>
public sealed class Scenario {

    public const string INVALID_SGA = "INVALID SGA";

    /// <summary>
    /// The text-name for the <see cref="Scenario"/>.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// The description text for the <see cref="Scenario"/>.
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// The relative filename to the "mp" scenarios folder.
    /// </summary>
    public string RelativeFilename { get; set; }

    /// <summary>
    /// Name of the sga file containing this <see cref="Scenario"/>.
    /// </summary>
    public string SgaName { get; set; }

    /// <summary>
    /// The max amount of players who can play on this map.
    /// </summary>
    public byte MaxPlayers { get; set; }

    /// <summary>
    /// The <see cref="ScenarioTheatre"/> this map takes place in.
    /// </summary>
    public ScenarioTheatre Theatre { get; set; }

    /// <summary>
    /// Can this <see cref="Scenario"/> be considered a winter map.
    /// </summary>
    public bool IsWintermap { get; set; }

    /// <summary>
    /// Get if the given scenario is visible in the lobby.
    /// </summary>
    public bool IsVisibleInLobby { get; set; }

    /// <summary>
    /// Get if the <see cref="Scenario"/> is a workshop map.
    /// </summary>
    public bool IsWorkshopMap => this.SgaName is not "MPScenarios" and not "MPXP1Scenarios";

    /// <summary>
    /// The <see cref="Wincondition"/> instances designed for this <see cref="Scenario"/>. Empty list means all <see cref="Wincondition"/> instances can be used.
    /// </summary>
    public List<string> Gamemodes { get; set; }

    /// <summary>
    /// Get if the scenario has a valid info or options file.
    /// </summary>
    [JsonIgnore]
    public bool HasValidInfoOrOptionsFile { get; }

    public string ToJsonReference() => this.RelativeFilename;

    public Scenario() {
        this.SgaName = INVALID_SGA;
        this.Gamemodes = new List<string>();
        this.HasValidInfoOrOptionsFile = true; // Under the assumption it's being set
        this.Name = "Unknwon";
        this.Description = "Undefined";
        this.RelativeFilename = "INVALID_FILENAME";
    }

    /// <summary>
    /// New <see cref="Scenario"/> instance with data from either an infor or options file.
    /// </summary>
    /// <param name="infofile">The path to the info file.</param>
    /// <param name="optionsfile">The path to the options file</param>
    /// <exception cref="ArgumentNullException"/>
    public Scenario(string infofile, string optionsfile) {

        // Make sure infofile is not null
        if (infofile is null) {
            throw new ArgumentNullException(nameof(infofile), "Info filepath cannot be null");
        }

        // Make sure optionsfile is not null
        if (optionsfile is null) {
            throw new ArgumentNullException(nameof(optionsfile), "Options filepath cannot be null");
        }

        this.RelativeFilename = Path.GetFileNameWithoutExtension(infofile);
        this.Gamemodes = new();
        this.SgaName = string.Empty;

        LuaState scenarioState = new();
        LuaVM.DoFile(scenarioState, infofile);
        LuaVM.DoFile(scenarioState, optionsfile);

        // Get header info (and if false, bail)
        if (scenarioState._G["HeaderInfo"] is not LuaTable headerInfo) {
            throw new Exception("Invalid scenario header 'HeaderInfo' not found!");
        }

        this.Name = headerInfo["scenarioname"].Str();
        this.Description = headerInfo["scenariodescription"].Str();
        this.MaxPlayers = (byte)(headerInfo["maxplayers"] as LuaNumber ?? new LuaNumber(0));

        int battlefront = (int)(LuaNumber)headerInfo["scenario_battlefront"].IfTrue(x => x is LuaNumber).ThenDo(x => x as LuaNumber).OrDefaultTo(() => new LuaNumber(2));
        this.Theatre = battlefront == 2 ? ScenarioTheatre.EasternFront : battlefront == 5 ? ScenarioTheatre.WesternFront : ScenarioTheatre.SharedFront;

        // Get the skins table (apperantly both are accepted...)
        if (headerInfo["default_skins"] is not LuaTable skins) {
            skins = headerInfo["default_skin"] as LuaTable ?? new();
        }

        this.IsWintermap = skins?.Contains("winter") ?? false;
        this.IsVisibleInLobby = (scenarioState._G["visible_in_lobby"] as LuaBool)?.IsTrue ?? true;

        this.HasValidInfoOrOptionsFile = true;

    }
    public override string ToString() => this.Name;

}

