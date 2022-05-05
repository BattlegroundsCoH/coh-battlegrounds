using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Battlegrounds.Networking.LobbySystem.json;

/// <summary>
/// 
/// </summary>
public class JsonLobbyCompany : ILobbyCompany {

    private ILobbyHandle? m_handle;

    /// <summary>
    /// 
    /// </summary>
    public bool IsAuto { get; }

    /// <summary>
    /// 
    /// </summary>
    public bool IsNone { get; }

    /// <summary>
    /// 
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// 
    /// </summary>
    public string Army { get; }

    /// <summary>
    /// 
    /// </summary>
    public float Strength { get; }

    /// <summary>
    /// 
    /// </summary>
    public string Specialisation { get; }

    /// <summary>
    /// 
    /// </summary>
    [JsonIgnore]
    public ILobbyHandle Handle => this.m_handle ?? throw new Exception("Lobby handle instance not set");

    /// <summary>
    /// 
    /// </summary>
    /// <param name="IsAuto"></param>
    /// <param name="IsNone"></param>
    /// <param name="Name"></param>
    /// <param name="Army"></param>
    /// <param name="Strength"></param>
    /// <param name="Specialisation"></param>
    [JsonConstructor]
    public JsonLobbyCompany(bool IsAuto, bool IsNone, string Name, string Army, float Strength, string Specialisation) {
        this.IsAuto = IsAuto;
        this.IsNone = IsNone;
        this.Name = Name;
        this.Army = Army;
        this.Strength = Strength;
        this.Specialisation = Specialisation;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="handle"></param>
    public void SetHandle(ILobbyHandle handle) {
        if (handle is null) {
            throw new ArgumentNullException(nameof(handle));
        }
        this.m_handle = handle;
    }

}

public class JsonLobbyCompanyConverter : JsonConverter<ILobbyCompany> {
    public override ILobbyCompany? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => JsonSerializer.Deserialize<JsonLobbyCompany>(ref reader, options);

    public override void Write(Utf8JsonWriter writer, ILobbyCompany value, JsonSerializerOptions options) {
        if (value is not JsonLobbyCompany json)
            throw new InvalidOperationException();
        JsonSerializer.Serialize(writer, json, options);
    }
}
