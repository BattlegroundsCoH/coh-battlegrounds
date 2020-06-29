using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Battlegrounds.Online {
    
    /// <summary>
    /// Utility for downloading content from a URL address.
    /// </summary>
    public static class SourceDownloader {
    
        /// <summary>
        /// Download the source code (the raw string content) found at the url path.
        /// </summary>
        /// <param name="urlpath">The full URL of the source code to download.</param>
        /// <returns>The string content found at the  URL or the empty string if the download failed.</returns>
        public static string DownloadSourceCode(string urlpath) {

            try {
                using (var client = new WebClient()) {
                    return client.DownloadString(urlpath);
                }
            } catch {
                return string.Empty;
            }

        }

        /// <summary>
        /// Download the file in binary format found at the url path.
        /// </summary>
        /// <param name="urlpath">The full URL of the source file to download.</param>
        /// <param name="encodeAsUTF8">Encode the binary content as UTF-8</param>
        /// <returns>A byte array containing the found content or a byte array of length 0 if no data was downloaded.</returns>
        public static byte[] DownloadSourceFile(string urlpath, bool encodeAsUTF8 = true) {

            try {
                using (var client = new WebClient()) {
                    if (encodeAsUTF8) {
                        return Encoding.UTF8.GetBytes(client.DownloadString(urlpath));
                    } else {
                        return client.DownloadData(urlpath);
                    }
                }
            } catch {
                return new byte[0];
            }


        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="urlpath"></param>
        /// <param name="filepath"></param>
        /// <returns></returns>
        public static bool DownloadToFile(string urlpath, string filepath) {
            try {
                using (var client = new WebClient()) {
                    client.DownloadFile(urlpath, filepath);
                }
            } catch {
                return false;
            }
            return true;
        }

    }

}
