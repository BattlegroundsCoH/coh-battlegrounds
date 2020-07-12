using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Battlegrounds.Json {
    
    /// <summary>
    /// Static utility class for parsing a Json file.
    /// </summary>
    public static class JsonParser {

        /// <summary>
        /// Parse a json file (Will only parse it - may parse syntactically incorrect json files!) and extract the top-level item.
        /// </summary>
        /// <typeparam name="T">The object type to retrieve from the file. Implements <see cref="IJsonObject"/>.</typeparam>
        /// <param name="jsonfile">The path of the Json file to open and parse.</param>
        /// <returns>The default value if parsing failed. Otherwise the object that was expected.</returns>
        public static T Parse<T>(string jsonfile) where T : IJsonObject {
            if (File.Exists(jsonfile)) {
                List<IJsonElement> topElements = Parse(jsonfile);
                if (topElements.Count == 1) {
                    return (T)(topElements[0] as IJsonObject);
                } else {
                    return default;
                }
            } else {
                return default;
            }
        }

        /// <summary>
        /// Parse a json string (Will only parse it - may parse syntactically incorrect json!) and extract the top-level item.
        /// </summary>
        /// <typeparam name="T">The object type to retrieve from the file. Implements <see cref="IJsonObject"/>.</typeparam>
        /// <param name="jsoncontent">The json string parse and interpret.</param>
        /// <returns>The default value if parsing failed. Otherwise the object that was expected.</returns>
        public static T ParseString<T>(string jsoncontent) where T : IJsonObject {
            List<IJsonElement> topElements = ParseString(jsoncontent);
            if (topElements.Count == 1) {
                return (T)(topElements[0] as IJsonObject);
            } else {
                return default;
            }
        }

        /// <summary>
        /// Parse a json file (Will only parse it - may parse syntactically incorrect json files!)
        /// </summary>
        /// <param name="jsonfile">The path of the Json file to open and parse.</param>
        /// <returns>A <see cref="List{IJsonElement}"/> containing all top-level elements in the json file.</returns>
        public static List<IJsonElement> Parse(string jsonfile) {

            if (!File.Exists(jsonfile)) {
                return new List<IJsonElement>();
            }

            // All contents
            string contents = File.ReadAllText(jsonfile);

            // Parse the string
            return ParseString(contents);

        }

        /// <summary>
        /// Parse a json string (Will only parse it - may parse syntactically incorrect json!)
        /// </summary>
        /// <param name="jsonstring">The Json string to parse and interpret.</param>
        /// <returns>A <see cref="List{IJsonElement}"/> containing all top-level elements in the json string.</returns>
        public static List<IJsonElement> ParseString(string jsonstring) {

            List<IJsonElement> objs = new List<IJsonElement>();

            int i = 0;
            while (i < jsonstring.Length) {

                ParseNext(ref i, jsonstring, out object result);

                if (result is IJsonElement jsle) {
                    objs.Add(jsle);
                }

                i++;

            }

            return objs;

        }

        private static void ParseNext(ref int i, string contents, out object result) {

            result = "";

            if (i >= contents.Length) {
                return;
            }

            if (contents[i] == '[') {

                i++;

                JsonArray array = new JsonArray();
                StringBuilder sb = new StringBuilder();

                while (i < contents.Length) {
                    
                    if (contents[i] == ']') {

                        if (sb.Length > 0) {
                            array.Add(new JsonValue(sb.ToString().Trim('\"')));
                        }

                        i++;
                        break;
                    }

                    ParseNext(ref i, contents, out object o);

                    if (o is IJsonElement e) {
                        array.Add(e);
                        sb.Clear();
                        i++;
                    } else if (contents[i] == ',') {
                        array.Add(new JsonValue(sb.ToString().Trim('\"')));
                        sb.Clear();
                        i++;
                    } else {
                        if (!char.IsWhiteSpace((o as string)[0])) {
                            sb.Append(o as string);
                        }
                    }

                }

                result = array;

            } else if (contents[i] == '{') {

                i++;

                Dictionary<string, object> value_set = new Dictionary<string, object>();
                StringBuilder ln = new StringBuilder();

                while (i < contents.Length) {
                    
                    if (contents[i] == '}') {
                        i++;
                        break;
                    }

                    ParseNext(ref i, contents, out object o);

                    if (o is string) {
                        string s = (o as string);
                        if (s.Length > 0) {
                            ln.Append(s);
                        }
                    } else {
                        if (o is IJsonElement e) {

                            if (ln.Length > 1) {

                                // Use regex to find the key
                                Match r = Regex.Match(ln.ToString(), @"\s*\""(?<key>\S*|\s*)\""\s*:\s*");

                                // Clear the string builder
                                ln.Clear();

                                // If success
                                if (r.Success) {
                                    value_set.Add(r.Groups["key"].Value, e);
                                } else {
                                    throw new JsonSyntaxException("Expected key but found value!");
                                }

                            } else {
                                throw new JsonSyntaxException("Expected key but found value!");
                            }

                        } else {
                            // TODO: Handle
                            throw new NotImplementedException();
                        }
                    }

                    if (ln.ToString().EndsWith(',') && ln.ToString().Count(x => x == '\"') % 2 == 0 && ln.Length > 2) {

                        var kv = ParseKV(ln.ToString());
                        value_set.Add(kv.Key, kv.Value);
                        ln.Clear();

                    }

                }

                var last = ParseKV(ln.ToString());
                value_set.Add(last.Key, last.Value);

                result = IJsonObject.Deserialize(value_set);

            } else {

                result = result as string + contents[i];
                i++;

            }

        }

        private static KeyValuePair<string, object> ParseKV(string kv) {

            // Match with regular expression
            Match v = Regex.Match(kv, @"\s*(?<key>\s*\""\S*\"")\s*:\s*(?<val>(\""(\s|\S)*\"")|(\d*))\s*");

            // Find the key and the value
            string key = v.Groups["key"].Value.Trim('\"');
            string value = v.Groups["val"].Value.Trim('\"');

            // Return key value pair
            return new KeyValuePair<string, object>(key, value);

        }

    }

}
