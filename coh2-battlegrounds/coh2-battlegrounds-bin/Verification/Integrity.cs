using System;
using System.IO;

using Battlegrounds.Logging;

namespace Battlegrounds.Verification;

/// <summary>
/// 
/// </summary>
public static class Integrity {

    private static readonly Logger logger = Logger.CreateLogger();

    private static ulong __integrityHash;

    /// <summary>
    /// 
    /// </summary>
    public static ulong IntegrityHash => __integrityHash;

    /// <summary>
    /// 
    /// </summary>
    public static string IntegrityHashString => $"0x{IntegrityHash:X}";

    /// <summary>
    /// 
    /// </summary>
    public static byte[] IntegrityHashBytes => BitConverter.GetBytes(__integrityHash);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="processPath"></param>
    public static void CheckIntegrity(string processPath) {

        // Grab self path
        string selfPath = processPath;
        string dllPath = processPath.Replace(".exe", ".Core.dll");
        string checksumpath = processPath.Replace("Battlegrounds.exe", "checksum.txt");

        // If dll is not found -> bail
        if (!File.Exists(dllPath))
            throw logger.Fatal("Battlegrounds.Core.dll not found.");

        // Check self
        var selfCheck = ComputeChecksum(selfPath);
        var binCheck = ComputeChecksum(dllPath) + selfCheck;

        // Check common
        __integrityHash = ComputeDirectoryChecksum(binCheck, Path.Combine(Path.GetDirectoryName(processPath) ?? throw logger.Fatal("Failed getting directory {0}", processPath), "bg_common"));

        // Check validity
        if (__integrityHash != Convert.ToUInt64(File.ReadAllText(checksumpath), 16)) {
#if DEBUG
            logger.Warning($"Integrity check failed - UPDATE CHECKSUM.TXT (Checksum = {IntegrityHashString})", nameof(Integrity));
#else
            __integrityHash = 0; // Try prevent this from being stored in memory
            throw logger.Fatal($"Checksum integrity check failed.");
#endif
        } else {

            // Log success
            logger.Info("Verified integrity of core files.", nameof(Integrity));

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

        // Skip workshop file, it won't be the same for everybody
        if (Path.GetFileNameWithoutExtension(filepath) is "workshop-map-db")
            return 0;

        ulong check = 0;
        using var fs = File.OpenRead(filepath);
        while (fs.Position < fs.Length) {
            check += (ulong)fs.ReadByte();
        }

        return check;

    }

}
