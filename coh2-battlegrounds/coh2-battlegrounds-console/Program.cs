using System;
using Battlegrounds.Compiler;
using Battlegrounds.Game.Battlegrounds;

namespace coh2_battlegrounds_console {
    
    class Program {
        
        static void Main(string[] args) {

            Battlegrounds.Game.Database.BlueprintManager.CreateDatabase();

            string latest = $"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}\\my games\\company of heroes 2\\playback\\temp.rec";

            GameMatch match = new GameMatch();
            if (match.LoadMatch($"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}\\my games\\company of heroes 2\\playback\\bam.rec")) {
                match.EvaluateResult();
            } else {
                Console.WriteLine("Failed to load replayfile...");
            }

            WinconditionCompiler.CompileToSga("temp_build", "session.scar");

            //Battlegrounds.Game.CoH2Launcher.Launch();

            Console.WriteLine("Press any key to exit...");
            Console.Read();

        }

    }

}
