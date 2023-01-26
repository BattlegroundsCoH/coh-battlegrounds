using System;

namespace Battlegrounds.Data.Generators.Lua.RuntimeServices;

/// <summary>
/// Lazy converter object that can trigger <see cref="LuaConverter.Write(LuaSourceBuilder, object)"/> at last possible moment.
/// </summary>
public sealed class LuaLazyConverter {

    private readonly LuaSourceBuilder _builder;
    private readonly LuaConverter _converter;
    private readonly object _obj;

    /// <summary>
    /// Initialise a new <see cref="LuaLazyConverter"/> that will store the required data for converting.
    /// </summary>
    /// <param name="builder">The builder to write to (This instance should not be passed between different <see cref="LuaSourceBuilder"/> instances).</param>
    /// <param name="converter">The converter instance to use.</param>
    /// <param name="target">The object to convert.</param>
    /// <exception cref="InvalidOperationException"/>
    public LuaLazyConverter(LuaSourceBuilder builder, LuaConverter converter, object target) {
        if (!converter.CanWrite(target.GetType())) {
            throw new InvalidOperationException($"Cannot use converter '{converter.GetType().FullName}' on type '{target.GetType().FullName}'.");
        }
        _builder = builder;
        _converter = converter;
        _obj = target;
    }

    /// <summary>
    /// Invoke the lazy converter.
    /// </summary>
    public void Convert() => _converter.Write(_builder, _obj);

}
