using System;
using System.IO;
using System.Diagnostics;
using System.Text;

namespace BattlegroundsApp.Utilities;

/// <summary>
/// Class for logging information to <see cref="OUT_PATH"/>. Inherits from <see cref="TraceListener"/> (So use <see cref="Trace.WriteLine(object?)"/> or equivalent methods 
/// to write logging messages).
/// </summary>
public sealed class Logger : TraceListener {

    public const string OUT_PATH = "coh2-bg.log";

    private readonly StreamWriter? m_writer;

    /// <summary>
    /// Get if logger instance has an open log file
    /// </summary>
    public bool HasFile => this.m_writer is not null;

    private static FileStream? Open() {
        try {
            return File.Open(OUT_PATH, FileMode.Append, FileAccess.Write, FileShare.Read);
        } catch (IOException) {
            return null;
        }
    }

    /// <summary>
    /// Initialsie a new <see cref="Logger"/> instance on <see cref="OUT_PATH"/>.
    /// </summary>
    public Logger() {
        FileStream? file = Open();
        if (file is not null) {
            if (file.Length > 768) { // Delete if file exceeds 768kb
                file.Close();
                File.Delete(OUT_PATH);
                file = Open();
                if (file is null) {
                    return;
                }
            }
            this.m_writer = new StreamWriter(file, Encoding.Unicode) {
                AutoFlush = true,
            };
            this.m_writer.WriteLine($"{Environment.NewLine}\tStarting new log on {DateTime.Now.ToLongDateString()} @ {DateTime.Now.ToLongTimeString()}{Environment.NewLine}");
            Trace.Listeners.Add(this);
        }
    }

    public override void Write(string? message) {
        try {
            this.m_writer?.Write(message);
        } catch (ObjectDisposedException) { }
    }

    public override void WriteLine(string? message) => this.Write($"[{DateTime.Now.ToLongTimeString()}] {message}{Environment.NewLine}");

    /// <summary>
    /// Saves and closes the log file with a specified exit code (in hex format)
    /// </summary>
    /// <param name="code">The exit code.</param>
    public void SaveAndClose(int code) {
        if (this.m_writer is not null) {
            this.m_writer.WriteLine($"{Environment.NewLine}\tClosing Application with error code 0x{code:X8} @ {DateTime.Now.ToLongTimeString()}{Environment.NewLine}");
            this.m_writer.WriteLine($"\t---------------------------------------------------------");
            this.m_writer.Flush();
            this.m_writer.Close();
        }
    }

}
