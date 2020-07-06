using System;
using System.Collections.Generic;
using System.Text;

namespace Battlegrounds.Functional {
    
    /// <summary>
    /// 
    /// </summary>
    public static class FunctionalArray {
    
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="array"></param>
        /// <param name="func"></param>
        /// <returns></returns>
        public static T[] ForEach<T>(this T[] array, Func<T, T> func) {
            T[] t = new T[array.Length];
            for (int i = 0; i < array.Length; i++) {
                t[i] = func(array[i]);
            }
            return t;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="array"></param>
        /// <param name="act"></param>
        /// <returns></returns>
        public static T[] ForEach<T>(this T[]array, Action<T> act) {
            for (int i = 0; i < array.Length; i++) {
                act(array[i]);
            }
            return array;
        }

    }

}
