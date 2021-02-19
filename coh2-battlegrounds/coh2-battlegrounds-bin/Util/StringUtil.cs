using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Battlegrounds.Util {
    
    /// <summary>
    /// 
    /// </summary>
    public static class StringUtil {

        /// <summary>
        /// 
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static byte[] Encode(this string s) => s.Encode(Encoding.ASCII);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="s"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public static byte[] Encode(this string s, Encoding encoding) => encoding.GetBytes(s);
    
    }

}
