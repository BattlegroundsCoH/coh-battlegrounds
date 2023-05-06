using System.Collections;
using System.Reflection;
using System.Resources;

using System.Windows.Media;
using System.Windows.Media.Imaging;

using Battlegrounds.Functional;
using Battlegrounds.Gfx;
using Battlegrounds.Gfx.Loaders;
using Battlegrounds.Logging;

namespace Battlegrounds.Resources;

public enum ResourceType {
    ProfanityFilter,
    // TODO: Add more types here on demand
}

public static class ResourceHandler {

    private static readonly Logger logger = Logger.CreateLogger();

    private static string[] _resourceNames;
    
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

    private static readonly Dictionary<string, ImageSource> _IconCacheLegacy;
    private static readonly Dictionary<IIconSource, ImageSource> _IconCache;
    private static MemoryStream? _profanityFilter;

    public static readonly IGfxMapLoaderFactory GfxLoaderFactory;

    public static readonly Dictionary<string, IGfxMap> GfxMaps;

    static ResourceHandler() {

        _IconCache = new();
        _IconCacheLegacy = new();

        _profanityFilter = null;
        _resourceNames = Array.Empty<string>();

        GfxMaps = new();
        GfxLoaderFactory = new GfxMapLoaderFactory();

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
            var set = resourceManager.GetResourceSet(culture, true, true) ?? throw new Exception("Failed to load resource set.");
            foreach (DictionaryEntry resource in set) {
                string? name = Path.GetFileNameWithoutExtension(resource.Key as string) ?? "InvalidPathEntry";
                if (resource.Value is not UnmanagedMemoryStream datastream) {
                    continue;
                }
                if (__iconResourceFiles.Contains(resource.Key as string)) {
                    using var ms = ToMemoryStream(datastream);
                    using var msReader = new BinaryReader(ms);
                    var gfxVersion = msReader.ReadInt32();
                    var gfxReader = GfxLoaderFactory.GetGfxMapLoader((GfxVersion)gfxVersion);
                    var gfxmap = GfxMaps[name] = gfxReader.LoadGfxMap(msReader);
                    logger.Info($"Loaded gfx map {name}(v:0x{gfxmap.GfxVersion:X2}) with {gfxmap.Count} gfx files.");
                } else if (resource.Key is "resources/profanities.json") {
                    _profanityFilter = ToMemoryStream(datastream);
                    logger.Info($"Loaded json resource into memory '{name}'");
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

    [Obsolete("Use the method accepting an IIconSource")]
    public static ImageSource? GetIcon(string iconType, string iconName) {
        if (_IconCacheLegacy.TryGetValue(iconName, out var source)) {
            return source;
        }
        if (GfxMaps[iconType] is IGfxMap gfx && gfx.GetResource(iconName) is GfxResource rs) {
            // TODO: Check resource type
            using (var stream = rs.Open()) {
                BitmapImage bitmapImage = new();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = stream;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                _IconCacheLegacy[iconName] = bitmapImage; // Save to cache, so we dont needlessly do this operation every time
                return bitmapImage;
            }
        }
        return null;
    }

    public static ImageSource? GetIcon(IIconSource iconSource) {
        if (_IconCache.TryGetValue(iconSource, out var source)) {
            return source;
        }
        if (GfxMaps[iconSource.Container] is IGfxMap gfx 
            && gfx.HasResource(iconSource.Identifier) && gfx.GetResource(iconSource.Identifier) is GfxResource rs) {
            // TODO: Check resource type
            using (var stream = rs.Open()) {
                BitmapImage bitmapImage = new();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = stream;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                _IconCache[iconSource] = bitmapImage; // Save to cache, so we dont needlessly do this operation every time
                return bitmapImage;
            }
        }
        logger.Warning("Failed locating resource {0}:{1}", iconSource.Container, iconSource.Identifier);
        return null;
    }

    [Obsolete("Use the method accepting an IIconSource")]
    public static bool HasIcon(string iconType, string iconName)
        => _IconCacheLegacy.ContainsKey(iconName) || (GfxMaps[iconType] is IGfxMap gfx && gfx.GetResource(iconName) is not null);

    public static bool HasIcon(IIconSource iconSource)
        => _IconCache.ContainsKey(iconSource) || (GfxMaps[iconSource.Container] is IGfxMap gfx && gfx.HasResource(iconSource.Identifier));

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
