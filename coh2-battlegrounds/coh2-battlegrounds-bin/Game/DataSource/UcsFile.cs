using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using RegexMatch = System.Text.RegularExpressions.Match;

namespace Battlegrounds.Game.DataSource {

    /// <summary>
    /// Represents a localized file used by Company of Heroes to store text data.
    /// </summary>
    public class UcsFile {

        private readonly Dictionary<uint, string> m_content;

        /// <summary>
        /// Get the max key index.
        /// </summary>
        public uint MaxKey => this.m_content.Max(x => x.Key);

        /// <summary>
        /// Get the min key index.
        /// </summary>
        public uint MinKey => this.m_content.Min(x => x.Key);

        /// <summary>
        /// Get the amount of keys.
        /// </summary>
        public int KeyCount => this.m_content.Count;

        private UcsFile() {
            this.m_content = new Dictionary<uint, string>();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="content"></param>
        public UcsFile(string content) {
            this.m_content = new() {
                [0] = content
            };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public UcsFile CreateKey(uint key, string value) {
            this.m_content[key] = value;
            return this;
        }

        /// <summary>
        /// Get the UCS localized string identified by the <paramref name="locID"/> key.
        /// </summary>
        /// <param name="locID">The numeric locale key to use when index.</param>
        /// <returns>The localized string identified by <paramref name="locID"/> or an error message if no string is found.</returns>
        public string this[uint locID] => this.m_content.ContainsKey(locID) ? this.m_content[locID] : $"${locID} No key";

        /// <summary>
        /// Get a <see cref="UcsString"/> containing basic reference information to the <paramref name="locID"/>.
        /// </summary>
        /// <param name="locID">The locale key ID to reference.</param>
        /// <returns>The <see cref="UcsString"/> referencing <paramref name="locID"/>.</returns>
        public UcsString GetRef(uint locID) => new(this, locID);

        /// <summary>
        /// Clone the current <see cref="UcsFile"/> instance with a total copy of all strings.
        /// </summary>
        /// <returns></returns>
        public UcsFile Clone() {

            // Create new file
            UcsFile file = new();
            foreach ((uint k, string v) in this.m_content) {
                file.m_content.Add(k, new(v)); // Copy string
            }

            // Return the file
            return file;

        }

        /// <summary>
        /// Update the string found at <paramref name="key"/>.
        /// </summary>
        /// <param name="key">The key to update.</param>
        /// <param name="newValue">The new value put.</param>
        public void UpdateString(uint key, string newValue) {
            if (!this.m_content.ContainsKey(key)) {
                throw new ArgumentOutOfRangeException(nameof(key), key, "Cannot update locale string of unregistered key.");
            }
            this.m_content[key] = newValue;
        }

        /// <summary>
        /// Save the current <see cref="UcsFile"/> instance to a file.
        /// </summary>
        /// <param name="ucsFilePath">The path of the UCS file to save to.</param>
        public void SaveToFile(string ucsFilePath) {

            // Delete file if it already exists
            if (File.Exists(ucsFilePath)) {
                File.Delete(ucsFilePath);
            }

            // Open stream
            using StreamWriter ws = new(File.OpenWrite(ucsFilePath), Encoding.UTF8);
            foreach ((uint k, string v) in this.m_content) {
                ws.WriteLine($"{k}\t{v}");
            }

        }

        /// <summary>
        /// The locale regex matcher.
        /// </summary>
        private static readonly Regex locRegex = new Regex(@"(?<key>\d+)\s*(?<value>.*)");

        /// <summary>
        /// Load a UCS file with UTF-8 encoding.
        /// </summary>
        /// <param name="ucsFilePath">The path to the UCS file.</param>
        /// <returns>A <see cref="UcsFile"/> instance containing the keys and values of the file.</returns>
        public static UcsFile LoadFromFile(string ucsFilePath) {

            // Create UCS file
            UcsFile file = new UcsFile();

            // Read UCS file.
            using (StreamReader sr = new StreamReader(File.OpenRead(ucsFilePath), Encoding.UTF8)) {

                // Line str
                string line;

                // While line is not null or empty.
                while (!string.IsNullOrEmpty(line = sr.ReadLine())) {

                    // Match with regex
                    RegexMatch match = locRegex.Match(line);

                    // If match
                    if (match.Success) {

                        // Try get key
                        if (uint.TryParse(match.Groups["key"].Value, out uint key)) {

                            // Add locstring
                            file.m_content.Add(key, match.Groups["value"].Value);

                        }

                    }

                }

            }

            // Return the created file.
            return file;

        }

    }

}
