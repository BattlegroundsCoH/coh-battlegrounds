namespace Battlegrounds.Models;

public delegate string ResolveLocaleString(uint key, params object[] args);

public readonly struct LocaleString(uint key, ResolveLocaleString resolver) {
    
    public static readonly LocaleString Empty = new(0, (_, _) => string.Empty);

    private readonly uint _key = key;
    private readonly ResolveLocaleString _resolver = resolver ?? throw new ArgumentNullException(nameof(resolver));

    public override string ToString() => $"${_key}";
    
    public string AsString(params object[] args) => _resolver?.Invoke(_key, args) ?? ToString();

    public static implicit operator string(LocaleString localeString) => localeString._resolver(localeString._key);
    public static implicit operator uint(LocaleString localeString) => localeString._key;

    public static LocaleString TempString(string value) {
        if (string.IsNullOrEmpty(value)) {
            throw new ArgumentException("Temporary strings must not be null or empty.", nameof(value));
        }
        return new LocaleString(uint.MaxValue, (_, _) => $"TEMP: {value}");
    }

    public static explicit operator LocaleString(string value) => TempString(value);

}
