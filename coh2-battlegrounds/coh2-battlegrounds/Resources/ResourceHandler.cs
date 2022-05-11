using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Threading;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using Battlegrounds.Gfx;

namespace BattlegroundsApp.Resources;

public class ResourceHandler {

    public enum ResourceType {
        ProfanityFilter,
        // TODO: Add more types here on demand
    }

    private readonly string[] m_resourceNames;
    private static readonly string[] __iconResourceFileNames = {
        "ability_icons",
        "item_icons",
        "symbol_icons",
        "unit_icons",
        "upgrade_icons",
        "phase_icons",
        "deploy_icons",
        "portraits",
        "minimap_icons",
    };
    private static readonly string[] __iconResourceFiles = {
        "resources/ingame/ability_icons.dat",
        "resources/ingame/item_icons.dat",
        "resources/ingame/symbol_icons.dat",
        "resources/ingame/unit_icons.dat",
        "resources/ingame/upgrade_icons.dat",
        "resources/ingame/phase_icons.dat",
        "resources/ingame/deploy_icons.dat",
        "resources/ingame/minimap_icons.dat",
        "resources/ingame/portraits.dat"
    };

    private readonly Dictionary<string, ImageSource> m_cache;
    private readonly Dictionary<string, GfxMap> m_gfxMaps;
    private MemoryStream? m_profanityFilter;

    public ResourceHandler() {
        this.m_cache = new();
        this.m_gfxMaps = new();
        this.m_profanityFilter = null;
        this.m_resourceNames = this.GetResourcePaths(Assembly.GetExecutingAssembly()).Cast<string>().ToArray();
    }

    private IEnumerable<object> GetResourcePaths(Assembly assembly) {
        var culture = Thread.CurrentThread.CurrentCulture;
        var resourceName = assembly.GetName().Name + ".g";
        var resourceManager = new ResourceManager(resourceName, assembly);
        try {
            var set = resourceManager.GetResourceSet(culture, true, true);
            if (set is null) {
                throw new Exception("Failed to load resource set.");
            }
            foreach (DictionaryEntry resource in set) {
                string? name = Path.GetFileNameWithoutExtension(resource.Key as string) ?? "InvalidPathEntry";
                if (resource.Value is not UnmanagedMemoryStream datastream) {
                    continue;
                }
                if (__iconResourceFiles.Contains(resource.Key as string)) {
                    var gfxmap = GfxMap.FromBinary(ToMemoryStream(datastream));
                    this.m_gfxMaps[name] = gfxmap;
                    Trace.WriteLine($"Loaded gfx map {name}(v:0x{gfxmap.BinaryVersion:X2}) with {gfxmap.Resources.Length} gfx files.", nameof(ResourceHandler));
                } else if (resource.Key is "resources/profanities.json") {
                    this.m_profanityFilter = ToMemoryStream(datastream);
                    Trace.WriteLine($"Loaded json resource into memory '{name}'");
                }
                yield return resource.Key;
            }
        } finally {
            resourceManager.ReleaseAllResources();
        }
    }

    private static MemoryStream ToMemoryStream(UnmanagedMemoryStream ums) {
        var ms = new MemoryStream();
        ums.CopyTo(ms);
        ms.Position = 0;
        return ms;
    }

    public ImageSource? GetIcon(string iconType, string iconName) {
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
        => this.m_cache.ContainsKey(iconName) || (this.m_gfxMaps[iconType] is GfxMap gfx && gfx.GetResource(iconName) is not null);

    public bool HasResource(string resourceName) => this.m_resourceNames.Contains(resourceName.ToLowerInvariant());

    public bool HasResource(Uri resourceUri) => this.m_resourceNames.Contains(resourceUri.AbsolutePath[1..].ToLowerInvariant());

    public MemoryStream? GetResourceAndUnload(ResourceType type) {
        switch (type) {
            case ResourceType.ProfanityFilter:
                var ms = new MemoryStream(this.m_profanityFilter?.ToArray() ?? Array.Empty<byte>());
                this.m_profanityFilter?.Dispose();
                return ms;
            default:
                throw new InvalidOperationException();
        }
    }

}
