using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

using RegexMatch = System.Text.RegularExpressions.Match;

namespace Battlegrounds.Game.DataSource {

    /// <summary>
    /// Represents a localized file used by Company of Heroes to store text data.
    /// </summary>
    public class UcsFile {

        private Dictionary<uint, string> m_content;

        private UcsFile() {
            this.m_content = new Dictionary<uint, string>();
        }

        /// <summary>
        /// Get the UCS localized string identified by the <paramref name="locID"/> key.
        /// </summary>
        /// <param name="locID">The numeric locale key to use when index.</param>
        /// <returns>The localized string identified by <paramref name="locID"/> or an error message if no string is found.</returns>
        public string this[uint locID] => this.m_content.ContainsKey(locID) ? this.m_content[locID] : $"${locID} No key!";

        /// <summary>
        /// Load a UCS file with UTF-8 encoding.
        /// </summary>
        /// <param name="ucsFilePath">The path to the UCS file.</param>
        /// <returns>A <see cref="UcsFile"/> instance containing the keys and values of the file.</returns>
        public static UcsFile LoadFromFile(string ucsFilePath) {

            UcsFile file = new UcsFile();

            using (StreamReader sr = new StreamReader(File.OpenRead(ucsFilePath), Encoding.UTF8)) {

                string line;

                Regex reg = new Regex(@"(?<key>\d+)\s*(?<value>.*)");

                while (!string.IsNullOrEmpty(line = sr.ReadLine())) {

                    RegexMatch match = reg.Match(line);

                    if (match.Success) {

                        if (uint.TryParse(match.Groups["key"].Value, out uint key)) {

                            file.m_content.Add(key, match.Groups["value"].Value);

                        }

                    }

                }

            }

            return file;

        }

    }

}
