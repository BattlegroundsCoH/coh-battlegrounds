using System;
using coh2_battlegrounds_bin;
using coh2_battlegrounds_bin.Game.Battlegrounds;

namespace coh2_battlegrounds_console {
    
    class Program {
        
        static void Main(string[] args) {

            string latest = $"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}\\my games\\company of heroes 2\\playback\\temp.rec";

            Match match = new Match();
            if (match.LoadMatch($"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}\\my games\\company of heroes 2\\playback\\jello.rec")) {
                match.EvaluateResult();
            } else {
                Console.WriteLine("Failed to load replayfile...");
            }

            Console.WriteLine("Press any key to exit...");
            Console.Read();

        }

    }

}
