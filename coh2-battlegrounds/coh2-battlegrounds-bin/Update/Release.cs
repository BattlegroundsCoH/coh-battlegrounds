using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Battlegrounds.Update;

public class Release {

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("tag_name")]
    public string TagName { get; set; }

    [JsonPropertyName("published_at")]
    public DateTime PublishedAt { get; set; }

    [JsonPropertyName("assets")]
    public List<Asset> Assets { get; set; }

}
