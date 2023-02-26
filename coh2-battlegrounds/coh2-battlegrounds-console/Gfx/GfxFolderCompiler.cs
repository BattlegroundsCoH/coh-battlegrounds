using System;
using System.IO;
using System.Linq;

using Battlegrounds.Gfx;

namespace Battlegrounds.Developer.Gfx;

public static class GfxFolderCompiler {

    public static void Compile(string input, string output, int version = GfxMap.GfxBinaryVersion) {

        // Define output
        if (string.IsNullOrEmpty(output)) {
            output = Path.Combine(Environment.CurrentDirectory, Path.GetDirectoryName(input + "\\") + ".dat");
        }

        // Log where
        Console.WriteLine("Compiling directory: " + input);
        Console.WriteLine("Output: " + output);

        // Select images
        string[] pngs = Directory.GetFiles(input, "*.png");
        GfxMap map = new(pngs.Length);

        // Loop over pngs
        for (int i = 0; i < pngs.Length; i++) {
            using (BinaryReader br = new(File.OpenRead(pngs[i]))) {
                if (!br.ReadBytes(8).SequenceEqual(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 })) {
                    continue;
                }
                br.BaseStream.Position += 8;
                int w = BitConverter.ToInt32(br.ReadBytes(4).Reverse().ToArray());
                int h = BitConverter.ToInt32(br.ReadBytes(4).Reverse().ToArray());
                br.BaseStream.Position = 0;
                string name = Path.GetFileNameWithoutExtension(pngs[i]);
                map.CreateResource(i, br.ReadBytes((int)br.BaseStream.Length), name, w, h);
                Console.WriteLine($"Created resource {i} [{name}, {w}x{h}]");
            }
        }

        // Save
        File.WriteAllBytes(output, map.AsBinary(version: version));

    }

}
