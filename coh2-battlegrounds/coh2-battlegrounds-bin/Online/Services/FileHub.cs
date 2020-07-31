using System;
using System.IO;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Diagnostics;

namespace Battlegrounds.Online.Services {
    
    /// <summary>
    /// 
    /// </summary>
    public static class FileHub {
    
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private static TcpClient Connect() {

            try {

                TcpClient client = new TcpClient();
                client.Connect(AddressBook.GetFileServer());

                if (client.Connected) {
                    return client;
                } else {
                    return null;
                }

            } catch (Exception e) {
                Trace.WriteLine(e);
                return null;
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="localapth"></param>
        /// <param name="downloadname"></param>
        /// <param name="lobbyID"></param>
        /// <returns></returns>
        public static bool DownloadFile(string localapth, string downloadname, string lobbyID) {

            if (Connect() is TcpClient client) {

                try {

                    StreamReader sr = new StreamReader(client.GetStream());
                    StreamWriter sw = new StreamWriter(client.GetStream());

                    sw.WriteLine($"DOWNLOAD {lobbyID} {downloadname}");
                    sw.Flush();

                    string response = sr.ReadLine();
                    if (int.TryParse(response, out int length)) {

                        List<byte> bytes = new List<byte>();

                        while (bytes.Count < length) {
                            byte[] buffer = new byte[client.ReceiveBufferSize];
                            int read = sr.BaseStream.Read(buffer, 0, client.ReceiveBufferSize);
                            bytes.AddRange(buffer[0..^(client.ReceiveBufferSize - read)]);
                            Trace.WriteLine($"Received {read} bytes ({bytes.Count}/{length})");
                        }

                        if (File.Exists(localapth)) {
                            File.Delete(localapth);
                        }

                        File.WriteAllBytes(localapth, bytes.ToArray());

                        Trace.WriteLine($"Downloaded file \"{downloadname}\" (\"{localapth}\") from {lobbyID} #{bytes.Count}", "Online-Service");

                        return true;

                    } else {

                        Trace.WriteLine($"Failed to download file (Message: {response})", "Online-Service");

                    }

                } catch (Exception e) { // semi ignore
                    Trace.WriteLine(e, "Online-Service");
                }

                return false;

            } else {

                Trace.WriteLine("Failed to download file (Unable to establish connection to file server!)", "Online-Service");
                return false;

            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="filepath"></param>
        /// <param name="uploadname"></param>
        /// <param name="lobbyID"></param>
        /// <returns></returns>
        public static bool UploadFile(string filepath, string uploadname, string lobbyID) {

            if (!File.Exists(filepath)) {
                return false;
            }

            if (Connect() is TcpClient client) {

                try {

                    byte[] file = File.ReadAllBytes(filepath);

                    StreamReader sr = new StreamReader(client.GetStream());
                    StreamWriter sw = new StreamWriter(client.GetStream());

                    sw.WriteLine($"UPLOAD {lobbyID} {uploadname} {file.Length}");
                    sw.Flush();

                    string response = sr.ReadLine();

                    if (response.StartsWith("OK")) {

                        Trace.WriteLine($"Received OK ({response})", "Online-Service");

                        sw.BaseStream.Write(file, 0, file.Length);
                        sw.Flush();

                        Trace.WriteLine($"Uploaded file \"{uploadname}\" (\"{filepath}\") to {lobbyID} #{file.Length}", "Online-Service");

                        return true;

                    } else {

                        Trace.WriteLine(response, "Online-Service");

                    }

                } catch (Exception e) { // semi ignore
                    Trace.WriteLine(e);
                }

            } else {

                Trace.WriteLine("Failed to upload file (Unable to establish connection to file server!)", "Online-Service");

            }

            return false;

        }

    }

}
