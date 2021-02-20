using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace BattlegroundsApp.Resources {
    
    public static class PngImageSource {

        public static ImageSource FromMemory(byte[] rawMemory) {

            PngBitmapDecoder decoder = new PngBitmapDecoder(new MemoryStream(rawMemory), BitmapCreateOptions.None, BitmapCacheOption.Default);
            return decoder.Frames[0];

        }

    }

}
