using System.Windows.Media.Imaging;
using System.Windows.Media;

using Battlegrounds.Gfx;

namespace Battlegrounds.Resources.Imaging;

public sealed class TgaImageSource : ICloneable {

    private readonly string m_fileSource;
    private readonly BitmapSource m_bmpSource;

    public double Width => this.m_bmpSource.Width;

    public double Height => this.m_bmpSource.Height;

    private TgaImageSource(string tgaSource, BitmapSource source) {
        this.m_bmpSource = source;
        this.m_fileSource = tgaSource;
    }

    public object Clone() => new TgaImageSource(this.m_fileSource, this.m_bmpSource.Clone());

    public static implicit operator BitmapSource(TgaImageSource tgaImage) => tgaImage.m_bmpSource;

    public static BitmapSource TargaBitmapSourceFromFile(string targaSource) {

        // Load image
        var targa = TgaPixelReader.ReadTarga(targaSource);

        // Grab format
        var format = targa.Is32Bit ? PixelFormats.Bgra32 : PixelFormats.Bgr24;

        // Create image and then return it
        return BitmapSource.Create(targa.Width, targa.Height, targa.DPIX, targa.DPIY, format, null, targa.GetPixelData(), targa.Stride);

    }

    public static TgaImageSource CreateTargaImage(string source) {

        // Get the targa
        var targa = TargaBitmapSourceFromFile(source);

        // Return image
        return new TgaImageSource(source, targa);

    }

}
