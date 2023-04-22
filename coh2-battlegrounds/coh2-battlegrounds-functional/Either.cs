using System;
using System.Diagnostics.CodeAnalysis;

namespace Battlegrounds.Functional;

/// <summary>
/// Struct holding either a value of type <typeparamref name="TOptionA"/> or a value of type <typeparamref name="TOptionB"/>.
/// </summary>
/// <typeparam name="TOptionA">The first possible type.</typeparam>
/// <typeparam name="TOptionB">The second possible type.</typeparam>
public readonly struct Either<TOptionA, TOptionB> {

    private readonly TOptionA? m_a;
    private readonly TOptionB? m_b;

    /// <summary>
    /// Represents a <see cref="Either{TOptionA, TOptionB}"/> instance where neither type is set.
    /// </summary>
    public static readonly Either<TOptionA, TOptionB> Neither = new Either<TOptionA, TOptionB>(default, default);

    /// <summary>
    /// Get if the contained type is the first option.
    /// </summary>
    [MemberNotNullWhen(true, nameof(m_a))]
    public bool IsFirst { get; }

    /// <summary>
    /// Get if the contained type is the second option.
    /// </summary>
    [MemberNotNullWhen(true, nameof(m_b))]
    public bool IsSecond { get; }

    /// <summary>
    /// Get if either type value is present.
    /// </summary>
    public bool Any => this.IsFirst || this.IsSecond;

    /// <summary>
    /// Get the first case value directly.
    /// </summary>
    /// <exception cref="InvalidOperationException"/>
    public TOptionA First => this.IsFirst ? m_a : throw new InvalidOperationException("Either case is second case.");

    /// <summary>
    /// Get the second case value directly.
    /// </summary>
    /// <exception cref="InvalidOperationException"/>
    public TOptionB Second => this.IsSecond ? m_b : throw new InvalidOperationException("Either case is first case.");

    /// <summary>
    /// Initialise new <see cref="Either{TOptionA, TOptionB}"/> instance where type option is <typeparamref name="TOptionA"/>.
    /// </summary>
    /// <param name="a">The value of <typeparamref name="TOptionA"/>.</param>
    public Either(TOptionA a) {

        this.m_a = a;
        this.IsFirst = true;

        this.m_b = default;
        this.IsSecond = false;

    }

    /// <summary>
    /// Initialise new <see cref="Either{TOptionA, TOptionB}"/> instance where type option is <typeparamref name="TOptionB"/>.
    /// </summary>
    /// <param name="b">The value of <typeparamref name="TOptionB"/>.</param>
    public Either(TOptionB b) {

        this.m_b = b;
        this.IsSecond = true;

        this.m_a = default;
        this.IsFirst = false;

    }

    private Either(TOptionA? a, TOptionB? b) {
        this.m_a = a;
        this.m_b = b;
        this.IsSecond = false;
        this.IsFirst = false;
    }

    /// <summary>
    /// Get value of <typeparamref name="TOptionA"/> if value is present.
    /// </summary>
    /// <param name="ifNot"></param>
    /// <returns>Value of <typeparamref name="TOptionA"/> if set; otherwise <paramref name="ifNot"/>.</returns>
    public TOptionA FirstOption(TOptionA ifNot) => this.IsFirst ? this.m_a : ifNot;

    /// <summary>
    /// Get value of <typeparamref name="TOptionA"/> if value is present.
    /// </summary>
    /// <param name="val">The contained value.</param>
    /// <returns>if first option is set, <see langword="true"/>; Otherwise <see langword="false"/>.</returns>
    public bool IfFirstOption([NotNullWhen(true)] out TOptionA? val) {
        if (this.IsFirst) {
            val = this.m_a;
            return true;
        }
        val = default;
        return false;
    }

    /// <summary>
    /// Get value of <typeparamref name="TOptionB"/> if value is present.
    /// </summary>
    /// <param name="ifNot"></param>
    /// <returns>Value of <typeparamref name="TOptionB"/> if set; otherwise <paramref name="ifNot"/>.</returns>
    public TOptionB SecondOption(TOptionB ifNot) => this.IsSecond ? this.m_b : ifNot;

    /// <summary>
    /// Get value of <typeparamref name="TOptionB"/> if value is present.
    /// </summary>
    /// <param name="val">The contained value.</param>
    /// <returns>if second option is set, <see langword="true"/>; Otherwise <see langword="false"/>.</returns>
    public bool IfSecondOption([NotNullWhen(true)] out TOptionB? val) {
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
/// Struct holding either a value of type <typeparamref name="TOptionA"/>, value of type <typeparamref name="TOptionB"/>
/// or a value of type <typeparamref name="TOptionC"/>.
/// </summary>
/// <typeparam name="TOptionA">The first possible type.</typeparam>
/// <typeparam name="TOptionB">The second possible type.</typeparam>
/// <typeparam name="TOptionC">The third possible type.</typeparam>
public readonly struct Either<TOptionA, TOptionB, TOptionC> {

    private readonly TOptionA? m_a;
    private readonly TOptionB? m_b;
    private readonly TOptionC? m_c;

    /// <summary>
    /// Represents a <see cref="Either{TOptionA, TOptionB, TOptionC}"/> instance where neither type is set.
    /// </summary>
    public static readonly Either<TOptionA, TOptionB, TOptionC> None = new Either<TOptionA, TOptionB, TOptionC>(default, default, default);

    /// <summary>
    /// Get if the contained type is the first option.
    /// </summary>
    [MemberNotNullWhen(true, nameof(m_a))]
    public bool IsFirst { get; }

    /// <summary>
    /// Get if the contained type is the second option.
    /// </summary>
    [MemberNotNullWhen(true, nameof(m_b))]
    public bool IsSecond { get; }

    /// <summary>
    /// Get if the contained type is the third option.
    /// </summary>
    [MemberNotNullWhen(true, nameof(m_c))]
    public bool IsThird { get; }

    /// <summary>
    /// Get if any type value is present.
    /// </summary>
    public bool Any => this.IsFirst || this.IsSecond || this.IsThird;

    /// <summary>
    /// Get the <see cref="Type"/> instance being represented.
    /// </summary>
    public Type ValueType => this.IsFirst ? typeof(TOptionA) : (this.IsSecond ? typeof(TOptionB) : typeof(TOptionC));

    /// <summary>
    /// Initialise new <see cref="Either{TOptionA, TOptionB, TOptionC}"/> instance where type option is <typeparamref name="TOptionA"/>.
    /// </summary>
    /// <param name="a">The value of <typeparamref name="TOptionA"/>.</param>
    public Either(TOptionA a) {

        this.m_a = a;
        this.IsFirst = true;

        this.m_b = default;
        this.IsSecond = false;

        this.m_c = default;
        this.IsThird = false;

    }

    /// <summary>
    /// Initialise new <see cref="Either{TOptionA, TOptionB, TOptionC}"/> instance where type option is <typeparamref name="TOptionB"/>.
    /// </summary>
    /// <param name="b">The value of <typeparamref name="TOptionB"/>.</param>
    public Either(TOptionB b) {

        this.m_b = b;
        this.IsSecond = true;

        this.m_a = default;
        this.IsFirst = false;

        this.m_c = default;
        this.IsThird = false;

    }

    /// <summary>
    /// Initialise new <see cref="Either{TOptionA, TOptionB, TOptionC}"/> instance where type option is <typeparamref name="TOptionC"/>.
    /// </summary>
    /// <param name="c">The value of <typeparamref name="TOptionC"/>.</param>
    public Either(TOptionC c) {

        this.m_c = c;
        this.IsThird = true;

        this.m_a = default;
        this.IsFirst = false;

        this.m_b = default;
        this.IsSecond = false;

    }

    private Either(TOptionA? a, TOptionB? b, TOptionC? c) {
        this.m_a = a;
        this.m_b = b;
        this.m_c = c;
        this.IsSecond = false;
        this.IsFirst = false;
        this.IsThird = false;
    }

    /// <summary>
    /// Get value of <typeparamref name="TOptionA"/> if value is present.
    /// </summary>
    /// <param name="ifNot"></param>
    /// <returns>Value of <typeparamref name="TOptionA"/> if set; otherwise <paramref name="ifNot"/>.</returns>
    public TOptionA FirstOption(TOptionA ifNot) => this.IsFirst ? this.m_a : ifNot;

    /// <summary>
    /// Get value of <typeparamref name="TOptionA"/> if value is present.
    /// </summary>
    /// <param name="val">The contained value.</param>
    /// <returns>if first option is set, <see langword="true"/>; Otherwise <see langword="false"/>.</returns>
    public bool IfFirstOption([NotNullWhen(true)] out TOptionA? val) {
        if (this.IsFirst) {
            val = this.m_a;
            return true;
        }
        val = default;
        return false;
    }

    /// <summary>
    /// Get value of <typeparamref name="TOptionB"/> if value is present.
    /// </summary>
    /// <param name="ifNot"></param>
    /// <returns>Value of <typeparamref name="TOptionB"/> if set; otherwise <paramref name="ifNot"/>.</returns>
    public TOptionB SecondOption(TOptionB ifNot) => this.IsSecond ? this.m_b : ifNot;

    /// <summary>
    /// Get value of <typeparamref name="TOptionB"/> if value is present.
    /// </summary>
    /// <param name="val">The contained value.</param>
    /// <returns>if second option is set, <see langword="true"/>; Otherwise <see langword="false"/>.</returns>
    public bool IfSecondOption([NotNullWhen(true)] out TOptionB? val) {
        if (this.IsSecond) {
            val = this.m_b;
            return true;
        }
        val = default;
        return false;
    }


    /// <summary>
    /// Get value of <typeparamref name="TOptionC"/> if value is present.
    /// </summary>
    /// <param name="ifNot"></param>
    /// <returns>Value of <typeparamref name="TOptionC"/> if set; otherwise <paramref name="ifNot"/>.</returns>
    public TOptionC ThirdOption(TOptionC ifNot) => this.IsThird ? this.m_c : ifNot;

    /// <summary>
    /// Get value of <typeparamref name="TOptionC"/> if value is present.
    /// </summary>
    /// <param name="val">The contained value.</param>
    /// <returns>if third option is set, <see langword="true"/>; Otherwise <see langword="false"/>.</returns>
    public bool IfThirdOption([NotNullWhen(true)] out TOptionC? val) {
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
