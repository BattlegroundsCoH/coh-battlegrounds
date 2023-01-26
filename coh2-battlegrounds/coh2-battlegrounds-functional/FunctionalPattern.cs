using System;

namespace Battlegrounds.Functional;

public static class FunctionalPattern {

    public static T Then<T>(this T e, Func<T, T> function)
        => function(e);

    public static T And<T>(this T t, Action a) {
        a();
        return t;
    }

}
