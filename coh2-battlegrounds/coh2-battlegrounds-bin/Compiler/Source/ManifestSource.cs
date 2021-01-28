using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Battlegrounds.Modding;
using Battlegrounds.Online;

namespace Battlegrounds.Compiler.Source {

    public class ManifestSource : IWinconditionSource {

        private Dictionary<string, List<string>> m_files;

        public ManifestSource() {
            
            // Files
            this.m_files = new Dictionary<string, List<string>>() {
                ["scar"] = new List<string>(),
                ["info"] = new List<string>(),
                ["win"] = new List<string>(),
                ["locale"] = new List<string>(),
                ["source"] = new List<string>(),
            };

            // Prepare reader
            using StreamReader reader = new StreamReader(BattlegroundsInstance.GetRelativePath(BattlegroundsPaths.BINARY_FOLDER, "scripts.manifest"));
            string current = string.Empty;
            string ln = null;

            // Read in data
            while((ln = reader.ReadLine()) is not null) {
                if (ln.Length > 0) {
                    if (ln[0] == '\t') {
                        this.m_files[current].Add(ln[1..]);
                    } else {
                        string k = ln[0..^1];
                        if (this.m_files.ContainsKey(k)) {
                            current = k;
                        } else {
                            throw new InvalidDataException();
                        }
                    }
                }
            }

        }

        private WinconditionSourceFile GetSourceFile(string path, string downloadFrom, Encoding encoding = null) {
            
            // Set encoding if not defined
            if (encoding == null) {
                encoding = Encoding.UTF8;
            }
            
            // Determine how to download it
            if (this.m_files["source"].FirstOrDefault() is "server") {
                throw new NotImplementedException();
            } else if (this.m_files["source"].FirstOrDefault() is "github") {
                return new WinconditionSourceFile(path, encoding.GetBytes(SourceDownloader.DownloadSourceCode(downloadFrom)));
            } else {
                throw new NotImplementedException();
            }

        }

        public WinconditionSourceFile GetInfoFile(IWinconditionMod mod) => throw new NotImplementedException();
        
        public WinconditionSourceFile[] GetLocaleFiles() => throw new NotImplementedException();
        
        public WinconditionSourceFile GetModGraphic() => throw new NotImplementedException();
        
        public WinconditionSourceFile[] GetScarFiles() => throw new NotImplementedException();
        
        public WinconditionSourceFile[] GetUIFiles(IWinconditionMod mod) => throw new NotImplementedException();
        
        public WinconditionSourceFile[] GetWinFiles() => throw new NotImplementedException();

    }

}
