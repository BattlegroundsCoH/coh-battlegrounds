using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using Battlegrounds.Gfx;

namespace BattlegroundsApp.Resources {

    public class ResourceHandler {

        private string[] m_resourceNames;
        private static readonly string[] __iconResourceFileNames = {
            "ability_icons",
            "item_icons",
            "symbol_icons",
            "unit_icons",
            "upgrade_icons",
            "phase_icons",
            "deploy_icons",
        };
        private static readonly string[] __iconResourceFiles = {
            "resources/ingame/ability_icons.dat",
            "resources/ingame/item_icons.dat",
            "resources/ingame/symbol_icons.dat",
            "resources/ingame/unit_icons.dat",
            "resources/ingame/upgrade_icons.dat",
            "resources/ingame/phase_icons.dat",
            "resources/ingame/deploy_icons.dat"
        };

        private Dictionary<string, ImageSource> m_cache;
        private Dictionary<string, GfxMap> m_gfxMaps;
        private ResourceSet m_set;

        public ResourceHandler() {
            this.m_cache = new();
            this.m_gfxMaps = new();
            this.m_resourceNames = this.GetResourcePaths(Assembly.GetExecutingAssembly()).Cast<string>().ToArray();
        }

        private IEnumerable<object> GetResourcePaths(Assembly assembly) {
            var culture = System.Threading.Thread.CurrentThread.CurrentCulture;
            var resourceName = assembly.GetName().Name + ".g";
            var resourceManager = new ResourceManager(resourceName, assembly);
            try {
                this.m_set = resourceManager.GetResourceSet(culture, true, true);
                foreach (System.Collections.DictionaryEntry resource in this.m_set) {
                    if (__iconResourceFiles.Contains(resource.Key as string)) {
                        string name = Path.GetFileNameWithoutExtension(resource.Key as string);
                        var datastream = resource.Value as UnmanagedMemoryStream;
                        var ms = new MemoryStream();
                        datastream.CopyTo(ms);
                        ms.Position = 0;
                        this.m_gfxMaps[name] = GfxMap.FromBinary(ms);
                    }
                    yield return resource.Key;
                }
            } finally {
                resourceManager.ReleaseAllResources();
            }
        }

        public ImageSource GetIcon(string iconType, string iconName) {
            if (this.m_cache.TryGetValue(iconName, out var source)) {
                return source;
            }
            if (this.m_gfxMaps[iconType] is GfxMap gfx && gfx.GetResource(iconName) is GfxResource rs) {
                using (var stream = rs.Open()) {
                    BitmapImage bitmapImage = new();
                    bitmapImage.BeginInit();
                    bitmapImage.StreamSource = stream;
                    bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                    bitmapImage.EndInit();
                    this.m_cache[iconName] = bitmapImage; // Save to cache, so we dont needlessly do this operation every time
                    return bitmapImage;
                }
            }
            return null;
        }

        public bool HasIcon(string iconType, string iconName) 
            => this.m_cache.ContainsKey(iconName) || (this.m_gfxMaps[iconType] is GfxMap gfx && gfx.GetResource(iconName) is GfxResource);

        public bool HasResource(string resourceName) => this.m_resourceNames.Contains(resourceName.ToLowerInvariant());

        public bool HasResource(Uri resourceUri) => this.m_resourceNames.Contains(resourceUri.AbsolutePath[1..].ToLowerInvariant());

    }

}
