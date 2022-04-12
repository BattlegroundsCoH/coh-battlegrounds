using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

using Battlegrounds.Functional;

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

        /// <summary>
        /// Language not defined
        /// </summary>
        Undefined = -1,

        /// <summary>
        /// Default language (English)
        /// </summary>
        Default = 0,

        /// <summary>
        /// English language
        /// </summary>
        English = Default,

        /// <summary>
        /// German language
        /// </summary>
        German,

        /// <summary>
        /// French language
        /// </summary>
        French,

        /// <summary>
        /// Spanish language
        /// </summary>
        Spanish,
        
        //Russian, // Leave Ukraine, then maybe

    }

    /// <summary>
    /// Represents a database of localized strings.
    /// </summary>
    public class Localize {

        /// <summary>
        /// Represents undefined (default) locale source.
        /// </summary>
        public const string UndefinedSource = "Default";

        private readonly Dictionary<LocaleKey, string> m_allText;
        private readonly Dictionary<Type, LocaleConverter> m_converters;

        /// <summary>
        /// Get the locale language being stored.
        /// </summary>
        public LocaleLanguage Language { get; }

        /// <summary>
        /// Get dictionary containing a specific converter for a specific type.
        /// </summary>
        public Dictionary<Type, LocaleConverter> Converters => this.m_converters;

        /// <summary>
        /// Initialize a new <see cref="Localize"/> class with language set for <paramref name="language"/>.
        /// </summary>
        /// <param name="language">The <see cref="LocaleLanguage"/> to contain localized data for.</param>
        public Localize(LocaleLanguage language) {

            // Set language
            this.Language = language;

            // Init private fields
            this.m_allText = new();
            this.m_converters = new();
            
            // Register some simple converters
            this.RegisterObjectConverter(new TimespanLocaleConverter());

        }

        /// <summary>
        /// Register a new object to localised string converter. Throws an exception if a converter has already been registered.
        /// </summary>
        /// <param name="converter">The converter to register.</param>
        /// <exception cref="Exception"></exception>
        public void RegisterObjectConverter(LocaleConverter converter) {
            if (this.m_converters.ContainsKey(converter.ConvertType))
                throw new Exception("Converter already registered for type.");
            else 
                this.m_converters[converter.ConvertType] = converter; 
        }

        /// <summary>
        /// Load locale file from a disk file.
        /// </summary>
        /// <param name="localefilepath">Relative path to locale file.</param>
        /// <returns>If loaded successfully <see langword="true"/> is returned; Otherwise <see langword="false"/>.</returns>
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
        /// Load locale file from memory
        /// </summary>
        /// <param name="memorypath">In-memory path.</param>
        /// <returns>If loaded successfully <see langword="true"/> is returned; Otherwise <see langword="false"/>.</returns>
        public bool LoadLocaleFileFromMemory(string memorypath) => LoadLocaleFileFromMemory(memorypath, Assembly.GetExecutingAssembly());

        /// <summary>
        /// Load locale file from memory
        /// </summary>
        /// <param name="memorypath">The path of the memory file</param>
        /// <param name="assembly">The assembly to load data from.</param>
        /// <param name="sourceID">The localise source being read from.</param>
        /// <returns>If loaded successfully <see langword="true"/> is returned; Otherwise <see langword="false"/>.</returns>
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
        /// Load locale file from memory
        /// </summary>
        /// <param name="memoryStream">The stream to read memory contents from.</param>
        /// <param name="sourceID">The localise source being read from.</param>
        /// <returns>If loaded successfully <see langword="true"/> is returned; Otherwise <see langword="false"/>.</returns>
        public bool LoadLocaleFileFromMemory(Stream memoryStream, string sourceID = UndefinedSource) {

            // Read contents
            using StreamReader sr = new StreamReader(memoryStream, Encoding.Unicode);
            StringBuilder sb = new StringBuilder();
            while (sr.ReadLine() is string s) {
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

            // Add keys
            if (!AddKeys(kvps)) {
                return false;
            }

            // Return true
            return true;

        }

        private bool AddKeys((LocaleKey, string)[] entries) {

            bool dups = false;

            // Loop through them all
            foreach (var (k, v) in entries) {
                if (!this.m_allText.ContainsKey(k)) {
                    this.m_allText[k] = v;
                } else {
                    Trace.WriteLine($"Duplicate locale entry '{k}' (e = \"{this.m_allText[k]}\", n = \"{v}\") (language is '{this.Language}')", "Localize");
                    dups = true;
                }
            }

            // return whatever dups is not
            return !dups;

        }

        /// <summary>
        /// Load a binary locale file
        /// </summary>
        /// <param name="stream">The memory stream to load binary locale file from.</param>
        /// <param name="sourceID">The localise source being read from.</param>
        /// <returns>If loaded successfully <see langword="true"/> is returned; Otherwise <see langword="false"/>.</returns>
        public bool LoadBinaryLocaleFile(MemoryStream stream, string sourceID = UndefinedSource) {

            // Define file
            LocalizedFile lf = new LocalizedFile(sourceID);
            if (!lf.LoadFromBinary(stream)) {
                return false;
            }

            // Get relevant keys
            var kvps = lf.GetLanguage(this.Language);
            if (!this.AddKeys(kvps)) {
                return false;
            }

            // Return true;
            return true;

        }

        /// <summary>
        /// Get the UTF-16 encoded string represented by the <paramref name="key"/>.
        /// </summary>
        /// <param name="key">The <see cref="LocaleKey"/> to use when locating the string.</param>
        /// <returns>The UTF-16 encoded string sought after if present in system. Otherwise <paramref name="key"/>.LocaleID is returned.</returns>
        public string GetString(LocaleKey key) {
            if (string.IsNullOrEmpty(key.LocaleID)) {
                return string.Empty;
            }
            if (this.m_allText.ContainsKey(key)) {
                return this.m_allText[key];
            } else {
                if (key.LocaleSource == UndefinedSource) {
                    if (this.m_allText.FirstOrDefault(x => x.Key.LocaleID == key.LocaleID) is KeyValuePair<LocaleKey, string> kvp) {
                        if (kvp.Value != null) {
                            return kvp.Value;
                        } else {
                            return $"LOC: {key.LocaleID}";
                        }
                    }
                }
                Trace.WriteLine($"Undefined locale key '{key.LocaleID}'@{key.LocaleSource}. (Lang : {this.Language})", nameof(Localize));
                return key.LocaleID;
            }
        }

        /// <summary>
        /// Get the first UTF-16 encoded string represented by the <paramref name="key"/> and fill in string parameters.
        /// </summary>
        /// <param name="key">The raw locale key to use when locating the string.</param>
        /// <param name="args">The argument values to give to the string parameters.</param>
        /// <returns>The UTF-16 encoded <see cref="string"/> with filled parameters sought after if present in system. Otherwsie <paramref name="key"/> is returned.</returns>
        public string GetString(LocaleKey key, params object[] args) {
            string str = this.GetString(key);
            for (int i = 0; i < args.Length; i++) {
                string value = TryGetObjectAsString(args[i]) switch {
                    LocaleKey lc => this.GetString(lc),
                    _ => args[i].ToString()
                };
                str = str.Replace($"{{{i}}}", value);
            }
            return str;
        }

        /// <summary>
        /// Get the first UTF-16 encoded string represented by the <paramref name="key"/>.
        /// </summary>
        /// <param name="key">The raw locale key to use when locating the string.</param>
        /// <returns>The UTF-16 encoded <see cref="string"/> sought after if present in system. Otherwsie <paramref name="key"/> is returned.</returns>
        public string GetString(string key) => this.GetString(new LocaleKey(key));

        /// <summary>
        /// Get the first UTF-16 encoded string represented by the <paramref name="key"/> and fill in string parameters.
        /// </summary>
        /// <param name="key">The raw locale key to use when locating the string.</param>
        /// <param name="args">The argument values to give to the string parameters.</param>
        /// <returns>The UTF-16 encoded <see cref="string"/> with filled parameters sought after if present in system. Otherwsie <paramref name="key"/> is returned.</returns>
        public string GetString(string key, params object[] args) => this.GetString(new LocaleKey(key), args.Map(TryGetObjectAsString));

        /// <summary>
        /// Get the UTF-16 encoded string represented by the enum value.
        /// </summary>
        /// <typeparam name="T">The enum type to get display value of</typeparam>
        /// <param name="enumValue">The enum value to find name of.</param>
        /// <param name="sourceID">The specific locale source to locate.</param>
        /// <returns>If enum value is defined, the display value is returned. Otherwise the used key is returned.</returns>
        public string GetEnum<T>(T enumValue, string sourceID = UndefinedSource) where T : Enum {
            string lookup = $"{typeof(T).Name}_{enumValue}";
            return this.GetString(new LocaleKey(lookup, sourceID));
        }

        /// <summary>
        /// Get an object as a localised string using a converter specified by the <see cref="Converters"/> dictionary.
        /// </summary>
        /// <param name="obj">The object to localise.</param>
        /// <returns>The localised object.</returns>
        /// <exception cref="Exception"></exception>
        public string GetObjectAsString(object obj) {
            if (this.m_converters.TryGetValue(obj.GetType(), out var converter))
                return converter.GetLocalisedString(this, obj);
            else
                throw new Exception($"Converter does not exist for object of type {obj.GetType().Name}.");
        }

        private object TryGetObjectAsString(object obj) {
            if (this.m_converters.TryGetValue(obj.GetType(), out var converter))
                return converter.GetLocalisedString(this, obj);
            else
                return obj;
        }

        /// <summary>
        /// Get the numeric suffix to a number (st, nd, rd, th).
        /// </summary>
        /// <param name="n">The number to get the suffix for.</param>
        /// <returns>A string with the number and its proper suffix.</returns>
        public string GetNumberSuffix(int n) {
            string s = n.ToString();
            if (s[^1] == '1') {
                return $"{n}{this.GetString("ST")}";
            } else if (s[^1] == '2') {
                return $"{n}{this.GetString("ND")}";
            } else if (s[^1] == '3') {
                return $"{n}{this.GetString("RD")}";
            } else {
                return $"{n}{this.GetString("TH")}";
            }
        }

    }

}
