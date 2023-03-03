using System;
using System.Collections.Generic;
using System.Linq;

using Battlegrounds.Functional;

namespace Battlegrounds.Data;

/// <summary>
/// 
/// </summary>
public class Table : Dictionary<object, object?> {

    /// <summary>
    /// 
    /// </summary>
    public virtual int Size => this.Count;

    /// <summary>
    /// 
    /// </summary>
    public virtual IEnumerable<string> StringKeys => this.Keys.ToArray().MapNotNull(x => x.ToString());

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public virtual bool IsArray() {
        if (this.Keys.All(x => x is int)) {
            return true; // Should have a more thorough approach here...
        } else {
            return false;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="action"></param>
    public virtual void Pairs(Action<object, object?> action) {
        foreach (var (k,v) in this) {
            action(k, v);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <returns></returns>
    public T? As<T>(object key) {
        if (this.TryGetValue(key, out object? v) && v is T t) {
            return t;
        }
        return default;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <returns></returns>
    public T As<T>(object key, T def) {
        if (this.TryGetValue(key, out object? v) && v is T t) {
            return t;
        }
        return def;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <returns></returns>
    public T Get<T>(object key) where T : notnull {
        if (this.TryGetValue(key, out object? v) && v is T t) {
            return t;
        }
        throw new Exception();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public bool Contains(object key) => this.Keys.Any(x => x == key);

}
