using System.Text.Json.Serialization;

namespace Battlegrounds.Update;

public class Asset {

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("browser_download_url")]
    public string InstallerDownloadUrl { get; set; }

}
