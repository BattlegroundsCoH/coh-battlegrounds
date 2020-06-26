using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Battlegrounds.Online {
    
    /// <summary>
    /// 
    /// </summary>
    public static class SourceDownloader {
    
        /// <summary>
        /// 
        /// </summary>
        /// <param name="urlpath"></param>
        /// <returns></returns>
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
        /// 
        /// </summary>
        /// <param name="urlpath"></param>
        /// <returns></returns>
        public static byte[] DownloadSourceFile(string urlpath) {

            try {
                using (var client = new WebClient()) {
                    return Encoding.UTF8.GetBytes(client.DownloadString(urlpath));
                }
            } catch {
                return new byte[0];
            }


        }

    }

}
