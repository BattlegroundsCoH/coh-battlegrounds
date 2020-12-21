using System;

namespace Battlegrounds.Functional {
    
    /// <summary>
    /// Functional-styled implmentation of a conditional statement.
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

            public T Then(Func<T, T> then) {
                if (_yes) {
                    return then.Invoke(this.__subj);
                } else {
                    return this.__subj;
                }
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="then"></param>
            /// <returns></returns>
            public IsTrue<T> Then(Action then) {
                if (_yes) {
                    then.Invoke();
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

            /// <summary>
            /// 
            /// </summary>
            /// <param name="then"></param>
            public void Else(Action then) {
                if (!_yes) {
                    then.Invoke();
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
        /// <param name="b"></param>
        /// <returns></returns>
        public static IsTrue<bool> IfTrue(this bool b) => new IsTrue<bool>(b, b);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="b"></param>
        /// <param name="act"></param>
        /// <returns></returns>
        public static IsTrue<bool> Then(this bool b, Action act) => new IsTrue<bool>(b, b).Then(act);

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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="b"></param>
        /// <returns></returns>
        public static IsTrue<bool> IfFalse(this bool b) => new IsTrue<bool>(!b, !b);

    }

}
