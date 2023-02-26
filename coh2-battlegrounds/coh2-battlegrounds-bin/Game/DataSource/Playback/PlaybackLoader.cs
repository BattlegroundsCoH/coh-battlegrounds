using Battlegrounds.Game.DataSource.Playback.CoH2;
using Battlegrounds.Game.DataSource.Playback.CoH3;

using System;

namespace Battlegrounds.Game.DataSource.Playback;

/// <summary>
/// 
/// </summary>
public class PlaybackLoader {

    /// <summary>
    /// The path to the latest replay file for CoH2.
    /// </summary>
    public static readonly string LATEST_COH2_REPLAY_FILE = $"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}\\my games\\company of heroes 2\\playback\\temp.rec";

    /// <summary>
    /// The path to the latest replay file for CoH3.
    /// </summary>
    public static readonly string LATEST_COH3_REPLAY_FILE = $"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}\\my games\\company of heroes 3\\playback\\temp.rec";

    /// <summary>
    /// 
    /// </summary>
    /// <param name="filepath"></param>
    /// <param name="game"></param>
    /// <returns></returns>
    /// <exception cref="NotSupportedException"></exception>
    public virtual IPlayback? LoadPlayback(string filepath, GameCase game) {
        var playback = game switch {
            GameCase.CompanyOfHeroes2 => GetCoH2Playback(filepath),
            GameCase.CompanyOfHeroes3 => GetCoH3Playback(filepath),
            _ => throw new NotSupportedException()
        };
        if (!playback.LoadPlayback()) {
            return null;
        }
        return playback;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="filepath"></param>
    /// <returns></returns>
    public virtual IPlayback GetCoH2Playback(string filepath) => new CoH2Playback(filepath);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="filepath"></param>
    /// <returns></returns>
    public virtual IPlayback GetCoH3Playback(string filepath) => new CoH3Playback(filepath);

}
