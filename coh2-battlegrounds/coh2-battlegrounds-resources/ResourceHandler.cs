using System.Collections;
using System.Diagnostics;
using System.Reflection;
using System.Resources;

using System.Windows.Media;
using System.Windows.Media.Imaging;

using Battlegrounds.Functional;
using Battlegrounds.Gfx;

namespace Battlegrounds.Resources;

public enum ResourceType {
    ProfanityFilter,
    // TODO: Add more types here on demand
}

public static class ResourceHandler {

    private static string[] _resourceNames;
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
        "entity_icons",
        "entity_symbols"
    };
    private static readonly string[] __iconResourceFiles = {
        "resources/ingame/ability_icons.dat",
        "resources/ingame/entity_icons.dat",
        "resources/ingame/entity_symbols.dat",
        "resources/ingame/item_icons.dat",
        "resources/ingame/symbol_icons.dat",
        "resources/ingame/unit_icons.dat",
        "resources/ingame/upgrade_icons.dat",
        "resources/ingame/phase_icons.dat",
        "resources/ingame/deploy_icons.dat",
        "resources/ingame/minimap_icons.dat",
        "resources/ingame/portraits.dat"
    };

    private static readonly Dictionary<string, ImageSource> _cache;
    private static readonly Dictionary<string, GfxMap> _gfxMaps;
    private static MemoryStream? _profanityFilter;

    static ResourceHandler() {
        
        _cache = new();
        _gfxMaps = new();
        _profanityFilter = null;
        _resourceNames = Array.Empty<string>();

    }

    /// <summary>
    /// Load all resources defined by the assembly.
    /// </summary>
    /// <param name="assembly">The assembly to load resources from</param>
    public static void LoadAllResources(Assembly? assembly) {
        assembly ??= Assembly.GetExecutingAssembly();
        _resourceNames = _resourceNames.Concat(GetResourcePaths(assembly).Cast<string>().ToArray());
    }

    private static IEnumerable<object> GetResourcePaths(Assembly assembly) {
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
                    _gfxMaps[name] = gfxmap;
                    Trace.WriteLine($"Loaded gfx map {name}(v:0x{gfxmap.BinaryVersion:X2}) with {gfxmap.Resources.Length} gfx files.", nameof(ResourceHandler));
                } else if (resource.Key is "resources/profanities.json") {
                    _profanityFilter = ToMemoryStream(datastream);
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

    public static ImageSource? GetIcon(string iconType, string iconName) {
        if (_cache.TryGetValue(iconName, out var source)) {
            return source;
        }
        if (_gfxMaps[iconType] is GfxMap gfx && gfx.GetResource(iconName) is GfxResource rs) {
            using (var stream = rs.Open()) {
                BitmapImage bitmapImage = new();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = stream;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                _cache[iconName] = bitmapImage; // Save to cache, so we dont needlessly do this operation every time
                return bitmapImage;
            }
        }
        return null;
    }

    public static bool HasIcon(string iconType, string iconName)
        => _cache.ContainsKey(iconName) || (_gfxMaps[iconType] is GfxMap gfx && gfx.GetResource(iconName) is not null);

    public static bool HasResource(string resourceName) => _resourceNames.Contains(resourceName.ToLowerInvariant());

    public static bool HasResource(Uri resourceUri) => _resourceNames.Contains(resourceUri.AbsolutePath[1..].ToLowerInvariant());

    public static MemoryStream? GetResourceAndUnload(ResourceType type) {
        switch (type) {
            case ResourceType.ProfanityFilter:
                var ms = new MemoryStream(_profanityFilter?.ToArray() ?? Array.Empty<byte>());
                _profanityFilter?.Dispose();
                return ms;
            default:
                throw new InvalidOperationException();
        }
    }

}
