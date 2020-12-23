using System;
using System.Windows.Controls;
using System.Windows.Threading;

namespace BattlegroundsApp {

    /// <summary>
    /// 
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public delegate bool StateChangeRequestHandler(object request);

    /// <summary>
    /// 
    /// </summary>
    public abstract class ViewState : UserControl {

        /// <summary>
        /// 
        /// </summary>
        public StateChangeRequestHandler StateChangeRequest { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public abstract void StateOnFocus();

        /// <summary>
        /// 
        /// </summary>
        public abstract void StateOnLostFocus();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <returns></returns>
        public virtual DispatcherOperation UpdateGUI(Action a) => this.Dispatcher.BeginInvoke(a);

    }

}
