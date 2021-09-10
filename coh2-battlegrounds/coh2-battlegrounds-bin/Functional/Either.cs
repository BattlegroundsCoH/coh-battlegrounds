using System;

namespace Battlegrounds.Functional {

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TOptionA"></typeparam>
    /// <typeparam name="TOptionB"></typeparam>
    public struct Either<TOptionA, TOptionB> {

        private readonly TOptionA m_a;
        private readonly TOptionB m_b;

        /// <summary>
        /// 
        /// </summary>
        public bool IsFirst { get; }

        /// <summary>
        /// 
        /// </summary>
        public bool IsSecond { get; }

        /// <summary>
        /// 
        /// </summary>
        public bool Any => this.IsFirst || this.IsSecond;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        public Either(TOptionA a) {

            this.m_a = a;
            this.IsFirst = true;

            this.m_b = default;
            this.IsSecond = false;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="b"></param>
        public Either(TOptionB b) {

            this.m_b = b;
            this.IsSecond = true;

            this.m_a = default;
            this.IsFirst = false;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ifNot"></param>
        /// <returns></returns>
        public TOptionA FirstOption(TOptionA ifNot) => this.IsFirst ? this.m_a : ifNot;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        public bool IfFirstOption(out TOptionA val) {
            if (this.IsFirst) {
                val = this.m_a;
                return true;
            }
            val = default;
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ifNot"></param>
        /// <returns></returns>
        public TOptionB SecondOption(TOptionB ifNot) => this.IsSecond ? this.m_b : ifNot;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        public bool IfSecondOption(out TOptionB val) {
            if (this.IsSecond) {
                val = this.m_b;
                return true;
            }
            val = default;
            return false;
        }

        public static implicit operator TOptionA(Either<TOptionA, TOptionB> either)
            => either.IsFirst ? either.m_a : throw new InvalidOperationException($"Cannot return value of type '{typeof(TOptionA).Name}' when given value is of type '{typeof(TOptionB).Name}'");

        public static implicit operator TOptionB(Either<TOptionA, TOptionB> either)
            => either.IsSecond ? either.m_b : throw new InvalidOperationException($"Cannot return value of type '{typeof(TOptionB).Name}' when given value is of type '{typeof(TOptionA).Name}'");

        public static implicit operator Either<TOptionA, TOptionB>(TOptionA a)
            => new(a);

        public static implicit operator Either<TOptionA, TOptionB>(TOptionB b)
            => new(b);

    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TOptionA"></typeparam>
    /// <typeparam name="TOptionB"></typeparam>
    public struct Either<TOptionA, TOptionB, TOptionC> {

        private readonly TOptionA m_a;
        private readonly TOptionB m_b;
        private readonly TOptionC m_c;

        /// <summary>
        /// 
        /// </summary>
        public bool IsFirst { get; }

        /// <summary>
        /// 
        /// </summary>
        public bool IsSecond { get; }

        /// <summary>
        /// 
        /// </summary>
        public bool IsThird { get; }

        /// <summary>
        /// 
        /// </summary>
        public bool Any => this.IsFirst || this.IsSecond || this.IsThird;

        /// <summary>
        /// 
        /// </summary>
        public Type ValueType => this.IsFirst ? typeof(TOptionA) : (this.IsSecond ? typeof(TOptionB) : typeof(TOptionC));

        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        public Either(TOptionA a) {

            this.m_a = a;
            this.IsFirst = true;

            this.m_b = default;
            this.IsSecond = false;

            this.m_c = default;
            this.IsThird = false;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="b"></param>
        public Either(TOptionB b) {

            this.m_b = b;
            this.IsSecond = true;

            this.m_a = default;
            this.IsFirst = false;

            this.m_c = default;
            this.IsThird = false;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="b"></param>
        public Either(TOptionC c) {

            this.m_c = c;
            this.IsThird = true;

            this.m_a = default;
            this.IsFirst = false;

            this.m_b = default;
            this.IsSecond = false;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ifNot"></param>
        /// <returns></returns>
        public TOptionA FirstOption(TOptionA ifNot) => this.IsFirst ? this.m_a : ifNot;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        public bool IfFirstOption(out TOptionA val) {
            if (this.IsFirst) {
                val = this.m_a;
                return true;
            }
            val = default;
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ifNot"></param>
        /// <returns></returns>
        public TOptionB SecondOption(TOptionB ifNot) => this.IsSecond ? this.m_b : ifNot;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        public bool IfSecondOption(out TOptionB val) {
            if (this.IsSecond) {
                val = this.m_b;
                return true;
            }
            val = default;
            return false;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="ifNot"></param>
        /// <returns></returns>
        public TOptionC ThirdOption(TOptionC ifNot) => this.IsThird ? this.m_c : ifNot;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        public bool IfThirdOption(out TOptionC val) {
            if (this.IsThird) {
                val = this.m_c;
                return true;
            }
            val = default;
            return false;
        }

        public static implicit operator TOptionA(Either<TOptionA, TOptionB, TOptionC> either)
            => either.IsFirst ? either.m_a : throw new InvalidOperationException($"Cannot return value of type '{typeof(TOptionA).Name}' when given value is of type '{either.ValueType.Name}'");

        public static implicit operator TOptionB(Either<TOptionA, TOptionB, TOptionC> either)
            => either.IsSecond ? either.m_b : throw new InvalidOperationException($"Cannot return value of type '{typeof(TOptionB).Name}' when given value is of type '{either.ValueType.Name}'");

        public static implicit operator TOptionC(Either<TOptionA, TOptionB, TOptionC> either)
            => either.IsThird ? either.m_c : throw new InvalidOperationException($"Cannot return value of type '{typeof(TOptionC).Name}' when given value is of type '{either.ValueType.Name}'");

        public static implicit operator Either<TOptionA, TOptionB, TOptionC>(TOptionA a)
            => new(a);

        public static implicit operator Either<TOptionA, TOptionB, TOptionC>(TOptionB b)
            => new(b);

        public static implicit operator Either<TOptionA, TOptionB, TOptionC>(TOptionC c)
            => new(c);

    }

}
