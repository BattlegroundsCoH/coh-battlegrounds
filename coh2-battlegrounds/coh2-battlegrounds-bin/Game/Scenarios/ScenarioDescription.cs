﻿namespace Battlegrounds.Game.Scenarios;

/// <summary>
/// 
/// </summary>
public sealed class ScenarioDescription {

    /// <summary>
    /// 
    /// </summary>
    public int Width { get; }

    /// <summary>
    /// 
    /// </summary>
    public int Length { get; }

    /// <summary>
    /// 
    /// </summary>
    public string File { get; }

    /// <summary>
    /// 
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// 
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public string Abbreviation { get; set; }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="file"></param>
    /// <param name="name"></param>
    /// <param name="width"></param>
    /// <param name="length"></param>
    public ScenarioDescription(string file, string name, int width, int length) {
        File = file;
        Name = name;
        Width = width;
        Length = length;
        Description = "";
        Abbreviation = "";
    }

}
