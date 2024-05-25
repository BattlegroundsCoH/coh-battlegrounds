using System.Globalization;
using System.Text;

namespace Battlegrounds.Core.Games.Scripts;

public sealed class ScarScriptWriter(Stream stream) : IDisposable {

    private readonly StreamWriter _writer = new StreamWriter(stream, encoding: Encoding.UTF8, leaveOpen: true);

    private readonly Stack<bool> _commaStack = [];

    private bool _disposed = false;
    private int indent = 0;

    public void Comment(string comment) {
        string[] lines = comment.Split(Environment.NewLine);
        foreach (string line in lines) {
            _writer.Write("-- ");
            _writer.WriteLine(line);
        }
    }

    private void WriteIndent() {
        _writer.Write(new string('\t', indent));
    }

    public ScarScriptWriter EmptyTable(string name = "") {
        WriteIndent();
        if (!string.IsNullOrEmpty(name)) {
            _writer.Write(name);
            _writer.Write(" = ");
        }
        _writer.Write("{}");
        if (indent == 0) {
            _writer.WriteLine();
        } else {
            WriteTrailingComma();
        }
        return this;
    }

    public ScarScriptWriter BeginTable(string name = "") { 
        WriteIndent();
        if (!string.IsNullOrEmpty(name)) {
            _writer.Write(name);
            _writer.Write(" = ");
        }
        _writer.WriteLine('{');
        _commaStack.Push(true);
        indent++;
        return this; 
    }

    public ScarScriptWriter EndTable() {
        indent--;
        WriteIndent();
        _commaStack.Pop();
        if (_commaStack.Count > 0) {
            _writer.WriteLine("},");
        } else {
            _writer.WriteLine('}');
        }
        return this;
    }

    public ScarScriptWriter WriteCollection<T>(string name, IEnumerable<T> collection, Action<T> handler) {
        BeginTable(name);
        foreach (T item in collection) {
            BeginTable();
            handler(item);
            EndTable();
        }
        EndTable();
        return this;
    }

    public ScarScriptWriter AssignTo(string name) {
        WriteIndent();
        _writer.Write(name);
        _writer.Write(" = ");
        return this;
    }

    public ScarScriptWriter Field(string name) {
        _writer.Write(name);
        _writer.Write(" = ");
        return this;
    }

    public ScarScriptWriter String(string value) {
        _writer.Write('"');
        _writer.Write(value);
        _writer.Write('"');
        WriteTrailingComma();
        return this;
    }

    public ScarScriptWriter Integer(int value) {
        _writer.Write(value);
        WriteTrailingComma();
        return this;
    }

    public ScarScriptWriter Number(float value) {
        _writer.Write(value.ToString(CultureInfo.InvariantCulture));
        WriteTrailingComma();
        return this;
    }

    public ScarScriptWriter Serialize<T>(T item, Action<ScarScriptWriter, T> action) {
        action(this, item);
        return this;
    }

    public void NewLine() {
        _writer.WriteLine();
    }

    private void WriteTrailingComma() {
        if (_commaStack.Count > 0 && _commaStack.Peek() == true) {
            _writer.Write(", ");
        }
    }

    public void Dispose() {
        if (!_disposed) {
            _disposed = true;
            _writer.Flush();
        }
    }

}
