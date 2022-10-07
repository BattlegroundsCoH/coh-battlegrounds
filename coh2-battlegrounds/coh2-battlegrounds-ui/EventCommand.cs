using System;

namespace Battlegrounds.UI;

/// <summary>
/// Base class for an event command instance.
/// </summary>
/// <remarks>
/// Note: Does not implement <see cref="System.Windows.Input.ICommand"/>.
/// </remarks>
public class EventCommand {

    private readonly Action<object, object> m_action;

    /// <summary>
    /// Initialise a new <see cref="EventCommand"/> instance with specified <paramref name="action"/>.
    /// </summary>
    /// <param name="action">The method to invoke when event is triggered.</param>
    public EventCommand(Action<object, object> action) => this.m_action = action;

    /// <summary>
    /// Execute the command.
    /// </summary>
    /// <param name="sender">The sender who triggered the event.</param>
    /// <param name="args">The argument given to the event.</param>
    public void Execute(object sender, object args) => this.m_action?.Invoke(sender, args);

}

/// <summary>
/// Extension class of <see cref="EventCommand"/> accepting a specific <see cref="EventArgs"/> as argument type.
/// </summary>
/// <remarks>
/// Using this event command requires an event compliant with a (sender: object, arg: EventArgs) even handler type.
/// </remarks>
/// <typeparam name="T">The specific <see cref="EventArgs"/> type to use as argument.</typeparam>
public class EventCommand<T> : EventCommand where T : EventArgs {

    /// <summary>
    /// Initialise a new <see cref="EventCommand"/> instance with specified <paramref name="action"/>.
    /// </summary>
    /// <param name="action">The method to invoke when event is triggered.</param>
    public EventCommand(Action<object, T> action) : base((a, b) => action(a, (T)b)) { }

}
