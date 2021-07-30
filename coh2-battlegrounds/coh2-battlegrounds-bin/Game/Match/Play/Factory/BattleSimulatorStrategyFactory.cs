using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Battlegrounds.Game.Match.Play.Factory {

    public class BattleSimulatorStrategyFactory : IPlayStrategyFactory {

        public IPlayStrategy CreateStrategy(ISession session) => new BattleSimulatorStrategy(session);

    }

}
