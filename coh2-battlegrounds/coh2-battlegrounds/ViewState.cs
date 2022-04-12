using System;
using System.Windows.Controls;
using System.Windows.Threading;

using BattlegroundsApp.Modals;

namespace BattlegroundsApp {

    /// <summary>
    /// Requests a change in state based on a requesting object.
    /// </summary>
    /// <param name="request">The request object to use when determining what state should be changed to.</param>
    /// <returns>Will return <see langword="true"/> if the reuqested state was set as active. Otherwise <see langword="false"/>.</returns>
    public delegate bool StateChangeRequestHandler(object request);

    /// <summary>
    /// Rapresents a state in the <see cref="ViewStateMachine"/>. Inherits from <see cref="UserControl"/> and <see cref="IState"/>. Abstract class.
    /// </summary>
    public abstract class ViewState : ModalControl, IState {

        /// <summary>
        /// Gets or sets the method for handling state change requests.
        /// </summary>
        public StateChangeRequestHandler StateChangeRequest { get; set; }

        /// <summary>
        /// Inform the <see cref="ViewState"/> that focus was gained.
        /// </summary>
        public abstract void StateOnFocus();

        /// <summary>
        /// Inform the <see cref="ViewState"/> that focus was lost.
        /// </summary>
        public abstract void StateOnLostFocus();

        /// <summary>
        /// Call an <see cref="Action"/> on the GUI thread using the <see cref="ViewState"/>.
        /// </summary>
        /// <param name="a">The action to perform on the GUI thread.</param>
        /// <returns>
        /// An object, which is returned immediately after the function
        /// is called, that can be used to interact with the delegate as it is pending execution
        /// in the event queue.
        /// </returns>
        public virtual DispatcherOperation UpdateGUI(Action a) => this.Dispatcher.BeginInvoke(a);

    }

}
