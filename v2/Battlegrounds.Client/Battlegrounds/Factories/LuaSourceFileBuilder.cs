using System.Text;
using System.Globalization;
using System.IO;

namespace Battlegrounds.Factories;

public sealed class LuaSourceFileBuilder {

    private readonly List<string> _declarations = [];
    private int _indentLevel = 0;
    private const string IndentString = "    ";

    private string CurrentIndent => string.Concat(Enumerable.Repeat(IndentString, _indentLevel));

    public LuaSourceFileBuilder IncreaseIndent() {
        _indentLevel++;
        return this;
    }

    public LuaSourceFileBuilder DecreaseIndent() {
        if (_indentLevel > 0)
            _indentLevel--;
        return this;
    }

    public LuaSourceFileBuilder DeclareGlobal(string name, string value) {
        _declarations.Add($"{CurrentIndent}{name} = \"{value}\"");
        return this;
    }

    public LuaSourceFileBuilder DeclareGlobal(string name, int value) {
        _declarations.Add($"{CurrentIndent}{name} = {value}");
        return this;
    }

    public LuaSourceFileBuilder DeclareGlobal(string name, bool value) {
        _declarations.Add($"{CurrentIndent}{name} = {value.ToString().ToLower()}");
        return this;
    }

    public LuaSourceFileBuilder DeclareGlobal(string name, double value) {
        _declarations.Add($"{CurrentIndent}{name} = {value.ToString(CultureInfo.InvariantCulture)}");
        return this;
    }

    public LuaSourceFileBuilder DeclareTable(string name, Action<TableBuilder> tableBuilder) {
        var builder = new TableBuilder(_indentLevel);
        tableBuilder(builder);
        _declarations.Add($"{CurrentIndent}{name} = {builder}");
        return this;
    }

    public override string ToString() {
        var sb = new StringBuilder();
        foreach (var declaration in _declarations) {
            sb.AppendLine(declaration);
        }
        return sb.ToString();
    }

    public LuaSourceFileBuilder WriteToFile(string testFilePath) {
        File.WriteAllText(testFilePath, ToString(), Encoding.UTF8);
        return this;
    }

    public class TableBuilder(int parentIndentLevel) {
        private readonly List<string> _elements = [];
        private readonly int _indentLevel = parentIndentLevel + 1;

        private string CurrentIndent => string.Concat(Enumerable.Repeat(IndentString, _indentLevel));
        private string ParentIndent => string.Concat(Enumerable.Repeat(IndentString, _indentLevel - 1));

        public TableBuilder AddValue(string value) {
            _elements.Add($"{CurrentIndent}\"{value}\"");
            return this;
        }

        public TableBuilder AddValue(int value) {
            _elements.Add($"{CurrentIndent}{value}");
            return this;
        }

        public TableBuilder AddValue(bool value) {
            _elements.Add($"{CurrentIndent}{value.ToString().ToLower()}");
            return this;
        }

        public TableBuilder AddValue(double value) {
            _elements.Add($"{CurrentIndent}{value.ToString(CultureInfo.InvariantCulture)}");
            return this;
        }

        public TableBuilder AddKeyValue(string key, string value) {
            _elements.Add($"{CurrentIndent}[\"{key}\"] = \"{value}\"");
            return this;
        }

        public TableBuilder AddKeyValue(string key, bool value) {
            _elements.Add($"{CurrentIndent}[\"{key}\"] = {value.ToString().ToLower()}");
            return this;
        }
        
        public TableBuilder AddKeyValue(string key, int value) {
            _elements.Add($"{CurrentIndent}[\"{key}\"] = {value}");
            return this;
        }

        public TableBuilder AddKeyValue(string key, double value) {
            _elements.Add($"{CurrentIndent}[\"{key}\"] = {value.ToString(CultureInfo.InvariantCulture)}");
            return this;
        }


        public TableBuilder AddKeyValue(int key, string value) {
            _elements.Add($"{CurrentIndent}[{key}] = \"{value}\"");
            return this;
        }

        public TableBuilder AddKeyValue(int key, bool value) {
            _elements.Add($"{CurrentIndent}[{key}] = {value.ToString().ToLower()}");
            return this;
        }
        
        public TableBuilder AddKeyValue(int key, int value) {
            _elements.Add($"{CurrentIndent}[{key}] = {value}");
            return this;
        }

        public TableBuilder AddKeyValue(int key, double value) {
            _elements.Add($"{CurrentIndent}[{key}] = {value.ToString(CultureInfo.InvariantCulture)}");
            return this;
        }

        public TableBuilder AddNestedTable(Action<TableBuilder> tableBuilder) {
            var builder = new TableBuilder(_indentLevel);
            tableBuilder(builder);
            _elements.Add($"{CurrentIndent}{builder}");
            return this;
        }

        public TableBuilder AddNestedTable(string key, Action<TableBuilder> tableBuilder) {
            var builder = new TableBuilder(_indentLevel);
            tableBuilder(builder);
            _elements.Add($"{CurrentIndent}[\"{key}\"] = {builder}");
            return this;
        }

        public TableBuilder AddNestedTable(int key, Action<TableBuilder> tableBuilder) {
            var builder = new TableBuilder(_indentLevel);
            tableBuilder(builder);
            _elements.Add($"{CurrentIndent}[{key}] = {builder}");
            return this;
        }

        public TableBuilder AddDictionary<T>(Dictionary<string, T> dictionary) {
            ArgumentNullException.ThrowIfNull(dictionary);

            foreach (var (key, value) in dictionary) {
                switch (value) {
                    case string strValue:
                        AddKeyValue(key, strValue);
                        break;
                    case int intValue:
                        AddKeyValue(key, intValue);
                        break;
                    case bool boolValue:
                        AddKeyValue(key, boolValue);
                        break;
                    case double doubleValue:
                        AddKeyValue(key, doubleValue);
                        break;
                    default:
                        AddKeyValue(key, value?.ToString() ?? "nil");
                        break;
                }
            }
            return this;
        }

        public TableBuilder AddDictionary<T>(Dictionary<int, T> dictionary) {
            ArgumentNullException.ThrowIfNull(dictionary);

            foreach (var (key, value) in dictionary) {
                switch (value) {
                    case string strValue:
                        AddKeyValue(key, strValue);
                        break;
                    case int intValue:
                        AddKeyValue(key, intValue);
                        break;
                    case bool boolValue:
                        AddKeyValue(key, boolValue);
                        break;
                    case double doubleValue:
                        AddKeyValue(key, doubleValue);
                        break;
                    default:
                        AddKeyValue(key, value?.ToString() ?? "nil");
                        break;
                }
            }
            return this;
        }

        public TableBuilder AddArray<T>(IEnumerable<T> array) {
            ArgumentNullException.ThrowIfNull(array);

            foreach (var value in array) {
                switch (value) {
                    case string strValue:
                        AddValue(strValue);
                        break;
                    case int intValue:
                        AddValue(intValue);
                        break;
                    case bool boolValue:
                        AddValue(boolValue);
                        break;
                    case double doubleValue:
                        AddValue(doubleValue);
                        break;
                    default:
                        AddValue(value?.ToString() ?? "nil");
                        break;
                }
            }
            return this;
        }

        public override string ToString() {
            if (_elements.Count == 0)
                return "{}";

            StringBuilder sb = new("{"+Environment.NewLine);
            for (int i = 0; i < _elements.Count; i++) {
                sb.Append(_elements[i]);
                if (i < _elements.Count - 1) {
                    sb.Append("," + Environment.NewLine);
                }
            }
            sb.Append(Environment.NewLine);
            sb.Append(ParentIndent);
            sb.Append('}');
            return sb.ToString();

        }

    }

}
