using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Battlegrounds.Steam;

public class Vdf {

    private readonly struct VDFToken {
        public readonly string Str;
        public readonly int TokTy;
        public VDFToken(string str, int ty) {
            this.Str = str;
            this.TokTy = ty;
        }
    }

    private static readonly Regex TokenRegex = new(@"(?<lbrace>{)|(?<rbrace>})|(?<str>\"".*?(?<!\\)\"")");

    private readonly Dictionary<string, object> m_vdfDictionary;

    private Vdf(Dictionary<string, object> vdfDictionary) {
        this.m_vdfDictionary = vdfDictionary;
    }

    public Vdf this[string key] {
        get => new(this.m_vdfDictionary[key] as Dictionary<string, object> ?? throw new KeyNotFoundException(key));
    }

    public string Str(string str) => this.m_vdfDictionary[str] as string ?? throw new Exception("Table cannot be converted into a string");

    public Dictionary<string, object> Table(string str) => this.m_vdfDictionary[str] as Dictionary<string, object> ?? throw new KeyNotFoundException(str);

    public static Vdf? FromFile(string filepath) {

        // Make sure file exists
        if (!File.Exists(filepath)) {
            return null;
        }

        // Open file
        using var fs = File.OpenRead(filepath);
        using var sr = new StreamReader(fs);

        // Prepare buffer
        StringBuilder sb = new();

        // Create token list
        List<VDFToken> tokens = new();

        // Read while true
        while (!sr.EndOfStream) {

            // Read next
            int cha = sr.Read();

            // Append
            sb.Append((char)cha);

            // Try match
            if (TokenRegex.Match(sb.ToString()) is Match m && m.Success) {
                sb.Clear();
                for (int i = 1; i < m.Groups.Count; i++) {
                    if (m.Groups[i].Value.Length > 0) {
                        tokens.Add(new(m.Groups[i].Value.Trim('\"'), i - 1));
                        break;
                    }
                }
            }

        }

        // Set dummy index
        int j = 0;

        // Parse
        var vdfcontent = Parse(tokens, ref j);

        return new(vdfcontent);

    }

    private static Dictionary<string, object> Parse(List<VDFToken> tokens, ref int i) {

        // Grab key
        string key = GetString(tokens[i++]);

        // If open
        if (tokens[i].TokTy == 0) {
            i++;

            Dictionary<string, object> res = new();
            while (i < tokens.Count) {

                if (tokens[i].TokTy == 1) {
                    break;
                } else {
                    var kv = Parse(tokens, ref i);
                    foreach (var (k,v) in kv) {
                        res[k] = v;
                    }
                }

                i++;

            }

            return new Dictionary<string, object>() { [key] = res };

        } else if (tokens[i].TokTy == 2) {
            return new Dictionary<string, object>() { [key] = tokens[i].Str };
        } else {
            throw new Exception("Unexpected close token!");
        }

    }

    private static string GetString(VDFToken t) => t.TokTy == 2 ? t.Str : throw new Exception("Expected string token but found none!");

}
