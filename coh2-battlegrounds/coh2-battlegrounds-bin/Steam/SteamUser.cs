using System;
using System.Text.Json;
using System.Text.Json.Serialization;

using Battlegrounds.Online;

namespace Battlegrounds.Steam {

    /// <summary>
    /// Represents a Steam User by ID and name. Implements <see cref="IJsonObject"/>. This class cannot be inherited.
    /// </summary>
    [JsonConverter(typeof(SteamUserJsonConverter))]
    public sealed class SteamUser {

        /// <summary>
        /// The display name of a <see cref="SteamUser"/>. (Not the actual account name!)
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The <see cref="ulong"/> user ID.
        /// </summary>
        public ulong ID { get; }

        internal SteamUser(ulong steamUID) {
            this.ID = steamUID;
            this.Name = "";
        }

        /// <summary>
        /// Update the name of the steam user
        /// </summary>
        public bool UpdateName() {

            // https://steamidfinder.com/lookup/76561198003529969/ ==> Will give the needed information

            string str = SourceDownloader.DownloadSourceCode($"https://steamidfinder.com/lookup/{this.ID}/");

            const string lookfor = "name <code>";
            int pos = str.IndexOf(lookfor, StringComparison.InvariantCulture);

            if (pos != -1) {
                int len = str.IndexOf('<', pos + lookfor.Length + 1) - (pos + lookfor.Length);
                this.Name = str.Substring(pos + lookfor.Length, len);
                return true;
            } else {
                this.Name = "Steam name unknown!";
                return false;
            }

        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString() => this.Name;

        /// <summary>
        /// Find a user by their steam 64-bit ID.
        /// </summary>
        /// <param name="id">The unique steam ID to use when retrieving basic user data.</param>
        /// <returns>The <see cref="SteamUser"/> with steam ID.</returns>
        /// <exception cref="ArgumentException"/>
        public static SteamUser FromID(ulong id) {

            // Create the user
            SteamUser user = new(id);

            // Update the user name (and make sure there's no problems fetching the name)
            if (!user.UpdateName()) {
                throw new ArgumentException($"Invalid steam user ID: {id}");
            }

            // Return user
            return user;

        }

    }

    public class SteamUserJsonConverter : JsonConverter<SteamUser> {

        public override SteamUser Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => SteamUser.FromID(reader.GetUInt64());

        public override void Write(Utf8JsonWriter writer, SteamUser value, JsonSerializerOptions options) => writer.WriteNumberValue(value.ID);

    }

}
