using Battlegrounds.Gfx;
using Battlegrounds.Logging;

namespace Battlegrounds.Resources;

/// <summary>
/// Static utility class for <b><i>dynamically</i></b> loaded resources.
/// </summary>
public static class ResourceLoader {

    private static readonly Logger logger = Logger.CreateLogger();

    private static void AddGfxEntry(IGfxMap map, string resourceName, string filepath, BinaryReader br) {

        // Check if resource is contained
        if (map.HasResource(resourceName)) {
            logger.Info($"Skipping GFX resource '{filepath}' (Resource already contains)");
            return;
        }

        // Open reader and verify is 
        if (!br.ReadBytes(8).SequenceEqual(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 })) {
            logger.Error($"Failed to load GFX resource '{filepath}' (Invalid PNG header found)");
            return;
        }

        // Read width and heigt
        br.BaseStream.Position += 8;
        int w = BitConverter.ToInt32(br.ReadBytes(4).Reverse().ToArray());
        int h = BitConverter.ToInt32(br.ReadBytes(4).Reverse().ToArray());
        br.BaseStream.Position = 0;

        // Create resource
        map.CreateResource(resourceName, br, w, h, GfxResourceType.Png);

    }

    /// <summary>
    /// Load the contents of the specified file and attempt to load it into the resource context defined by the <paramref name="identifier"/>.
    /// </summary>
    /// <param name="identifier">The identifier to load resource data into.</param>
    /// <param name="filepath">The filepath to load the resource data from.</param>
    public static void LoadResourceFile(string identifier, string filepath) {

        // Ensure file exists before trying to load it.
        if (!File.Exists(filepath))
            return;

        // Get resource name
        string resourceName = Path.GetFileNameWithoutExtension(filepath);

        // Determine action
        switch (identifier) {
            default:
                if (ResourceHandler.GfxMaps.TryGetValue(identifier, out IGfxMap? map)) {

                    using var fs = File.OpenRead(filepath);
                    if (filepath.EndsWith(".dat")) {

                        // Keep track of added
                        int added = 0;

                        // Read version
                        using var fsr = new BinaryReader(fs);
                        GfxVersion ver = (GfxVersion)fsr.ReadInt32();

                        // Assume gfx map
                        IGfxMap other = ResourceHandler.GfxLoaderFactory.GetGfxMapLoader(ver).LoadGfxMap(fsr);
                        foreach (string resource in other.GetResourceManifest())
                            if (!map.HasResource(resource)) {
                                map.AddResource(other.GetResource(resource)!);
                                added++;
                            }

                        // Log how much is actually loaded
                        logger.Info($"Loaded gfx map {identifier}(v:0x{other.GfxVersion:X2}) of which {added}/{other.Count} gfx files were added ({map.Count} total '{identifier}' gfx files).");

                    } else {
                        AddGfxEntry(map, resourceName, filepath, new BinaryReader(fs));
                    }

                } else if (Array.IndexOf(ResourceIdenitifers.IconIdentifiers, identifier) != -1) {

                    // Open map
                    using var fs = File.OpenRead(filepath);
                    using var fsr = new BinaryReader(fs);

                    // Determine version
                    GfxVersion ver = (GfxVersion)fsr.ReadInt32();
                    var gfxReader = ResourceHandler.GfxLoaderFactory.GetGfxMapLoader(ver);

                    var gfxmap = ResourceHandler.GfxMaps[identifier] = gfxReader.LoadGfxMap(fsr);

                    // Log
                    logger.Info($"Loaded gfx map {identifier}(v:0x{gfxmap.GfxVersion:X}) with {gfxmap.Count} gfx files.");

                } else {
                    logger.Warning($"Cannot load resource of type '{identifier}'");
                }
                break;
        }        

    }

    /// <summary>
    /// Load all the file contents in a directory into the resource context defined by the <paramref name="identifier"/>.
    /// </summary>
    /// <param name="identifier">The identifier to load resource data into.</param>
    /// <param name="path">The directory path to load resources from.</param>
    /// <param name="shouldPackage">Should the call emit warnings that the folder should be packaged and not have loose files.</param>
    public static void LoadResourceFolder(string identifier, string path, bool shouldPackage = true) {

        if (!Directory.Exists(path))
            return;

        if (shouldPackage)
            logger.Info($"Resource folder '{path}' contains unpackaged resources.");

        // Grab files
        string[] files = Directory.GetFiles(path);

        // Try find GFX resource and increase its size
        if (ResourceHandler.GfxMaps.TryGetValue(identifier, out IGfxMap? map)) {
            map.Allocate(files.Length);
        }

        // Load
        foreach (var filepath in files)
            LoadResourceFile(identifier, filepath);

    }

    /// <summary>
    /// Load all the resources that have been packaged with the application.
    /// </summary>
    public static void LoadAllPackagedResources() {

        // Load included gfx files
        LoadResourceFile(ResourceIdenitifers.ABILITY_ICONS, "Gfx\\Icons\\ability_icons.dat");
        LoadResourceFile(ResourceIdenitifers.DEPLOY_ICONS, "Gfx\\Icons\\deploy_icons.dat");
        LoadResourceFile(ResourceIdenitifers.ENTITY_ICONS, "Gfx\\Icons\\entity_icons.dat");
        LoadResourceFile(ResourceIdenitifers.ENTITY_SYMBOL_ICONS, "Gfx\\Icons\\entity_symbols.dat");
        LoadResourceFile(ResourceIdenitifers.ITEM_ICONS, "Gfx\\Icons\\item_icons.dat");
        LoadResourceFile(ResourceIdenitifers.MINIMAP_ICONS, "Gfx\\Icons\\minimap_icons.dat");
        LoadResourceFile(ResourceIdenitifers.PHASE_ICONS, "Gfx\\Icons\\phase_icons.dat");
        LoadResourceFile(ResourceIdenitifers.PORTRAITS, "Gfx\\Icons\\portraits.dat");
        LoadResourceFile(ResourceIdenitifers.SYMBOL_ICONS, "Gfx\\Icons\\symbol_icons.dat");
        LoadResourceFile(ResourceIdenitifers.UNIT_ICONS, "Gfx\\Icons\\unit_icons.dat");
        LoadResourceFile(ResourceIdenitifers.UPGRADE_ICONS, "Gfx\\Icons\\upgrade_icons.dat");

        // Load any additional files that are stored directly on drive
        foreach (string identifier in ResourceIdenitifers.IconIdentifiers)
            LoadResourceFolder(identifier, $"Gfx\\Icons\\{identifier}\\");

    }

}
