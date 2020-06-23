using System;
using coh2_battlegrounds_bin;

namespace coh2_battlegrounds_console {
    
    class Program {
        
        static void Main(string[] args) {

            string latest = $"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}\\my games\\company of heroes 2\\playback\\temp_mod.rec";

            ReplayFile replay = new ReplayFile($"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}\\my games\\company of heroes 2\\playback\\temp_mod.rec");
            if (replay.LoadReplay()) {
                replay.Dump();
            } else {
                Console.WriteLine("Failed to load replayfile...");
            }

            Console.WriteLine("Press any key to exit...");
            Console.Read();

        }

    }

}
