using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace coh2_battlegrounds_bin {
    
    /// <summary>
    /// 
    /// </summary>
    public static class ByteUtil {
    
        /// <summary>
        /// 
        /// </summary>
        /// <param name="barray"></param>
        /// <param name="content"></param>
        /// <returns></returns>
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
