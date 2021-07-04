using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Battlegrounds.Functional {
    
    public static class FunctionalPattern {

        public static T Then<T>(this T e, Func<T, T> function)
            => function(e);

    }

}
