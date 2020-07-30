using System;

namespace Battlegrounds.Functional {
    
    /// <summary>
    /// 
    /// </summary>
    public static class FunctionalConditional {
    
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public class IsTrue<T> {
            private T __subj;
            private readonly bool _yes; // non-mutable and based on initial condition value
            public IsTrue(bool y, T subj) {
                this.__subj = subj;
                this._yes = y;
            }
            /// <summary>
            /// 
            /// </summary>
            /// <param name="then"></param>
            /// <returns></returns>
            public IsTrue<T> Then(Action<T> then) {
                if (_yes) {
                    then.Invoke(this.__subj);
                }
                return this;
            }
            
            /// <summary>
            /// 
            /// </summary>
            /// <param name="condition"></param>
            /// <returns></returns>
            public IsTrue<T> ElseIf(Predicate<T> condition) {
                if (!_yes) {
                    return this.__subj.IfTrue(condition);
                } else {
                    return new IsTrue<T>(false, this.__subj);
                }
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="then"></param>
            public void Else(Action<T> then) {
                if (!_yes) {
                    then.Invoke(this.__subj);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="o"></param>
        /// <param name="condition"></param>
        /// <returns></returns>
        public static IsTrue<T> IfTrue<T>(this T o, Predicate<T> condition) {
            if (condition(o)) {
                return new IsTrue<T>(true, o);
            } else {
                return new IsTrue<T>(false, o);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="o"></param>
        /// <param name="condition"></param>
        /// <returns></returns>
        public static IsTrue<T> IfFalse<T>(this T o, Predicate<T> condition) {
            if (condition(o)) {
                return new IsTrue<T>(false, o);
            } else {
                return new IsTrue<T>(true, o);
            }
        }

    }

}
