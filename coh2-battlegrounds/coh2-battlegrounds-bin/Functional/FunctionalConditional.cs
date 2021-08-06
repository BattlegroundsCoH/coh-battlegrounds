using System;

namespace Battlegrounds.Functional {

    /// <summary>
    /// Functional-styled implmentation of a conditional statement.
    /// </summary>
    public static class FunctionalConditional {

        /// <summary>
        /// Functional helper object that handles a "var v = if x then y otherwise default" case.
        /// </summary>
        /// <typeparam name="U"></typeparam>
        /// <typeparam name="V"></typeparam>
        public class Otherwise<U, V> {

            private V __subj;
            private U __exist;
            private readonly bool _hasValue;

            /// <summary>
            /// Initialize a new <see cref="Otherwise{U, V}"/> class without a value.
            /// </summary>
            /// <param name="subject">The subject used to create this case.</param>
            public Otherwise(V subject) {
                this.__subj = subject;
                this._hasValue = false;
            }

            /// <summary>
            /// Initialize a new <see cref="Otherwise{U, V}"/> class with some value and a flag determining if previous condition was <see langword="true"/>.
            /// </summary>
            /// <param name="hasVal">Should previous case be considered true?</param>
            /// <param name="subj">The subject used to create this case.</param>
            /// <param name="v">The existing value of the case.</param>
            public Otherwise(bool hasVal, V subj, U v) {
                this._hasValue = hasVal;
                this.__subj = subj;
                this.__exist = v;
            }

            // TODO: Add a "ThenIf" that can do additional stuff given som condition)

            /// <summary>
            /// Default to some case defined by <paramref name="def"/>.
            /// </summary>
            /// <param name="def">The function to invoke if case should be defaulted.</param>
            /// <returns>
            /// Will return <see langword="this"/> if value is present in case. 
            /// Otherwise a new <see cref="Otherwise{U, V}"/> object with value defined as return value of <paramref name="def"/>
            /// </returns>
            public Otherwise<U, V> OrDefaultTo(Func<U> def) => _hasValue ? this : new Otherwise<U, V>(true, this.__subj, def());

            public static implicit operator U(Otherwise<U, V> otherwise) => otherwise.__exist;

        }

        /// <summary>
        /// Functional helper class telling whether som initial condition is <see langword="true"/> for a subject of type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public class IsTrue<T> {

            private T __subj;
            private readonly bool _yes; // non-mutable and based on initial condition value

            /// <summary>
            /// Create a new instance of the <see cref="IsTrue{T}"/> helper class.
            /// </summary>
            /// <param name="y">The given initial (base) condition.</param>
            /// <param name="subj">The subject element of the condition.</param>
            public IsTrue(bool y, T subj) {
                this.__subj = subj;
                this._yes = y;
            }

            /// <summary>
            /// Invoke an instance of <see cref="Action{T}"/> taking the subject as argument if the base condition is <see langword="true"/>.
            /// </summary>
            /// <param name="then">The action object to invoke.</param>
            /// <returns>The calling <see cref="IsTrue{T}"/> instance.</returns>
            public IsTrue<T> Then(Action<T> then) {
                if (_yes) {
                    then.Invoke(this.__subj);
                }
                return this;
            }

            /// <summary>
            /// Invoke an instance of <see cref="Func{T, TResult}"/> if the base condition is <see langword="true"/>.
            /// </summary>
            /// <param name="then">The function object to invoke.</param>
            /// <returns>The result of the <paramref name="then"/> parameter if base condition is <see langword="true"/>. Otherwise the subject of the calling instance is returned.</returns>
            public T Then(Func<T, T> then) {
                if (_yes) {
                    return then.Invoke(this.__subj);
                } else {
                    return this.__subj;
                }
            }

            /// <summary>
            /// If true then do some function and create a <see cref="Otherwise{U, V}"/> case object.
            /// </summary>
            /// <typeparam name="U">The new value type to work with.</typeparam>
            /// <param name="func">Function to invoke if base condition is <see langword="true"/>.</param>
            /// <returns>A new <see cref="Otherwise{U, V}"/> instance with subject and, if base condition is <see langword="true"/>, the result of <paramref name="func"/>.</returns>
            public Otherwise<U, T> ThenDo<U>(Func<T, U> func) {
                if (_yes)
                    return new Otherwise<U, T>(true, this.__subj, func(this.__subj));
                else
                    return new Otherwise<U, T>(this.__subj);
            }

            /// <summary>
            /// Invoke an instance of <see cref="Action{T}"/> if the base condition is <see langword="true"/>.
            /// </summary>
            /// <param name="then">The action object to invoke.</param>
            /// <returns>The calling <see cref="IsTrue{T}"/> instance.</returns>
            public IsTrue<T> Then(Action then) {
                if (_yes) {
                    then.Invoke();
                }
                return this;
            }

            /// <summary>
            /// Invoke the <see cref="Predicate{T}"/> function if the base condition is <see langword="false"/>.
            /// </summary>
            /// <param name="condition">The <see cref="Predicate{T}"/> condition to check.</param>
            /// <returns>
            /// If base condition is <see langword="true"/>, the calling instance is returned. 
            /// Otherwise a new <see cref="IsTrue{T}"/> instance is returned based of the <see cref="Predicate{T}"/> result.
            /// </returns>
            public IsTrue<T> ElseIf(Predicate<T> condition) {
                if (!_yes) {
                    return this.__subj.IfTrue(condition);
                } else {
                    return this;
                }
            }

            /// <summary>
            /// Invoke the <see cref="Predicate{T}"/> function on the <paramref name="newSubject"/> parameter if the base condition is <see langword="false"/>.
            /// </summary>
            /// <param name="newSubject">The new subject of the whole <see cref="IsTrue{T}"/> chain.</param>
            /// <param name="condition">The <see cref="Predicate{T}"/> condition to check.</param>
            /// <returns>
            /// If base condition is <see langword="true"/> the calling instance is returned. 
            /// Otherwise a new <see cref="IsTrue{T}"/> instance is returned based of the <see cref="Predicate{T}"/> result with <paramref name="newSubject"/> as subject.
            /// </returns>
            public IsTrue<T> ElseIf(T newSubject, Predicate<T> condition) {
                if (!_yes && condition(newSubject)) {
                    return new IsTrue<T>(true, newSubject);
                } else {
                    return this;
                }
            }

            /// <summary>
            /// Invoke a <see cref="Action{T}"/> instance, taking the subject as argument, if the base condition is <see langword="false"/>.
            /// </summary>
            /// <param name="then">The <see cref="Action{T}"/> to invoke.</param>
            public void Else(Action<T> then) {
                if (!_yes) {
                    then.Invoke(this.__subj);
                }
            }

            /// <summary>
            /// Invoke a <see cref="Action{T}"/> instance, if the base condition is <see langword="false"/>.
            /// </summary>
            /// <param name="then">The <see cref="Action{T}"/> to invoke.</param>
            public void Else(Action then) {
                if (!_yes) {
                    then.Invoke();
                }
            }

            /// <summary>
            /// Invoke a <see cref="Func{T, TResult}"/> instance, if the base condition is <see langword="false"/>.
            /// </summary>
            /// <param name="then">The <see cref="Func{T, TResult}"/> to invoke.</param>
            /// <returns>The result of the <paramref name="then"/> parameter if base condition is <see langword="false"/>. Otherwise the subject of the calling instance is returned.</returns>
            public T Else(Func<T, T> then) {
                if (!_yes) {
                    return then.Invoke(this.__subj);
                } else {
                    return this.__subj;
                }
            }

            /// <summary>
            /// Check if some other condition (<paramref name="then"/>) is true and return the given subject (<paramref name="other"/>). Otherwise return the subject of the initial instance.
            /// </summary>
            /// <param name="other">The new subject to return if <paramref name="then"/> is <see langword="true"/> and base condition is <see langword="false"/>.</param>
            /// <param name="then">The <see cref="Predicate{T}"/> to check.</param>
            /// <returns>
            /// The <paramref name="other"/> parameter if base condition is <see langword="false"/> and the <paramref name="then"/> predicate is <see langword="true"/>. 
            /// Otherwise the subject of the calling instance is returned.
            /// </returns>
            public T Else(T other, Predicate<T> then) {
                if (!_yes && then(other)) {
                    return other;
                } else {
                    return this.__subj;
                }
            }

            /// <summary>
            /// Negate the base condition of the instance.
            /// </summary>
            /// <returns>A negated instance of the <see cref="IsTrue{T}"/> instance.</returns>
            public IsTrue<T> Negate() => new IsTrue<T>(!this._yes, this.__subj);

            /// <summary>
            /// Retrieve the underlying base condition value.
            /// </summary>
            /// <returns>The base condition of the <see cref="IsTrue{T}"/> instance.</returns>
            public bool ToBool() => this._yes;

            /// <summary>
            /// Retrieve the subject associated with the <see cref="IsTrue{T}"/> instance.
            /// </summary>
            /// <returns>The subject of the <see cref="IsTrue{T}"/> instance.</returns>
            public T ToSubject() => this.__subj;

            /// <summary>
            /// Negate the base condition value of the given <see cref="IsTrue{T}"/> instance.
            /// </summary>
            /// <param name="isTrue">The <see cref="IsTrue{T}"/> instance to negate base condition value of.</param>
            /// <returns>The instance of the negated <see cref="IsTrue{T}"/>.</returns>
            public static IsTrue<T> operator !(IsTrue<T> isTrue) => isTrue.Negate();

            public static explicit operator bool(IsTrue<T> isTrue) => isTrue._yes;

            public static explicit operator T(IsTrue<T> isTrue) => isTrue.__subj; // This will conflict if T is bool

        }

        /// <summary>
        /// Check if a given condition on <typeparamref name="T"/> is <see langword="true"/>.
        /// </summary>
        /// <typeparam name="T">The subject of the resulting <see cref="IsTrue{T}"/> instance.</typeparam>
        /// <param name="o">The given object that is the subject of the initial condition.</param>
        /// <param name="condition">The <see cref="Predicate{T}"/> to match.</param>
        /// <returns>A new <see cref="IsTrue{T}"/> instance with base condition set as result of the <see cref="Predicate{T}"/>.</returns>
        public static IsTrue<T> IfTrue<T>(this T o, Predicate<T> condition) => new IsTrue<T>(condition(o), o);

        /// <summary>
        /// Create a <see cref="IsTrue{T}"/> instance based on the <see cref="bool"/> value as initial condition.
        /// </summary>
        /// <param name="b">The initial value.</param>
        /// <returns>A new <see cref="IsTrue{T}"/> instance based on <see cref="bool"/> value.</returns>
        public static IsTrue<bool> IfTrue(this bool b) => new IsTrue<bool>(b, b);

        /// <summary>
        /// Invoke an action on a <see cref="bool"/> value, given it's <see langword="true"/>.
        /// </summary>
        /// <param name="b">The <see cref="bool"/> value.</param>
        /// <param name="act"><see cref="Action"/> to invoke, if <see cref="bool"/> value is <see langword="true"/>.</param>
        /// <returns>A new <see cref="IsTrue{T}"/> instance based on <see cref="bool"/> value.</returns>
        public static IsTrue<bool> Then(this bool b, Action act) => new IsTrue<bool>(b, b).Then(act);

        /// <summary>
        /// Invoke a <see cref="Func{TResult}"/> if initial boolean value is <see langword="true"/>.
        /// </summary>
        /// <typeparam name="T">The type of which the subject will be.</typeparam>
        /// <param name="b">The initial <see cref="bool"/> value.</param>
        /// <param name="act">The <see cref="Func{TResult}"/> (action) to invoke if the initial value is <see langword="true"/>.</param>
        /// <returns>A new <see cref="IsTrue{T}"/> instance with base condition from initial <see cref="bool"/> value and result of action (or default if initial condition is <see langword="false"/>).</returns>
        public static IsTrue<T> Then<T>(this bool b, Func<T> act) => new IsTrue<T>(b, b ? act() : default);

        /// <summary>
        /// Check if a given condition on <typeparamref name="T"/> is <see langword="false"/>.
        /// </summary>
        /// <typeparam name="T">The subject of the resulting <see cref="IsTrue{T}"/> instance.</typeparam>
        /// <param name="o">The given object that is the subject of the initial condition.</param>
        /// <param name="condition">The <see cref="Predicate{T}"/> to match.</param>
        /// <returns>A new <see cref="IsTrue{T}"/> instance with base condition set as result of the <see cref="Predicate{T}"/>.</returns>
        public static IsTrue<T> IfFalse<T>(this T o, Predicate<T> condition) => new IsTrue<T>(!condition(o), o);

        /// <summary>
        /// Create a <see cref="IsTrue{T}"/> instance based on the <see cref="bool"/> value as initial condition.
        /// </summary>
        /// <param name="b">The initial value.</param>
        /// <returns>A new <see cref="IsTrue{T}"/> instance based on <see cref="bool"/> value.</returns>
        public static IsTrue<bool> IfFalse(this bool b) => new IsTrue<bool>(!b, !b);

    }

}
