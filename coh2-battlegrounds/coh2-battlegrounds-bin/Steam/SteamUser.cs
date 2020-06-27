using System;

using Battlegrounds.Game.Database.json;
using Battlegrounds.Online;

namespace Battlegrounds.Steam {

    /// <summary>
    /// Represents a Steam User by ID and name. Can be serialised as a Json object. This class cannot be inherited.
    /// </summary>
    public sealed class SteamUser : IJsonObject {
    
        /// <summary>
        /// The display name of a <see cref="SteamUser"/>. (Not the actual account name!)
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The <see cref="UInt64"/> user ID.
        /// </summary>
        public ulong ID { get; }

        private SteamUser(ulong steamUID) {
            this.ID = steamUID;
            this.Name = "";
        }

        /// <summary>
        /// Update the name of the steam user
        /// </summary>
        public bool UpdateName() {

            // So we don't have any real association with Steam
            // Meaning we'll have to "stumble" upon this data from somewhere else (like a 3rd party website freely giving away this information)
            // https://steamidfinder.com/lookup/76561198003529969/ ==> Will give a website containing the information

            string str = SourceDownloader.DownloadSourceCode($"https://steamidfinder.com/lookup/{this.ID}/");
            //File.WriteAllText("ts.html", str);

            const string lookfor = "name <code>";
            int pos = str.IndexOf(lookfor);

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
        /// Find a user by their ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static SteamUser FromID(ulong id) {

            // Create the user
            SteamUser user = new SteamUser(id);

            // Update the user name (and make sure there's no problems fetching the name)
            if (!user.UpdateName()) {
                throw new ArgumentException($"Invalid steam user ID: {id}");
            }

            // Return user
            return user;

        }

    }

}
