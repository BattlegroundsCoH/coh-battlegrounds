using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

using Battlegrounds.Modding;
using Battlegrounds.Networking.Memory;
using Battlegrounds.Networking.Server;

namespace Battlegrounds.Compiler.Source {

    public class ManifestSource : IWinconditionSource {

        private static readonly ObjectCache ServerFiles = new(TimeSpan.FromMinutes(15));

        private ServerAPI m_api;

        public ManifestSource(ServerAPI serverAPI) {

            // Set api
            this.m_api = serverAPI;

            // Cache
            var result = ServerFiles.GetCachedValue(this.GetCachedFiles) as Dictionary<string, List<WinconditionSourceFile>>;
            if (result["scar"].Count > 0) {
                Trace.WriteLine("Successfully downloaded the gamemode files", nameof(ManifestSource));
            } else {
                Trace.WriteLine("Failed to download the gamemode files", nameof(ManifestSource));
            }

        }

        private Dictionary<string, List<WinconditionSourceFile>> GetCachedFiles() {

            // Create dictionary to store categorised files in.
            Dictionary<string, List<WinconditionSourceFile>> sourceFiles = new() {
                ["scar"] = new(),
                ["info"] = new(),
                ["locale"] = new(),
                ["win"] = new(),
                ["gfx"] = new(),
            };

            // Download the files
            /*var files = this.m_api.DownloadGamemodeFiles();
            for (int i = 0; i < files.Length; i++) {

                // If scar file, add to scar entry
                if (Path.GetExtension(files[i].filepath) is ".scar") {
                    sourceFiles["scar"].Add(new WinconditionSourceFile(files[i].filepath, files[i].filedata));
                } else if (Path.GetExtension(files[i].filepath) is ".ucs") {
                    sourceFiles["locale"].Add(new WinconditionSourceFile(files[i].filepath, files[i].filedata));
                } else if (Path.GetExtension(files[i].filepath) is ".win") {
                    sourceFiles["win"].Add(new WinconditionSourceFile(files[i].filepath, files[i].filedata));
                } else if (files[i].filepath.StartsWith("info\\")) {
                    sourceFiles["info"].Add(new WinconditionSourceFile(files[i].filepath, files[i].filedata));
                } else if (Path.GetExtension(files[i].filepath) is ".gfx" or ".dds") {
                    sourceFiles["gfx"].Add(new WinconditionSourceFile(files[i].filepath, files[i].filedata));
                }

            }*/

            return sourceFiles;

        }

        public WinconditionSourceFile GetInfoFile(IGamemode mod)
            => (ServerFiles.GetCachedValue(this.GetCachedFiles) as Dictionary<string, List<WinconditionSourceFile>>)["info"].FirstOrDefault(x => x.Path.EndsWith(".info"));

        public WinconditionSourceFile[] GetLocaleFiles()
            => (ServerFiles.GetCachedValue(this.GetCachedFiles) as Dictionary<string, List<WinconditionSourceFile>>)["locale"].ToArray();

        public WinconditionSourceFile GetModGraphic()
            => (ServerFiles.GetCachedValue(this.GetCachedFiles) as Dictionary<string, List<WinconditionSourceFile>>)["info"].FirstOrDefault(x => x.Path.EndsWith(".dds"));

        public WinconditionSourceFile[] GetScarFiles()
            => (ServerFiles.GetCachedValue(this.GetCachedFiles) as Dictionary<string, List<WinconditionSourceFile>>)["scar"].ToArray();

        public WinconditionSourceFile[] GetUIFiles(IGamemode mod)
            => (ServerFiles.GetCachedValue(this.GetCachedFiles) as Dictionary<string, List<WinconditionSourceFile>>)["gfx"].ToArray();

        public WinconditionSourceFile[] GetWinFiles()
            => (ServerFiles.GetCachedValue(this.GetCachedFiles) as Dictionary<string, List<WinconditionSourceFile>>)["win"].ToArray();

    }

}
