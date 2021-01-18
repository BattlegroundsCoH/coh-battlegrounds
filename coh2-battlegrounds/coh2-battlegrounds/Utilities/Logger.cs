using System;
using System.IO;
using System.Diagnostics;
using System.Text;

namespace BattlegroundsApp.Utilities {

    /// <summary>
    /// Class for logging information to <see cref="OUT_PATH"/>. Inherits from <see cref="TraceListener"/> (So use <see cref="Trace.WriteLine(object?)"/> or equivalent methods 
    /// to write logging messages).
    /// </summary>
    public sealed class Logger : TraceListener {

        public const string OUT_PATH = "coh2-bg.log";

        private StreamWriter m_writer;

        private static FileStream Open() => File.Open(OUT_PATH, FileMode.Append, FileAccess.Write, FileShare.Read);

        public Logger() {
            FileStream file = Open();
            if (file.Length > 1024 * 128) { // Delete if file exceeds 128 MB
                file.Close();
                File.Delete(OUT_PATH);
                file = Open();
            }
            this.m_writer = new StreamWriter(file, Encoding.Unicode) {
                AutoFlush = true,
            };
            this.m_writer.WriteLine($"{Environment.NewLine}\tStarting new log on {DateTime.Now.ToLongDateString()} @ {DateTime.Now.ToLongTimeString()}{Environment.NewLine}");
            Trace.Listeners.Add(this);
        }

        public override void Write(string message) => this.m_writer.Write(message);

        public override void WriteLine(string message) => this.Write($"[{DateTime.Now.ToLongTimeString()}] {message}{Environment.NewLine}");

        public void SaveAndClose(int code) {
            this.m_writer.WriteLine($"{Environment.NewLine}\tClosing Application with error code 0x{code:X8} @ {DateTime.Now.ToLongTimeString()}{Environment.NewLine}");
            this.m_writer.WriteLine($"\t---------------------------------------------------------");
            this.m_writer.Flush();
            this.m_writer.Close();
        }

    }

}
