using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Battlegrounds.Util {
    
    public class DistinctList<T> : List<T> {

        public new void Add(T item) {
            if (!this.Contains(item)) {
                base.Add(item);
            }
        }

    }

}
