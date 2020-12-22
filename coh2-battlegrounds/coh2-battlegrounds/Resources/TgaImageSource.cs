using System;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace BattlegroundsApp.Resources {

    public sealed class TgaImageSource : ICloneable {

        private string m_fileSource;
        private BitmapSource m_bmpSource;

        public double Width => this.m_bmpSource.Width;

        public double Height => this.m_bmpSource.Height;

        private TgaImageSource(string tgaSource, BitmapSource source) {
            this.m_bmpSource = source;
            this.m_fileSource = tgaSource;
        }

        public object Clone() => new TgaImageSource(this.m_fileSource, this.m_bmpSource.Clone());

        public static implicit operator BitmapSource(TgaImageSource tgaImage) => tgaImage.m_bmpSource;

        public static BitmapSource TargaBitmapSourceFromFile(string targaSource) {

            // Make sure the file exists
            if (!File.Exists(targaSource)) {
                throw new FileNotFoundException("Failed to find targa file from argument.", targaSource);
            }

            // Open the binary reader
            using BinaryReader binaryReader = new BinaryReader(File.OpenRead(targaSource));

            // Define local variables
            byte IDLength, ColourMapType, DataType, ColourMapD, BitsPerPixel, Flags;
            short ColourMapO, ColourMapL, XO, YO, Width, Height;

            // Try and read header
            try {

                IDLength = binaryReader.ReadByte();
                ColourMapType = binaryReader.ReadByte();
                DataType = binaryReader.ReadByte();

                ColourMapO = binaryReader.ReadInt16();
                ColourMapL = binaryReader.ReadInt16();
                ColourMapD = binaryReader.ReadByte();

                XO = binaryReader.ReadInt16();
                YO = binaryReader.ReadInt16();

                Width = binaryReader.ReadInt16();
                Height = binaryReader.ReadInt16();

                BitsPerPixel = binaryReader.ReadByte();
                Flags = binaryReader.ReadByte();

            } catch (Exception e) {
                throw new BadImageFormatException("Failed to read tga header.", e);
            }

            // Verify header details
            if (DataType != 2 || (BitsPerPixel != 24 && BitsPerPixel != 32) || (Flags & 0xC0) != 0) {
                throw new BadImageFormatException("Unexpected tga file format.");
            }

            // 32 bits per pixel?
            bool is32bit = BitsPerPixel == 32;

            // Goto colour map and skip it
            binaryReader.BaseStream.Seek(IDLength, SeekOrigin.Current);
            if (ColourMapType == 1) {
                binaryReader.BaseStream.Seek(ColourMapL * (ColourMapD >> 3), SeekOrigin.Current);
            }

            // Read raw rgb(a) data
            byte[] imageBuffer = new byte[(BitsPerPixel >> 3) * Width * Height];
            binaryReader.BaseStream.Read(imageBuffer, 0, (Width * Height) * (BitsPerPixel / 8));

            // Create image and then return it
            return BitmapSource.Create(Width, Height, 96, 96, is32bit ? PixelFormats.Bgra32 : PixelFormats.Rgb24, null, imageBuffer, (Width * BitsPerPixel) / 8);

        }

        public static TgaImageSource CreateTargaImage(string source) {

            // Get the targa
            var targa = TargaBitmapSourceFromFile(source);

            // Return image
            return new TgaImageSource(source, targa);

        }

    }

}
