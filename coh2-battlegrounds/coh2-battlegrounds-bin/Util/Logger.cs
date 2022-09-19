using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace Battlegrounds.Util;

/// <summary>
/// Class for logging information to <see cref="OUT_PATH"/>. Inherits from <see cref="TraceListener"/> (So use <see cref="Trace.WriteLine(object?)"/> or equivalent methods 
/// to write logging messages).
/// </summary>
public sealed class Logger : TraceListener {

    private static readonly string OUT_PATH = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Battlegrounds-CoH2\\coh2-bg.log");

    private readonly StreamWriter? m_writer;
    private readonly DateTime m_startupTime;

    /// <summary>
    /// Get if logger instance has an open log file
    /// </summary>
    public bool HasFile => this.m_writer is not null;

    /// <summary>
    /// Initialsie a new <see cref="Logger"/> instance on <see cref="OUT_PATH"/>.
    /// </summary>
    public Logger() {
        
        // Grab startup time
        this.m_startupTime = DateTime.Now;

        // Open file
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
            this.StoreLogFile();
        }
    }

    private void StoreLogFile() {

        // Ensure file exists
        if (File.Exists(OUT_PATH)) {

            // Create storage paths
            var d = $"{this.m_startupTime.Year}_{this.m_startupTime.Month}_{this.m_startupTime.Day}_{this.m_startupTime.Hour}_{this.m_startupTime.Minute}_{this.m_startupTime.Second}";
            var store_path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Battlegrounds-CoH2\\logs");
            var store_name = Path.Combine(store_path, $"coh2-bg_{d}.log");
            
            // Make sure container folder exists
            if (!Directory.Exists(store_path)) {
                Directory.CreateDirectory(store_path);
            }
            
            // Copy the file
            File.Copy(OUT_PATH, store_name);

        }

    }

    private static FileStream? Open() {
        try {
            return File.Open(OUT_PATH, FileMode.Create, FileAccess.Write, FileShare.Read);
        } catch (IOException ioex) {
            Trace.WriteLine(ioex, nameof(Logger)); // <-- will only be visible in VS
            return null;
        }
    }

}
