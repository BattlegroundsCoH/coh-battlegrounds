using System;
using System.Reflection.Emit;
using System.Reflection;
using System.Windows.Markup;
using System.Windows;

using Battlegrounds.ErrorHandling.CommonExceptions;
using Battlegrounds.Functional;

namespace Battlegrounds.UI;

/// <summary>
/// A <see cref="MarkupExtension"/> which adds support for adding bindings of events to <see cref="EventCommand"/> properties.
/// </summary>
public class EventBinding : MarkupExtension {

    /// <summary>
    /// Get or set the name of the property to bind handler to.
    /// </summary>
    public string Handler { get; set; } = string.Empty;

    public override object? ProvideValue(IServiceProvider serviceProvider) {

        // Get target provider
        if (serviceProvider.GetService(typeof(IProvideValueTarget)) is not IProvideValueTarget targetProvider) {
            throw new InvalidOperationException();
        }

        // Make sure there's an object
        if (targetProvider.TargetObject is not FrameworkElement) {
            throw new InvalidOperationException();
        }

        // if event
        if (targetProvider.TargetProperty is EventInfo eventInfo) {

            // Get event type
            if (eventInfo.EventHandlerType is not Type eventType) {
                return null;
            }

            // Get invoke method
            var invoke = eventType.GetMethod("Invoke") ?? throw new ObjectNotFoundException("Method 'Invoke' not found");

            // Return method
            return this.CreateDelegate(eventType, invoke.ReturnType, invoke.GetParameters().Map(x => x.ParameterType));

        } else if (targetProvider.TargetProperty is MethodInfo methodInfo) {

            // Grab parameters
            var methodParams = methodInfo.GetParameters();
            if (methodParams.Length != 2) {
                throw new NotSupportedException();
            }

            // Grab delegate data
            var delegateType = methodParams[1].ParameterType;
            var invoke = delegateType.GetMethod("Invoke") ?? throw new ObjectNotFoundException("Method 'Invoke' not found");

            // Grab delegate
            var del = this.CreateDelegate(delegateType, invoke.ReturnType, invoke.GetParameters().Map(x => x.ParameterType));

            // Invoke it
            methodInfo.Invoke(null, new object[] { targetProvider.TargetObject, del });

            // Return delegate
            return del;

        }

        var ty = targetProvider.TargetProperty.GetType();

        throw new InvalidOperationException();

    }

    private Delegate CreateDelegate(Type delegateType, Type returnType, Type[] parameters) {

        // Create invoker
        DynamicMethod method = new(string.Empty, returnType, parameters);
        var body = method.GetILGenerator();
        body.Emit(OpCodes.Ldarg, 0); // Load first arg onto stack (the sender)
        body.Emit(OpCodes.Ldarg, 1); // Load second arg onto stack (the event arguments)
        body.Emit(OpCodes.Ldstr, this.Handler); // Push name of handler property to fetch on data context.
        body.Emit(OpCodes.Call, Exec); // Invoke our custom defined ExecuteEvent intermediary method.
        body.Emit(OpCodes.Ret); // Return (NOTE: This will likely cause unexpected behaviour (or crash) in case the event expects a return value).

        // Return delegate to dynamic method
        return method.CreateDelegate(delegateType);

    }

    private static readonly MethodInfo Exec =
        typeof(EventBinding).GetMethod(nameof(ExecuteEvent)) ?? throw new ObjectNotFoundException($"Method '{nameof(ExecuteEvent)}' not found");

    public static void ExecuteEvent(object sender, object eventArgs, string handler) {
        object? dataContext = GetDataContext(sender as FrameworkElement);
        if (dataContext is not null) {
            var propType = dataContext.GetType().GetProperty(handler, BindingFlags.Public | BindingFlags.Instance);
            if (propType?.GetValue(dataContext) is EventCommand cmd) {
                cmd.Execute(sender, eventArgs);
            }
        }
    }

    private static object? GetDataContext(FrameworkElement? element) {
        if (element is FrameworkElement e) {
            return e.DataContext is object src ? src : GetDataContext(element.Parent as FrameworkElement);
        }
        return null;
    }

}
