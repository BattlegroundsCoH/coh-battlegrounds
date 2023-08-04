using System.IO;

namespace Battlegrounds.Developer;

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
