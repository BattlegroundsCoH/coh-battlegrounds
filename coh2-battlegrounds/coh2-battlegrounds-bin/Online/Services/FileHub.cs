using System;
using System.IO;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Diagnostics;

namespace Battlegrounds.Online.Services {
    
    /// <summary>
    /// Static helper class for downloading and uploading files to the file-server.
    /// </summary>
    public static class FileHub {
    
        /// <summary>
        /// Establish a <see cref="TcpClient"/> connection to the file-server.
        /// </summary>
        /// <returns>A connected <see cref="TcpClient"/> or null if not found.</returns>
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
        /// Download a file from the server.
        /// </summary>
        /// <param name="localapth">The local path that the downloaded file contents will be written to.</param>
        /// <param name="downloadname">The name of the file to download from the server.</param>
        /// <param name="lobbyID">The lobby ID to use when identifying the lobby to download data from.</param>
        /// <returns>If download was ssuccesful, true is returned; otherwise false.</returns>
        public static bool DownloadFile(string localapth, string downloadname, string lobbyID) {

            // Establish connection
            if (Connect() is TcpClient client) {

                try {

                    // Open streams
                    StreamReader sr = new StreamReader(client.GetStream());
                    StreamWriter sw = new StreamWriter(client.GetStream());

                    // Send download request
                    sw.WriteLine($"DOWNLOAD {lobbyID} {downloadname}");
                    sw.Flush();

                    // Read the response
                    string response = sr.ReadLine();

                    // If a response can be parsed into an int (the size of the file to download)
                    if (int.TryParse(response, out int length)) {

                        // Growable buffer for the bytes
                        List<byte> bytes = new List<byte>();

                        // While we've not finished downloading
                        while (bytes.Count < length) {

                            // Allocate buffer and the buffer
                            byte[] buffer = new byte[client.ReceiveBufferSize];
                            int read = sr.BaseStream.Read(buffer, 0, client.ReceiveBufferSize);

                            // Add the downloaded bytes to the growable buffer
                            bytes.AddRange(buffer[0..^(client.ReceiveBufferSize - read)]);
                            
                            // Log how much was downloaded
                            Trace.WriteLine($"Received {read} bytes ({bytes.Count}/{length})", "Online-Service");

                        }

                        // If the file already exists, we delete it (Just to be safe)
                        if (File.Exists(localapth)) {
                            File.Delete(localapth);
                        }

                        // Write all downloaded binary contents.
                        File.WriteAllBytes(localapth, bytes.ToArray());

                        // Log what, how, and where the file was downloaded.
                        Trace.WriteLine($"Downloaded file \"{downloadname}\" (\"{localapth}\") from {lobbyID} #{bytes.Count}", "Online-Service");

                        // Return true ==> we've downloaded the file
                        return true;

                    } else {

                        // Log that it was not possible to download the file (with a given reason from the server)
                        Trace.WriteLine($"Failed to download file (Message: {response})", "Online-Service");

                    }

                } catch (Exception e) { // semi ignore
                    Trace.WriteLine(e, "Online-Service");
                }

                // Return false (reached this point but no file was downloaded)
                return false;

            } else {

                // Log that we couldn't connect
                Trace.WriteLine("Failed to download file (Unable to establish connection to file server!)", "Online-Service");
                return false;

            }

        }

        /// <summary>
        /// Upload a file to the server.
        /// </summary>
        /// <param name="filepath">The local path of the file to upload.</param>
        /// <param name="uploadname">The name of the file to upload to the server.</param>
        /// <param name="lobbyID">The lobby ID to use when identifying the lobby to upload data to.</param>
        /// <returns>If upload was ssuccesful, true is returned; otherwise false.</returns>
        public static bool UploadFile(string filepath, string uploadname, string lobbyID) {

            // If the file doesn't exit on our end - we naturally can't download it
            if (!File.Exists(filepath)) {
                return false;
            }

            // Read bytes from file
            byte[] file = File.ReadAllBytes(filepath);

            // Upload file using byte-content
            return UploadFile(file, uploadname, lobbyID);

        }

        /// <summary>
        /// Upload byte file content to server.
        /// </summary>
        /// <param name="byteContent">The byte content representing a file to upload.</param>
        /// <param name="uploadname">The name of the file to upload to the server.</param>
        /// <param name="lobbyID">The lobby ID to use when identifying the lobby to upload data to.</param>
        /// <returns>If upload was ssuccesful, <see langword="true"/> is returned; otherwise <see langword="false"/>.</returns>
        public static bool UploadFile(byte[] byteContent, string uploadname, string lobbyID) {

            // Establish connection
            if (Connect() is TcpClient client) {

                try {

                    // Open connection
                    StreamReader sr = new StreamReader(client.GetStream());
                    StreamWriter sw = new StreamWriter(client.GetStream());

                    // Tell the server we wish to upload a file of specified length
                    sw.WriteLine($"UPLOAD {lobbyID} {uploadname} {byteContent.Length}");
                    sw.Flush();

                    // Read response
                    string response = sr.ReadLine();

                    // If response starts with OK
                    if (response.StartsWith("OK")) {

                        // Log we received the OK
                        Trace.WriteLine("Received OK", "Online-Service");

                        // Send the byte data
                        sw.BaseStream.Write(byteContent, 0, byteContent.Length);
                        sw.Flush();

                        // Get a response from the server
                        response = sr.ReadLine();

                        // Check for server response
                        if (response.CompareTo($"OK {byteContent.Length}") == 0) {

                            // Log that we managed to uplaod the file
                            Trace.WriteLine($"Uploaded file \"{uploadname}\" to {lobbyID} ({byteContent.Length} bytes)", "Online-Service");

                            // Return true
                            return true;

                        } else {

                            // Log error
                            Trace.WriteLine($"Failed to upload ({response})", "Online-Service");

                        }

                    } else {

                        // Write the failed response
                        Trace.WriteLine(response, "Online-Service");

                    }

                } catch (Exception e) { // semi ignore
                    Trace.WriteLine(e);
                }

            } else {

                // Log we failed to uploaded because no connection was made
                Trace.WriteLine("Failed to upload file (Unable to establish connection to file server!)", "Online-Service");

            }

            // Return false (reached this line - file was not uploaded)
            return false;

        }

    }

}
