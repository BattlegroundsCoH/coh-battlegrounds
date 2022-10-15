using System.Diagnostics;

using Battlegrounds.Gfx;

namespace Battlegrounds.Resources;

/// <summary>
/// Static utility class for <b><i>dynamically</i></b> loaded resources.
/// </summary>
public static class ResourceLoader {

    private static void AddGfxEntry(GfxMap map, string resourceName, string filepath, BinaryReader br) {

        // Check if resource is contained
        if (map.HasResource(resourceName)) {
            Trace.WriteLine($"Skipping GFX resource '{filepath}' (Resource already contains)", nameof(ResourceLoader));
            return;
        }

        // Open reader and verify is 
        if (!br.ReadBytes(8).SequenceEqual(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 })) {
            Trace.WriteLine($"Failed to load GFX resource '{filepath}' (Invalid PNG header found)", nameof(ResourceLoader));
            return;
        }

        // Read width and heigt
        br.BaseStream.Position += 8;
        int w = BitConverter.ToInt32(br.ReadBytes(4).Reverse().ToArray());
        int h = BitConverter.ToInt32(br.ReadBytes(4).Reverse().ToArray());
        br.BaseStream.Position = 0;

        // Create resource
        map.CreateResource(resourceName, br, w, h);

    }

    /// <summary>
    /// Load the contents of the specified file and attempt to load it into the resource context defined by the <paramref name="identifier"/>.
    /// </summary>
    /// <param name="identifier">The identifier to load resource data into.</param>
    /// <param name="filepath">The filepath to load the resource data from.</param>
    public static void LoadResourceFile(string identifier, string filepath) {

        if (!File.Exists(filepath))
            return;

        // Get resource name
        string resourceName = Path.GetFileNameWithoutExtension(filepath);

        // Determine action
        switch (identifier) {
            default:
                if (ResourceHandler.GfxMaps.TryGetValue(identifier, out GfxMap? map)) {

                    using var fs = File.OpenRead(filepath);
                    if (filepath.EndsWith(".dat")) {

                        // Keep track of added
                        int added = 0;

                        // Assume gfx map
                        GfxMap other = GfxMap.FromBinary(fs);
                        foreach (string resource in other.Resources)
                            if (!map.HasResource(resource)) {
                                map.AddResource(other.GetResource(resource)!);
                                added++;
                            }

                        // Log how much is actually loaded
                        var msg = $"Loaded gfx map {identifier}(v:0x{other.BinaryVersion:X2}) of which {added}/{other.Count} gfx files were added ({map.Count} total '{identifier}' gfx files).";
                        Trace.WriteLine(msg, nameof(ResourceLoader));

                    } else {
                        AddGfxEntry(map, resourceName, filepath, new BinaryReader(fs));
                    }

                } else if (Array.IndexOf(ResourceIdenitifers.IconIdentifiers, identifier) != -1) {

                    // Register
                    using var fs = File.OpenRead(filepath);
                    var gfxmap = ResourceHandler.GfxMaps[identifier] = GfxMap.FromBinary(fs);

                    // Log
                    Trace.WriteLine($"Loaded gfx map {identifier}(v:0x{gfxmap.BinaryVersion:X2}) with {gfxmap.Count} gfx files.", nameof(ResourceLoader));

                } else {
                    Trace.WriteLine($"Cannot load resource of type '{identifier}'", nameof(ResourceLoader));
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
            Trace.WriteLine($"Resource folder '{path}' contains unpackaged resources.", nameof(ResourceLoader));

        // Grab files
        string[] files = Directory.GetFiles(path);

        // Try find GFX resource and increase its size
        if (ResourceHandler.GfxMaps.TryGetValue(identifier, out GfxMap? map)) {
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
