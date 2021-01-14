using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

using RegexMatch = System.Text.RegularExpressions.Match;

namespace Battlegrounds.Game.DataSource {
    
    /// <summary>
    /// 
    /// </summary>
    public class UcsFile {

        private Dictionary<uint, string> m_content;

        private UcsFile() {
            this.m_content = new Dictionary<uint, string>();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="locID"></param>
        /// <returns></returns>
        public string this[uint locID] => m_content[locID];

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ucsFilePath"></param>
        /// <returns></returns>
        public static UcsFile LoadFromFile(string ucsFilePath) {

            UcsFile file = new UcsFile();

            using (StreamReader sr = new StreamReader(File.OpenRead(ucsFilePath), Encoding.UTF8)) {

                string line;

                Regex reg = new Regex(@"(?<key>\d+)\s*(?<value>\S*)");

                while(!string.IsNullOrEmpty(line = sr.ReadLine())) {

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
