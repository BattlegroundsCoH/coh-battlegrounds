using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BattlegroundsApp {
    
    public interface IStateMachine<T> where T : IState {
    
        public T State { get; set; }

        void SetState(T state);

        bool StateChangeRequest(object request);

        StateChangeRequestHandler GetRequestHandler();

    }

}
