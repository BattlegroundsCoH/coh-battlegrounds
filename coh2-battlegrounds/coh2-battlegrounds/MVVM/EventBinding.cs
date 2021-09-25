using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Windows;
using System.Windows.Markup;

using Battlegrounds.Functional;

namespace BattlegroundsApp.MVVM {

    /// <summary>
    /// A <see cref="MarkupExtension"/> which adds support for adding bindings of events to <see cref="EventCommand"/> properties.
    /// </summary>
    public class EventBinding : MarkupExtension {

        /// <summary>
        /// Get or set the name of the property to bind handler to.
        /// </summary>
        public string Handler { get; set; }

        public override object ProvideValue(IServiceProvider serviceProvider) {

            // Get target provider
            if (serviceProvider.GetService(typeof(IProvideValueTarget)) is not IProvideValueTarget targetProvider) {
                throw new InvalidOperationException();
            }

            // Make sure there's an object
            if (targetProvider.TargetObject is not FrameworkElement) {
                throw new InvalidOperationException();
            }

            // Get target property (target property on target object)
            if (targetProvider.TargetProperty is not EventInfo eventInfo) {
                throw new InvalidOperationException();
            }

            // Get event type
            if (eventInfo.EventHandlerType is not Type eventType) {
                return null;
            }

            // Get invoke method
            var invoke = eventType.GetMethod("Invoke");

            // Create invoker
            DynamicMethod method = new(string.Empty, invoke.ReturnType, invoke.GetParameters().Map(x => x.ParameterType));
            var body = method.GetILGenerator();
            body.Emit(OpCodes.Ldarg, 0); // Load first arg onto stack (the sender)
            body.Emit(OpCodes.Ldarg, 1); // Load second arg onto stack (the event arguments)
            body.Emit(OpCodes.Ldstr, this.Handler); // Push name of handler property to fetch on data context.
            body.Emit(OpCodes.Call, Exec); // Invoke our custom defined ExecuteEvent intermediary method.
            body.Emit(OpCodes.Ret); // Return (NOTE: This will likely cause unexpected behaviour (or crash) in case the event expects a return value).

            // Return method
            return method.CreateDelegate(eventType);

        }

        private static readonly MethodInfo Exec = typeof(EventBinding).GetMethod(nameof(ExecuteEvent));

        public static void ExecuteEvent(object sender, object eventArgs, string handler) {
            FrameworkElement senderFrameworkElement = sender as FrameworkElement;
            object dataContext = GetDataContext(senderFrameworkElement);
            if (dataContext is not null) {
                var propType = dataContext.GetType().GetProperty(handler, BindingFlags.Public | BindingFlags.Instance);
                if (propType.GetValue(dataContext) is EventCommand cmd) {
                    cmd.Exec(sender, eventArgs);
                }
            }
        }

        private static object GetDataContext(FrameworkElement element)
            => element.DataContext is object source ? source : GetDataContext(element.Parent as FrameworkElement);

    }

}
