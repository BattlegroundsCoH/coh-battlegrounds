using System;
using System.Windows;
using System.Windows.Threading;

using Battlegrounds.Functional;

namespace Battlegrounds.UI.Threading;

public static class ResultThreadHelper {

    public static Result<T?> ThenDispatch<T>(this Result<T?> result, Action<T?> action) => result.ThenDispatch(Application.Current.Dispatcher, action);

    public static Result<T?> ThenDispatch<T>(this Result<T?> result, Dispatcher dispatcher, Action<T?> action) => result.Then(x => dispatcher.Invoke(action, x));

    public static Result<T?> ElseDispatch<T>(this Result<T?> result, Action action) => result.ElseDispatch(Application.Current.Dispatcher, action);

    public static Result<T?> ElseDispatch<T>(this Result<T?> result, Dispatcher dispatcher, Action action) => result.Else(() => dispatcher.Invoke(action));

}
