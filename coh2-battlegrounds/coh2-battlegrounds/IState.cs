using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace BattlegroundsApp {
    
    /// <summary>
    /// 
    /// </summary>
    public interface IState {

        /// <summary>
        /// 
        /// </summary>
        StateChangeRequestHandler StateChangeRequest { get; set; }

        /// <summary>
        /// 
        /// </summary>
        void StateOnFocus();

        /// <summary>
        /// 
        /// </summary>
        void StateOnLostFocus();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <returns></returns>
        DispatcherOperation UpdateGUI(Action a);

    }

}
