using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Battlegrounds.Game.Gameplay;

namespace Battlegrounds.Game.Match.Data.Events {

    public class SurrenderEvent : IMatchEvent {

        public char Identifier => 'S';

        public uint Uid { get; }

        public Player Player { get; }

        public SurrenderEvent(uint eventID, Player player) {
            this.Uid = eventID;
            this.Player = player;
        }

    }

}
