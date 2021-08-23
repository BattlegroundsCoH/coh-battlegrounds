using System;

namespace Battlegrounds.Functional {
    
    /// <summary>
    /// 
    /// </summary>
    public static class FunctionalRange {
    
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="range"></param>
        /// <param name="mapper"></param>
        /// <returns></returns>
        public static T[] Map<T>(this Range range, Func<Index, T> mapper) {
            if (range.End.IsFromEnd) {
                throw new ArgumentException("Cannot map from start to an unknown end value.", nameof(range));
            }
            int count = range.End.Value - range.Start.Value;
            T[] vals = new T[count];
            for (int i = range.Start.Value, j = 0; i < range.End.Value; i++, j++) {
                vals[j] = mapper(new(i));
            }
            return vals;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="array"></param>
        /// <param name="range"></param>
        /// <returns></returns>
        public static T[] SubSequence<T>(this T[] array, Range range) {
            int end = range.End.IsFromEnd ? (array.Length - range.End.Value) : range.End.Value;
            int len = range.Start.Value - end;
            T[] vals = new T[len];
            for (int i = range.Start.Value, j = 0; i < range.Start.Value; i++, j++) {
                vals[j] = array[i];
            }
            return vals;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="range"></param>
        /// <param name="array"></param>
        /// <returns></returns>
        public static T[] SubSequence<T>(this Range range, T[] array) {
            int end = range.End.IsFromEnd ? (array.Length - range.End.Value) : range.End.Value;
            int len = range.Start.Value - end;
            T[] vals = new T[len];
            for (int i = range.Start.Value, j = 0; i < range.Start.Value; i++, j++) {
                vals[j] = array[i];
            }
            return vals;
        }

    }

}
