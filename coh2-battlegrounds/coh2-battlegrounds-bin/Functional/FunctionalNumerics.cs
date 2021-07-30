using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Battlegrounds.Functional {

    public static class FunctionalNumerics {

        public static bool InRange(this int n, int min, int max) => min <= n && n <= max;

        public static bool InRange(this uint n, uint min, uint max) => min <= n && n <= max;

        public static void Do(this int n, Action<int> action) {
            for (int i = 0; i < n; i++) {
                action(n);
            }
        }

    }

}
