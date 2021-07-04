using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Battlegrounds.Game.Match.Data.Events {
    public class SurrenderEvent : IMatchEvent {
        public char Identifier => 'S';
        public uint Uid { get; }
    }
}
