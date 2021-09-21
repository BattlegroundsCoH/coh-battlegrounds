using System;

namespace Battlegrounds.Networking {

    /// <summary>
    /// Class representing arguments to a <see cref="ObservableValueChangedHandler{T}"/> event.
    /// </summary>
    public class ObservableValueChangedEventArgs : EventArgs {

        /// <summary>
        /// Get the name of the changed property.
        /// </summary>
        public string Property { get; }

        /// <summary>
        /// Get if any value was  given when the property was changed.
        /// </summary>
        public bool HasValue { get; }

        /// <summary>
        /// Get the value that was changed to.
        /// </summary>
        public object Value { get; }

        /// <summary>
        /// Get or init if the observable value event is oriented towards the broker
        /// </summary>
        public bool IsBrokerEvent { get; init; }

        /// <summary>
        /// Create a new <see cref="ObservableValueChangedEventArgs"/> instance with only the name of the changed property.
        /// </summary>
        /// <param name="propName">The name of the property that was changed.</param>
        public ObservableValueChangedEventArgs(string propName) {
            this.Property = propName;
            this.HasValue = false;
        }

        /// <summary>
        /// Create a new <see cref="ObservableValueChangedEventArgs"/> instance with name and value of the changed property.
        /// </summary>
        /// <param name="propName">The name of the property that was changed.</param>
        /// <param name="value">The new value assigned to the changed property.</param>
        public ObservableValueChangedEventArgs(string propName, object value) : this(propName) {
            this.Value = value;
            this.HasValue = true;
        }

    }

    /// <summary>
    /// Delegate handler for handling observable value changes.
    /// </summary>
    /// <param name="sender">The observed object that triggered the handller.</param>
    /// <param name="e"></param>
    public delegate void ObservableValueChangedHandler<T>(T sender, ObservableValueChangedEventArgs e);

    /// <summary>
    /// Delegate handler for handing observable method invokations
    /// </summary>
    /// <param name="sender">The observed object that triggered the handler.</param>
    /// <param name="invokedMethod">The name of the invoked method.</param>
    /// <param name="args">The arguments given to the method.</param>
    public delegate void ObservableMethodInvokedHandler<T>(T sender, string invokedMethod, params object[] args);

    /// <summary>
    /// Interface for subscribing to obersvable eventso of network objects.
    /// </summary>
    /// <typeparam name="T">The network object to observe.</typeparam>
    public interface INetworkObjectObservable<T> {

        /// <summary>
        /// Event triggered when a network object value was changed.
        /// </summary>
        event ObservableValueChangedHandler<T> ValueChanged;

        /// <summary>
        /// Event triggered when a network object method is invoked.
        /// </summary>
        event ObservableMethodInvokedHandler<T> MethodInvoked;

    }

}
