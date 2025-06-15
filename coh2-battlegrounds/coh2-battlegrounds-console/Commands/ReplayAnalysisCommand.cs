using Battlegrounds.Game.DataSource.Playback;
using Battlegrounds.Game.Match.Data;
using Battlegrounds.Game.Match;
using Battlegrounds.Game;
using Battlegrounds.Logging;

using System.IO;

namespace Battlegrounds.Developer.Commands;

public class ReplayAnalysisCommand : Command {

    private static readonly Logger logger = Logger.CreateLogger();

    public static readonly Argument<string> PATH =
        new Argument<string>("-f", "Specifies the playback file to analyse.", File.Exists("temp.rec") ? "temp.rec" : PlaybackLoader.LATEST_COH2_REPLAY_FILE);
    
    public static readonly Argument<bool> JSON = new Argument<bool>("-json", "Specifies the analysis should be saved to a json file.", false);

    public static readonly Argument<bool> COH3 = new Argument<bool>("-coh3", "Specifies if the analysis is of a CoH3 playback file.", false);

    public ReplayAnalysisCommand() : base("playback", "Runs an analysis of the most recent replay file (or otherwise specified file).", PATH, JSON, COH3) { }

    public override void Execute(CommandArgumentList argumentList) {

        var target = argumentList.GetValue(PATH);
        var compile_json = argumentList.GetValue(JSON);
        var gameTarget = argumentList.GetValue(COH3) ? GameCase.CompanyOfHeroes3 : GameCase.CompanyOfHeroes2;

        // Fix target if coh3 flag and default was coh2
        if (target == PlaybackLoader.LATEST_COH2_REPLAY_FILE && gameTarget is GameCase.CompanyOfHeroes3) {
            target = PlaybackLoader.LATEST_COH3_REPLAY_FILE;
        }

        // Load program
        Program.LoadBGAndProceed();

        // Create playback loader
        var loader = new PlaybackLoader();

        logger.Info("Parsing latest replay file");
        var playbackFile = loader.LoadPlayback(target, gameTarget);
        if (playbackFile is null) {
            logger.Info("Failed reading replay file");
            return;
        }

        logger.Info($"Partial: {playbackFile.IsPartial}");

        ReplayMatchData playback = new ReplayMatchData(new NullSession());
        playback.SetReplayFile(playbackFile);
        if (!playback.ParseMatchData()) {
            logger.Info("Failed to parse match data");
            return;
        }

        if (compile_json) {
            logger.Info("Compiling to json playback");
            JsonPlayback events = new JsonPlayback(playback);
            File.WriteAllText("playback.json", events.ToJson());
            logger.Info("Saved to replay analysis to playback.json");
        }

    }

}
