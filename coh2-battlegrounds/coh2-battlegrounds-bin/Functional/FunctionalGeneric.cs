using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Battlegrounds.Functional {
    
    /// <summary>
    /// 
    /// </summary>
    public static class FunctionalGeneric {

        public static T Random<T>(this IEnumerable<T> enumerable)
            => enumerable.Random(new Random());

        public static T Random<T>(this IEnumerable<T> enumerable, Random random) {
            int i = (enumerable.Count() > 0) ? random.Next(0, enumerable.Count()) : - 1;
            return (i >= 0) ? enumerable.ElementAt(i) : default;
        }

    }

}
