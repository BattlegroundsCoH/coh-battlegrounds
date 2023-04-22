using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;

using Battlegrounds.Functional;
using Battlegrounds.Gfx;
using Battlegrounds.Gfx.Writers;
using Battlegrounds.Logging;

namespace Battlegrounds.Developer.Gfx;

public static class GfxFolderCompiler {

    private static readonly Logger logger = Logger.CreateLogger();

    public static void Compile(string input, string output, GfxVersion version = GfxVersion.Latest, string relativePath = "", bool convertToPngWhenTga = false) {

        // Define output
        if (string.IsNullOrEmpty(output)) {
            output = Path.Combine(Environment.CurrentDirectory, Path.GetDirectoryName(input + "\\") + ".dat");
        }

        // Log where
        logger.Info("Compiling directory: " + input);
        logger.Info("Output: " + output);

        // Select images
        string absInput = Path.GetFullPath(input);
        string[] inputFiles = Directory.GetFiles(absInput, "*.png", SearchOption.AllDirectories)
            .Concat(Directory.GetFiles(absInput, "*.tga", SearchOption.AllDirectories))
            .Concat(Directory.GetFiles(absInput, "*.bmp", SearchOption.AllDirectories))
            .Concat(Directory.GetFiles(absInput, "*.xaml", SearchOption.AllDirectories))
            .Concat(Directory.GetFiles(absInput, "*.html", SearchOption.AllDirectories));
        IGfxMap map = version == GfxVersion.V3 ? new PathGfxMap() { Delimiter = '\\' } : new StandardGfxMap(inputFiles.Length);

        // Loop over pngs
        for (int i = 0; i < inputFiles.Length; i++) {
            try {
                string ext = Path.GetExtension(inputFiles[i]);
                string name = map is PathGfxMap ? inputFiles[i].Replace(Path.GetFullPath(relativePath) + "\\", string.Empty) : Path.GetFileName(inputFiles[i]);
                string rid = name.Replace(ext, string.Empty);
                using var br = new BinaryReader(File.OpenRead(inputFiles[i]));
                switch (Path.GetExtension(inputFiles[i])) {
                    case ".png":
                        RegisterPng(rid, map, br, i, version);
                        break;
                    case ".tga":
                        RegisterTarga(rid, map, br, i, convertToPngWhenTga);
                        break;
                    default:
                        throw new NotImplementedException();
                }


            } catch (Exception ex) {
                logger.Info($"Failed creating resource {i}: {ex.Message}");
            }
        }

        // Delete output if exists
        if (File.Exists(output)) {
            File.Delete(output);
        }

        // Open file
        using var fs = File.OpenWrite(output);

        // Get writer
        var writer = new GfxMapWriterFactory().GetWriter(map).Save(map, fs);

    }

    private static void RegisterPng(string inputfile, IGfxMap gfx, BinaryReader br, int i, GfxVersion version) {

        if (!br.ReadBytes(8).SequenceEqual(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 }) && version != GfxVersion.V3) {
            logger.Info($"Skipping resource {i}, not a PNG");
            return;
        }

        br.BaseStream.Position += 8;
        int w = BitConverter.ToInt32(br.ReadBytes(4).Reverse().ToArray());
        int h = BitConverter.ToInt32(br.ReadBytes(4).Reverse().ToArray());
        br.BaseStream.Position = 0;
        
        gfx.CreateResource(i, br.ReadBytes((int)br.BaseStream.Length), inputfile, w, h, GfxResourceType.Png);

        logger.Info($"Created PNG resource {i} [{inputfile}, {w}x{h}]");

    }

    private static unsafe void RegisterTarga(string inputfile, IGfxMap gfx, BinaryReader br, int i, bool toPng) {

        // Open tga
        var tgaHeader = TgaPixelReader.ReadTargaImageHeader(br);
        if (tgaHeader.IfSecondOption(out IOException? ioex)) {
            logger.Info($"Skipping resource {i}, not a valid Targa file - {ioex.Message}");
            return;
        }

        // Reset (yes, reread the header if toPng flag is enabled...)
        br.BaseStream.Position = 0;

        if (toPng) {

            // Read to image
            TgaImage img = TgaPixelReader.ReadTarga(br);

            // Create image from pixeldata
            byte[] bytes = img.GetPixelData();
            fixed (byte* pData = bytes) {

                // Get format
                PixelFormat pixelFormat = img.Is32Bit ? PixelFormat.Format32bppArgb : PixelFormat.Format24bppRgb;

                // Create bitmap
                Bitmap bmp = new Bitmap(img.Width, img.Height, img.Stride, pixelFormat, new IntPtr(pData));

                // Reinterpret as png
                using MemoryStream container = new MemoryStream();
                bmp.Save(container, ImageFormat.Png);
                
                // Reset position
                container.Position = 0;

                // Open reader on container
                using var pngReader = new BinaryReader(container);

                // Call png register
                RegisterPng(inputfile, gfx, pngReader, i, gfx.GfxVersion);

            }

        } else {

            // Read bytes
            byte[] targaImageData = br.ReadBytes((int)br.BaseStream.Length);

            // Create
            gfx.CreateResource(i, targaImageData, inputfile, tgaHeader.First.Width, tgaHeader.First.Height, GfxResourceType.Tga);

            // Log
            logger.Info($"Created TGA resource {i} [{inputfile}, {tgaHeader.First.Width}x{tgaHeader.First.Height}]");

        }

    }

}
