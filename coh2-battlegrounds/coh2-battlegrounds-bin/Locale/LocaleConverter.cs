using System;

namespace Battlegrounds.Locale;

/// <summary>
/// Abstract class for implementing a locale converter.
/// </summary>
public abstract class LocaleConverter {

    /// <summary>
    /// Get the tyoe the converter is converting from.
    /// </summary>
    public abstract Type ConvertType { get; }

    /// <summary>
    /// Get the localised string representation of the given <paramref name="localeObject"/>.
    /// </summary>
    /// <param name="localize">The <see cref="Localize"/> instance to pull localised string data from.</param>
    /// <param name="localeObject">The object to localise.</param>
    /// <returns>The localised string representation of the <paramref name="localeObject"/>.</returns>
    public abstract string GetLocalisedString(Localize localize, object localeObject);

}

/// <summary>
/// Abstract class for implementing a locale converter for <typeparamref name="T"/> instances.
/// </summary>
/// <typeparam name="T">The type to convert from.</typeparam>
public abstract class LocaleConverter<T> : LocaleConverter {

    /// <summary>
    /// Get the <see cref="Type"/> representation of <typeparamref name="T"/>.
    /// </summary>
    public override Type ConvertType => typeof(T);

    /// <summary>
    /// Get the localised string representation of the given <paramref name="localeObject"/>.
    /// </summary>
    /// <param name="localize">The <see cref="Localize"/> instance to pull localised string data from.</param>
    /// <param name="localeObject">The object to localise.</param>
    /// <returns>The localised string representation of the <paramref name="localeObject"/>.</returns>
    /// <exception cref="ArgumentException"></exception>
    public sealed override string GetLocalisedString(Localize localize, object localeObject)
        => localeObject is T tObj ? GetLocalisedString(localize, tObj) : throw new ArgumentException($"Argument must be of type {typeof(T).Name}", nameof(localeObject));

    /// <summary>
    /// Get the localised string representation of the given <paramref name="localeObject"/>.
    /// </summary>
    /// <param name="localize">The <see cref="Localize"/> instance to pull localised string data from.</param>
    /// <param name="localeObject">The object to localise.</param>
    /// <returns>The localised string representation of the <paramref name="localeObject"/>.</returns>
    public abstract string GetLocalisedString(Localize localize, T localeObject);

}
