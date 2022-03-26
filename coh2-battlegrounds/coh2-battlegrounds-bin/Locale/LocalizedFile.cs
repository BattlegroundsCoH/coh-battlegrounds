using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using Battlegrounds.Functional;
using Battlegrounds.Util;

namespace Battlegrounds.Locale {

    public class LocalizedFile {

        private Dictionary<LocaleLanguage, List<(LocaleKey key, string value)>> m_loadedStrings;
        private string m_sourceID;

        /// <summary>
        /// Get all distinct keys
        /// </summary>
        public string[] DistinctKeys => this.m_loadedStrings.Values.SelectMany(x => x.Select(x => x.key.LocaleID)).Distinct().ToArray();

        public LocalizedFile(string sourceID) {
            this.m_sourceID = sourceID;
            this.m_loadedStrings = new Dictionary<LocaleLanguage, List<(LocaleKey key, string value)>>();
        }

        public bool LoadFromString(string fullString) {

            // Split into lines
            string[] lns = fullString.Split('\n');

            // Make sure we have more than 1 line
            if (lns.Length > 1) {

                bool hasLanguage = false;
                LocaleLanguage lang = LocaleLanguage.Undefined;

                int i = 0;
                while (i < lns.Length) {

                    if (lns[i].Length > 0) {

                        if (lns[i].All(char.IsWhiteSpace)) {
                            i++;
                            continue;
                        }

                        if (lns[i][0] == ' ') {
                            if (hasLanguage) {

                                int j = lns[i].IndexOf(':');

                                if (j == -1) {
                                    throw new InvalidDataException("Expected a ':' symbol following a key, but found none.");
                                }

                                string k = lns[i][0..j].Trim(' ');
                                string v = lns[i][(j + 1)..].Replace('\r', ' ').Trim(' ').Trim('\"');

                                // Add string
                                this.m_loadedStrings[lang].Add((new LocaleKey(k, this.m_sourceID), v));

                            } else {
                                return false;
                            }
                        } else {
                            if (TryGetLanguage(lns[i], out lang)) {
                                if (!this.m_loadedStrings.ContainsKey(lang)) {
                                    this.m_loadedStrings.Add(lang, new List<(LocaleKey key, string value)>());
                                }
                                hasLanguage = true;
                            }
                        }

                    }

                    i++;
                }

                return true;

            }

            // Return false
            return false;

        }

        private static bool TryGetLanguage(string str, out LocaleLanguage lang) {
            int s = str.IndexOf(':');
            if (s > 0) {
                str = str[0..s];
                lang = str.ToLower() switch {
                    "english" => LocaleLanguage.English,
                    "german" => LocaleLanguage.German,
                    "french" => LocaleLanguage.French,
                    "spanish" => LocaleLanguage.Spanish,
                    //"russian" => LocaleLanguage.Russian,
                    _ => LocaleLanguage.Undefined
                };
                return lang != LocaleLanguage.Undefined;
            } else {
                lang = LocaleLanguage.Default;
                return false;
            }
        }

        public (LocaleKey k, string v)[] GetLanguage(LocaleLanguage locale) {
            if (this.m_loadedStrings.ContainsKey(locale)) {
                return this.m_loadedStrings[locale].ToArray();
            } else {
                return Array.Empty<(LocaleKey, string)>();
            }
        }

        public byte[] AsBinary() {

            using MemoryStream ms = new MemoryStream();
            using BinaryWriter bw = new BinaryWriter(ms, Encoding.Unicode);

            byte[] ids = Encoding.Unicode.GetBytes(this.m_sourceID);

            bw.Write(ids.Length);
            bw.Write(ids);

            bw.Write(this.m_loadedStrings.Count);
            this.m_loadedStrings.Keys.ForEach(x => {
                bw.Write((byte)0);
                bw.Write((byte)x);
                bw.Write(this.m_loadedStrings[x].Count);
            });

            int j = 0;
            bw.Write(this.DistinctKeys.Length);
            this.DistinctKeys.ForEach(x => {
                byte[] kc = Encoding.Unicode.GetBytes(x);
                bw.Write((ushort)kc.Length);
                bw.Write(kc);
                bw.Write(j);
                j++;
            });

            // Sort by keys
            this.m_loadedStrings.ForEach(lang => {
                bw.Write((byte)lang.Key);
                lang.Value.ForEach(v => {
                    int k = Array.IndexOf(this.DistinctKeys, v.key.LocaleID);
                    if (k >= 0) {
                        bw.Write(k);
                    } else {
                        bw.Write(-1);
                        byte[] kb = Encoding.Unicode.GetBytes(v.key.LocaleID);
                        bw.Write((ushort)kb.Length);
                        bw.Write(kb);
                    }
                    byte[] sb = Encoding.Unicode.GetBytes(v.value);
                    bw.Write((ushort)sb.Length);
                    bw.Write(sb);
                });
            });

            ms.Position = 0;
            return ms.ToArray();

        }

        public bool LoadFromBinary(MemoryStream stream) {

            // Create binary reader
            using BinaryReader reader = new BinaryReader(stream, Encoding.Unicode, true);

            // Read identifier
            string identifier = reader.ReadUnicodeString();
            if (identifier != this.m_sourceID) {
                if (this.m_sourceID == Localize.UndefinedSource) {
                    this.m_sourceID = identifier;
                } else {
                    return false;
                }
            }

            // Create loaded strings
            var tmp = new Dictionary<LocaleLanguage, int>();

            // Read the amount of languages
            int langCount = reader.ReadInt32();
            for (int i = 0; i < langCount; i++) {
                reader.Skip(1); // skip a single byte
                LocaleLanguage lang = (LocaleLanguage)reader.ReadByte();
                tmp.Add(lang, reader.ReadInt32());
                this.m_loadedStrings.Add(lang, new List<(LocaleKey key, string value)>());
            }

            // Read lookup keys
            int lookupCount = reader.ReadInt32();
            string[] lookup = new string[lookupCount];
            for (int i = 0; i < lookupCount; i++) {
                string str = reader.ReadUnicodeString(reader.ReadUInt16());
                int j = reader.ReadInt32();
                lookup[j] = str;
            }

            // Read to end
            while (!reader.HasReachedEOS()) {
                LocaleLanguage lan = (LocaleLanguage)reader.ReadByte();
                int count = tmp[lan];
                for (int i = 0; i < count; i++) {
                    int lookupID = reader.ReadInt32();
                    string locID = null;
                    if (lookupID == -1) {
                        locID = reader.ReadUnicodeString(reader.ReadUInt16());
                    } else {
                        locID = lookup[lookupID];
                    }
                    this.m_loadedStrings[lan].Add((new LocaleKey(locID, identifier), reader.ReadUnicodeString(reader.ReadUInt16())));
                }
            }

            // Return true
            return true;

        }

    }

}
