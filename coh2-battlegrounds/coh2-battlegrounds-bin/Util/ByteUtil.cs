using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace coh2_battlegrounds_bin.Util {
    
    /// <summary>
    /// 
    /// </summary>
    public static class ByteUtil {
    
        /// <summary>
        /// Check if a byte array matches the contents of the string.
        /// </summary>
        /// <param name="barray">Byte array to check for matching content</param>
        /// <param name="content">The string content to compare against</param>
        /// <returns>True if the contents of the byte array matches the content of the string.</returns>
        public static bool Match(byte[] barray, string content) {
            if (barray.Length == content.Length) {
                for (int i = 0; i < barray.Length; i++) {
                    if ((char)barray[i] != content[i]) {
                        return false;
                    }
                }
                return true;
            } else {
                return false;
            }
        }

    }

}
