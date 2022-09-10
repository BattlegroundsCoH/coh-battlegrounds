#if DEBUG
using System;
using System.Diagnostics;
using System.IO;

namespace Battlegrounds;

public class BattlegroundsDebug {

    public bool UseLocalWincondition { get; }

    public BattlegroundsDebug() {
        try {
            if (File.Exists("debug.ini")) {
                var lines = File.ReadAllLines("debug.ini");
                for (int i = 0; i < lines.Length; i++) {
                    string[] kvp = lines[i].Split('=');
                    if (kvp.Length is 2) {
                        switch (kvp[0].Trim()) {
                            case "useLocalSource":
                                UseLocalWincondition = kvp[1].Trim() is "true";
                                break;
                            default:
                                break;
                        }
                    }
                }
            }
        } catch (Exception e) {
            Trace.WriteLine($"Failed to read debugging data: {e}", nameof(BattlegroundsDebug));
        }
    }

}

#endif