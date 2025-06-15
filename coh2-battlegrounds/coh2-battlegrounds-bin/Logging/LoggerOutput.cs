using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace Battlegrounds.Logging;

/// <summary>
/// Class for logging information to <see cref="OUT_PATH"/>. Inherits from <see cref="TraceListener"/> (So use <see cref="Trace.WriteLine(object?)"/> or equivalent methods 
/// to write logging messages).
/// </summary>
public sealed class LoggerOutput : TraceListener {

    private static readonly string OUT_PATH = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Battlegrounds-CoH\\coh2-bg.log");

    private readonly StreamWriter? m_writer;
    private readonly DateTime m_startupTime;

    /// <summary>
    /// Get if logger instance has an open log file
    /// </summary>
    public bool HasFile => m_writer is not null;

    /// <summary>
    /// Initialsie a new <see cref="LoggerOutput"/> instance on <see cref="OUT_PATH"/>.
    /// </summary>
    public LoggerOutput() {

        // Grab startup time
        m_startupTime = DateTime.Now;

        // Open file
        FileStream? file = Open();
        if (file is not null) {
            if (file.Length > 768 * 1024) { // Delete if file exceeds 768kb
                file.Close();
                File.Delete(OUT_PATH);
                file = Open();
                if (file is null) {
                    return;
                }
            }
            m_writer = new StreamWriter(file, Encoding.Unicode) {
                AutoFlush = true,
            };
            m_writer.WriteLine($"{Environment.NewLine}\tStarting new log on {DateTime.Now.ToLongDateString()} @ {DateTime.Now.ToLongTimeString()}{Environment.NewLine}");
        }
    }

    /// <inheritdoc/>
    public override void Write(string? message) {
        try {
            m_writer?.Write(message);
        } catch (ObjectDisposedException) { }
    }

    /// <inheritdoc/>
    public override void WriteLine(string? message) => Write($"[{DateTime.Now.ToLongTimeString()}] {message}{Environment.NewLine}");

    /// <summary>
    /// Saves and closes the log file with a specified exit code (in hex format)
    /// </summary>
    /// <param name="code">The exit code.</param>
    public void SaveAndClose(int code) {
        if (m_writer is not null) {
            m_writer.WriteLine($"{Environment.NewLine}\tClosing Application with error code 0x{code:X8} @ {DateTime.Now.ToLongTimeString()}{Environment.NewLine}");
            m_writer.WriteLine($"\t---------------------------------------------------------");
            m_writer.Flush();
            m_writer.Close();
            StoreLogFile();
        }
    }

    private void StoreLogFile() {

        // Ensure file exists
        if (File.Exists(OUT_PATH)) {

            // Create storage paths
            var d = $"{m_startupTime.Year}_{m_startupTime.Month}_{m_startupTime.Day}_{m_startupTime.Hour}_{m_startupTime.Minute}_{m_startupTime.Second}";
            var store_path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Battlegrounds-CoH\\logs");
            var store_name = Path.Combine(store_path, $"coh-bg_{d}.log");

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
            Trace.WriteLine(ioex, nameof(LoggerOutput)); // <-- will only be visible in VS
            return null;
        }
    }

}
