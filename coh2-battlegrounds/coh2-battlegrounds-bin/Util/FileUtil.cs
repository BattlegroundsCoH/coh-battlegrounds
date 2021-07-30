using System;
using System.IO;
using System.Collections.Generic;
using System.Text;

namespace Battlegrounds.Util {

    public static class FileUtil {

        public static byte[] ReadUTF8Binary(string filepath) {
            using (BinaryReader reader = new BinaryReader(File.OpenRead(filepath), Encoding.UTF8, false)) {
                return reader.ReadBytes((int)reader.BaseStream.Length);
            }
        }

    }

}
