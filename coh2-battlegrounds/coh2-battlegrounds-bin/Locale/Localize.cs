using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Battlegrounds.Locale {

    /// <summary>
    /// Represents a key to use when indexing a localized string.
    /// </summary>
    /// <param name="LocaleID">The actual ID to lookup.</param>
    /// <param name="LocaleSource">The potential source to select from.</param>
    public record LocaleKey(string LocaleID, string LocaleSource = Localize.UndefinedSource);

    /// <summary>
    /// Enum representation of supported languages.
    /// </summary>
    public enum LocaleLanguage {
        Undefined = -1,
        Default = 0,
        English = Default,
        German,
        French,
        Spanish,
        Russian,
    }

    /// <summary>
    /// Represents a database of localized strings.
    /// </summary>
    public class Localize {

        /// <summary>
        /// Represents undefined (default) locale source.
        /// </summary>
        public const string UndefinedSource = "Default";

        private Dictionary<LocaleKey, string> m_allText;

        /// <summary>
        /// Get the locale language being stored.
        /// </summary>
        public LocaleLanguage Language { get; }

        /// <summary>
        /// Initialize a new <see cref="Localize"/> class with language set for <paramref name="language"/>.
        /// </summary>
        /// <param name="language">The <see cref="LocaleLanguage"/> to contain localized data for.</param>
        public Localize(LocaleLanguage language) {
            this.Language = language;
            this.m_allText = new Dictionary<LocaleKey, string>();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="localefilepath"></param>
        /// <returns></returns>
        public bool LoadLocaleFile(string localefilepath) {
            if (File.Exists(localefilepath)) {
                var loc = new LocalizedFile(Path.GetFileNameWithoutExtension(localefilepath));
                if (loc.LoadFromString(File.ReadAllText(localefilepath))) {
                    return this.LoadLocaleFile(loc);
                }
            }
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="memorypath"></param>
        /// <returns></returns>
        public bool LoadLocaleFileFromMemory(string memorypath) => LoadLocaleFileFromMemory(memorypath, Assembly.GetExecutingAssembly());

        /// <summary>
        /// 
        /// </summary>
        /// <param name="memorypath"></param>
        /// <param name="assembly"></param>
        /// <returns></returns>
        public bool LoadLocaleFileFromMemory(string memorypath, Assembly assembly, string sourceID = null) {
            if (assembly.GetManifestResourceStream(memorypath) is Stream fs) {
                if (sourceID is null) {
                    int l = memorypath.LastIndexOf('.');
                    if (l > 0) {
                        int sl = memorypath.LastIndexOf('.', l - 1);
                        if (sl > 0) {
                            sourceID = memorypath[sl..l];
                        } else {
                            sourceID = UndefinedSource;
                        }
                    } else {
                        sourceID = UndefinedSource;
                    }
                }
                return LoadLocaleFileFromMemory(fs, sourceID);
            } else {
                return false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="memoryStream"></param>
        /// <param name="sourceID"></param>
        /// <returns></returns>
        public bool LoadLocaleFileFromMemory(Stream memoryStream, string sourceID = UndefinedSource) {
            
            // Read contents
            using StreamReader sr = new StreamReader(memoryStream, Encoding.Unicode);
            StringBuilder sb = new StringBuilder();
            while(sr.ReadLine() is string s) {
                sb.AppendLine(s);
            }

            // Parse and load
            var loc = new LocalizedFile(sourceID);
            if (loc.LoadFromString(sb.ToString())) {
                return LoadLocaleFile(loc);
            }
            
            // Return false (Failed to load locale)
            return false;

        }

        private bool LoadLocaleFile(LocalizedFile fl) {

            // Get all keys for THIS language
            var kvps = fl.GetLanguage(this.Language);

            // Loop through them all
            foreach(var (k, v) in kvps) {
                if (!this.m_allText.ContainsKey(k)) {
                    this.m_allText[k] = v;
                } else {
                    Trace.WriteLine($"Duplicate locale entry '' (e = \"{this.m_allText[k]}\", n = \"{v}\") (language is '{this.Language}')", "Localize");
                    return false;
                }
            }

            // Return true
            return true;

        }

        /// <summary>
        /// Get the UTF-16 encoded string represented by the <paramref name="key"/>.
        /// </summary>
        /// <param name="key">The <see cref="LocaleKey"/> to use when locating the string.</param>
        /// <returns>The UTF-16 encoded string sought after if present in system. Otherwise <paramref name="key"/>.LocaleID is returned.</returns>
        public string GetString(LocaleKey key) { 
            if (this.m_allText.ContainsKey(key)) {
                return this.m_allText[key];
            } else {
                if (key.LocaleSource == UndefinedSource) {
                    if (this.m_allText.FirstOrDefault(x => x.Key.LocaleID == key.LocaleID) is KeyValuePair<LocaleKey, string> kvp) {
                        return kvp.Value;
                    }
                }
                Trace.WriteLine($"Undefined locale key '{key.LocaleID}'@{key.LocaleSource}. (Lang : {this.Language})", "Localize");
                return key.LocaleID;
            }
        }

        /// <summary>
        /// Get the first UTF-16 encoded string represented by the <paramref name="key"/>.
        /// </summary>
        /// <param name="key">The raw locale key to use when locating the string.</param>
        /// <returns>The UTF-16 encoded string sought after if present in system. Otherwsie <paramref name="key"/> is returned.</returns>
        public string GetString(string key) => this.GetString(new LocaleKey(key));

    }

}
