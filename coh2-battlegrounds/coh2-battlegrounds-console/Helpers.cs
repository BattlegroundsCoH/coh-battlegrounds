using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace coh2_battlegrounds_console;
public static class Helpers {

    public static void CopyFilesRecursively(string srcPath, string destPath) {

        //Create all of the directories
        foreach (string dirPath in Directory.GetDirectories(srcPath, "*", SearchOption.AllDirectories)) {
            Directory.CreateDirectory(dirPath.Replace(srcPath, destPath));
        }

        //Copy all the files & Replaces any files with the same name
        foreach (string newPath in Directory.GetFiles(srcPath, "*.*", SearchOption.AllDirectories)) {
            File.Copy(newPath, newPath.Replace(srcPath, destPath), true);
        }

    }

}
