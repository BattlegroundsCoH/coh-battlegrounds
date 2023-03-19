using Battlegrounds.Game.DataSource.Playback.CoH2;
using Battlegrounds.Game.DataSource.Playback.CoH3;

using System;

namespace Battlegrounds.Game.DataSource.Playback;

/// <summary>
/// Class responsible for loading a playback file based on the specified game case.
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
    /// Loads playback data from the specified file path and returns an instance of the <see cref="IPlayback"/> interface.
    /// </summary>
    /// <param name="filepath">The path of the playback file to load.</param>
    /// <param name="game">An enumeration value representing the version of the game (either Company of Heroes 2 or Company of Heroes 3).</param>
    /// <returns>An instance of the <see cref="IPlayback"/> interface, or <c>null</c> if the playback data could not be loaded.</returns>
    /// <exception cref="NotSupportedException">Thrown if the specified game version is not supported.</exception>
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
    /// Returns a new instance of the <see cref="CoH2Playback"/> class for loading playback data from Company of Heroes 2.
    /// </summary>
    /// <param name="filepath">The path of the playback file to load.</param>
    /// <returns>A new instance of the <see cref="CoH2Playback"/> class.</returns>
    public virtual IPlayback GetCoH2Playback(string filepath) => new CoH2Playback(filepath);

    /// <summary>
    /// Returns a new instance of the <see cref="CoH3Playback"/> class for loading playback data from Company of Heroes 3.
    /// </summary>
    /// <param name="filepath">The path of the playback file to load.</param>
    /// <returns>A new instance of the <see cref="CoH3Playback"/> class.</returns>
    public virtual IPlayback GetCoH3Playback(string filepath) => 
        BattlegroundsInstance.UseLightCoH3PlaybackLoader ? GetCoH3LightPlayback(filepath) : new CoH3Playback(filepath);

    /// <summary>
    /// Returns a new instance of the <see cref="CoH3PlaybackLight"/> class for loading playback data from Company of Heroes 3, with a less correct reading method than the standard <see cref="CoH3Playback"/> class.
    /// </summary>
    /// <param name="filepath">The path of the playback file to load.</param>
    /// <returns>A new instance of the <see cref="CoH3PlaybackLight"/> class.</returns>
    public virtual IPlayback GetCoH3LightPlayback(string filepath) => new CoH3PlaybackLight(filepath);

}
