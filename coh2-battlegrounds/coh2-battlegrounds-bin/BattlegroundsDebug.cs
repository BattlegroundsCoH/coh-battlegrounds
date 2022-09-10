#if DEBUG
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Battlegrounds;

/// <summary>
/// Class representing additional debug settings.
/// </summary>
public class BattlegroundsDebug {

    /// <summary>
    /// Get if the wincondition source should be the local source and not the source from the repository.
    /// </summary>
    public bool UseLocalWincondition { get; }

    /// <summary>
    /// Get or set additional flags that should be set in the wincondition output.
    /// </summary>
    public string[] ScarFlags { get; }

    /// <summary>
    /// Initialise the <see cref="BattlegroundsDebug"/> instance.
    /// </summary>
    public BattlegroundsDebug() {
        try {
            List<string> scarFlags = new();
            if (File.Exists("debug.ini")) {
                var lines = File.ReadAllLines("debug.ini");
                for (int i = 0; i < lines.Length; i++) {
                    string[] kvp = lines[i].Split('=');
                    if (kvp.Length is 2) {
                        string trimmed = kvp[0].Trim();
                        string v = kvp[1].Trim();
                        switch (trimmed) {
                            case "useLocalSource":
                                UseLocalWincondition = v is "true";
                                break;
                            default:
                                if (trimmed.StartsWith("scar_")) {
                                    string k = trimmed[5..];
                                    scarFlags.Add($"{k} = {v}");
                                } else {
                                    Trace.WriteLine($"Invalid debug value: '{trimmed}' (='{v}')", nameof(BattlegroundsDebug));
                                }
                                break;
                        }
                    }
                }
            }
            this.ScarFlags = scarFlags.ToArray();
        } catch (Exception e) {
            
            Trace.WriteLine($"Failed to read debugging data: {e}", nameof(BattlegroundsDebug));
            
            this.ScarFlags = Array.Empty<string>();

        }
    }

}

#endif