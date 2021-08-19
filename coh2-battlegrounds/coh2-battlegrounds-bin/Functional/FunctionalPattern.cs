using System;

namespace Battlegrounds.Functional {

    public static class FunctionalPattern {

        public static T Then<T>(this T e, Func<T, T> function)
            => function(e);

        public static V Map<U, V>(this U e, Func<U, V> function)
            => function(e);

    }

}
