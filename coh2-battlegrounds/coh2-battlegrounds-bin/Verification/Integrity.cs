using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Battlegrounds.ErrorHandling;

namespace Battlegrounds.Verification;

public static class Integrity {

    private static ulong __integrityHash;

    public static ulong IntegrityHash => __integrityHash;

    public static string IntegrityHashString => $"0x{IntegrityHash:X}";

    public static void CheckIntegrity(string processPath) {

        // Grab self path
        string selfPath = processPath;
        string dllPath = processPath.Replace(".exe", "-bin.dll");
        string netPath = processPath.Replace(".exe", "-networking.dll");
        string checksumpath = processPath.Replace("coh2-battlegrounds.exe", "checksum.txt");

        // If dll is not found -> bail
        if (!File.Exists(dllPath))
            throw new FatalAppException();

        // If net dll not found -> bail
        if (!File.Exists(netPath))
            throw new FatalAppException();

        // Check self
        var selfCheck = ComputeChecksum(selfPath);
        var binCheck = ComputeChecksum(dllPath) + selfCheck;
        var netCheck = ComputeChecksum(netPath) + binCheck;

        // Check common
        __integrityHash = ComputeDirectoryChecksum(netCheck, Path.Combine(Path.GetDirectoryName(processPath) ?? throw new FatalAppException(), "bg_common"));

        // Check validity
        if (__integrityHash != Convert.ToUInt64(File.ReadAllText(checksumpath), 16)) {
#if DEBUG
            Trace.WriteLine($"<DEBUG> Integrity check failed - UPDATE CHECKSUM.TXT (Checksum = {IntegrityHashString})", nameof(Integrity));
#else
            __integrityHash = 0; // Try prevent this from being stored in memory
            //throw new FatalAppException($"Checksum integrity check failed."); // Should not actually write this
#endif
        } else {

            // Log success
            Trace.WriteLine("Verified integrity of core files.", nameof(Integrity));

        }

    }

    private static ulong ComputeDirectoryChecksum(ulong check, string dir) {

        if (dir.EndsWith("map_icons"))
            return 0;

        string[] files = Directory.GetFiles(dir);
        ulong fileSum = 0;
        foreach (var f in files)
            fileSum += ComputeChecksum(f);

        ulong dsum = 0;
        string[] dirs = Directory.GetDirectories(dir);
        foreach (var d in dirs)
            dsum += ComputeDirectoryChecksum(0, d);

        return check + dsum + fileSum;

    }

    private static ulong ComputeChecksum(string filepath) {

        ulong check = 0;
        using var fs = File.OpenRead(filepath);
        while (fs.Position < fs.Length) {
            check += (ulong)fs.ReadByte();
        }

        return check;

    }

}
