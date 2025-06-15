using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Battlegrounds.Steam;

/// <summary>
/// Represents a Steam User by ID and name. This class cannot be inherited.
/// </summary>
[JsonConverter(typeof(SteamUserJsonConverter))]
public sealed class SteamUser {

    private string m_displayName;

    /// <summary>
    /// The display name of a <see cref="SteamUser"/>. (Not the actual account name!)
    /// </summary>
    public string Name {
        get => string.IsNullOrEmpty(this.m_displayName) ? this.UpdateName() : this.m_displayName;
        set => this.m_displayName = value;
    }

    /// <summary>
    /// The <see cref="ulong"/> user ID.
    /// </summary>
    public ulong ID { get; }

    internal SteamUser(ulong steamUID) {
        this.ID = steamUID;
        this.m_displayName = "";
    }

    /// <summary>
    /// Update the name of the steam user
    /// </summary>
    private string UpdateName() {
        var user = SteamInstance.FromLocalInstall();
        if (user is not null) {
            return this.Name = user.Name;
        }
        return "Steam User Not Found";
    }

    /// <summary>
    /// Returns a string that represents the current object.
    /// </summary>
    /// <returns>A string that represents the current object.</returns>
    public override string ToString() => this.Name;

    /// <summary>
    /// Creates a temporary <see cref="SteamUser"/> instance with the specified identifier and display name
    /// </summary>
    /// <param name="steamUID">The steam ID</param>
    /// <param name="displayName">The steam display name (NOT account name)</param>
    /// <returns>A <see cref="SteamUser"/> instance</returns>
    public static SteamUser CreateTempUser(ulong steamUID, string displayName) => new SteamUser(steamUID) { m_displayName = displayName };

}

/// <summary>
/// Sealed converter class for converting a <see cref="SteamUser"/> instance into and from json format.
/// </summary>
public sealed class SteamUserJsonConverter : JsonConverter<SteamUser> {

    /// <inheritdoc/>
    public override SteamUser Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => new(reader.GetUInt64());

    /// <inheritdoc/>
    public override void Write(Utf8JsonWriter writer, SteamUser value, JsonSerializerOptions options) => writer.WriteNumberValue(value.ID);

}
